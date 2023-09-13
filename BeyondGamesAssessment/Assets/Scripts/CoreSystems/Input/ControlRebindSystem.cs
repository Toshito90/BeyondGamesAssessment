using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.InputSystem;
using TMPro;

namespace CoreSystems.InputSystem
{
    public class ControlRebindSystem
    {
        // ### Defines for name of the action map; the name of Player Prefs file
        private const string mainActionMapName = "Gameplay";

        // ### Defines the name of the save for PlayerPrefs
        private const string prefsSaveName = "ControlOverrides";

        // ### Reference of the Input Action Asset
        private InputActionAsset inputActionAsset;

        // ### Reference of the Input Reader Component
        private InputReader inputReader;

        // ### Operation of the Rebind Functionality
        private InputActionRebindingExtensions.RebindingOperation rebindingOperation;

        // ### Current Index of the Input in the Action
        private int currentIndex = 0;

		// ### Current Binding Index of the Input in the Action
		private int currentBindingIndex = 0;

        // ### Checks if the Player is binding the Key
        private bool isOnBindingKey = false;

        // ### Reference of the Text that will change to inform the Player when Rebinding
        private TextMeshProUGUI startRebindButton = null;

        // ### List of Actions
        private List<InputActionReference> keyActions;

        // ### Event that will be triggered when the Rebind is finished
        public event Action onFinishRebind;

        // ### Event that will be triggered when the Rebind system finished the Startup
        public event Action onFinishedStart;

        /// <summary>
        /// Loads all the necessary Data and Setup all the Keybinds
        /// </summary>
        public void Startup(InputReader inputReader, InputActionAsset inputAsset, out List<InputActionReference> inputActionReferenceList)
        {
            this.inputReader = inputReader;
            inputActionAsset = inputAsset;

            LoadBindings();

            keyActions = new List<InputActionReference>();

            PlayerInput playerInput = inputReader.GetComponent<PlayerInput>();
            var actionMaps = playerInput.actions.actionMaps;

            foreach (var actionMap in actionMaps)
            {
                var inputActions = actionMap.actions;
                foreach (var inputAction in inputActions)
                {
                    InputActionReference inputActionReference = new InputActionReference();
                    inputActionReference.Set(inputAction);
                    keyActions.Add(inputActionReference);
                }
            }

            inputActionReferenceList = keyActions;

            onFinishedStart?.Invoke();
        }

        /// <summary>
        /// Store the Control Path Overrides
        /// </summary>
        public void StoreControlOverrides()
        {
            //saving
            BindingWrapperClass bindingList = new BindingWrapperClass();
            foreach (var map in inputReader.PlayerInput.actions.actionMaps)
            {
                foreach (var binding in map.bindings)
                {
                    if (!string.IsNullOrEmpty(binding.overridePath))
                    {
                        bindingList.bindingList.Add(new BindingSerializable(binding.id.ToString(), binding.overridePath));
                    }
                }
            }

            PlayerPrefs.SetString(prefsSaveName, JsonUtility.ToJson(bindingList));
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Loads the Control Path Overrides
        /// </summary>
        public void LoadControlOverrides()
        {
            if (PlayerPrefs.HasKey(prefsSaveName))
            {
                BindingWrapperClass bindingList = JsonUtility.FromJson(PlayerPrefs.GetString(prefsSaveName), typeof(BindingWrapperClass)) as BindingWrapperClass;

                //create a dictionary to easier check for existing overrides
                Dictionary<System.Guid, string> overrides = new Dictionary<System.Guid, string>();
                foreach (var item in bindingList.bindingList)
                {
                    overrides.Add(new System.Guid(item.id), item.path);
                }

                //walk through action maps check dictionary for overrides
                foreach (var map in inputReader.PlayerInput.actions.actionMaps)
                {
                    var bindings = map.bindings;
                    for (var i = 0; i < bindings.Count; ++i)
                    {
                        if (overrides.TryGetValue(bindings[i].id, out string overridePath))
                        {
                            //if there is an override apply it
                            map.ApplyBindingOverride(i, new InputBinding { overridePath = overridePath });
                        }
                    }
                }
            }
            else
            {
                // Create, Reset and Save then recurssively load the file until it can read the Prefs
                SaveBindings();
                LoadControlOverrides();
            }
        }

        /// <summary>
        /// Save the Keybinds
        /// </summary>
        public void SaveBindings()
        {
            StoreControlOverrides();
        }

        /// <summary>
        /// Load the Keybinds
        /// </summary>
        public void LoadBindings()
        {
            LoadControlOverrides();
        }

        /// <summary>
        /// Starts the Process to Rebind the Keys
        /// </summary>
        public void StartRebinding(TextMeshProUGUI startRebindButton, int buttonIndex, int buttonBindingIndex)
        {
            if (isOnBindingKey)
            {
                // Avoid double Rebinding process, so it can also avoid memory leak and other issues related with the input system
                CancelBindings();
            }

            isOnBindingKey = true;

            inputReader.SetActive(false);

            inputReader.PlayerInput.SwitchCurrentActionMap("Rebinding");

            currentIndex = buttonIndex;
            currentBindingIndex = buttonBindingIndex;

            this.startRebindButton = startRebindButton;
            this.startRebindButton.text = "Waiting for Input...";


            rebindingOperation = keyActions[currentIndex].action.PerformInteractiveRebinding(currentBindingIndex)
                .WithControlsExcluding("<Mouse>/leftButton")
                .WithControlsExcluding("<Mouse>/rightButton")
                .WithControlsExcluding("<Mouse>/press")
                .WithControlsExcluding("<Pointer>/position")
                .WithCancelingThrough("<Keyboard>/escape")
                .OnMatchWaitForAnother(0.1f)
                .OnCancel(operation =>
                {
                    RebindComplete();
                    CleanUp();
                })
               .OnComplete(operation =>
               {
                   if (CheckIfDuplicated(keyActions[currentIndex].action))
                   {
                       ClearDuplicatedOverride();
                   }

                   RebindComplete();
                   CleanUp();
               }).Start();
        }

        /// <summary>
        /// Called after the Rebind was complete
        /// </summary>
        private void RebindComplete()
        {
            inputReader.PlayerInput.SwitchCurrentActionMap("Gameplay");
            inputReader.PlayerInput.enabled = true;
 
            isOnBindingKey = false;

            inputReader.SetActive(true);

            UpdateUI();

            inputReader.StartCoroutine(WaitCompleteRebind());

            if (onFinishRebind != null) onFinishRebind();
        }

        /// <summary>
        /// Checks if there is any keybind that is repeated
        /// </summary>
        private bool CheckIfDuplicated(UnityEngine.InputSystem.InputAction action)
        {
            if (action == null) return false;

            if (action.bindings[0] == action.bindings[1])
            {
                return true;
            }

            foreach (InputBinding binding in action.actionMap.bindings)
            {
                // Don't try to search on the same action
                if (binding.action == action.bindings[0].action || binding.action == action.bindings[1].action)
                {
                    continue;
                }

                if (binding.effectivePath == action.bindings[0].effectivePath
                    || binding.effectivePath == action.bindings[1].effectivePath)
                {
                    inputReader.PlayerInput.SwitchCurrentActionMap(mainActionMapName);
                    inputReader.SetActive(true);
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Clears the Duplicated Keybind
        /// </summary>
        void ClearDuplicatedOverride()
        {
            if (keyActions[currentIndex].action.bindings[currentBindingIndex] == null ||
                keyActions[currentIndex].action.bindings[currentBindingIndex].effectivePath == "" ||
                keyActions[currentIndex].action.bindings[currentBindingIndex].effectivePath == "N/A")
            {
                return;
            }

            if (keyActions[currentIndex].action.bindings[0].effectivePath == keyActions[currentIndex].action.bindings[1].effectivePath)
            {
                if (currentBindingIndex == 0)
                {
                    keyActions[currentIndex].action.ApplyBindingOverride(1, "N/A");
                }
                else
                {
                    keyActions[currentIndex].action.ApplyBindingOverride(0, "N/A");
                }
            }

            for (int i = 0; i < keyActions.Count; i++)
            {
                if (i == currentIndex) continue;

                if (keyActions[i].action.bindings[0] != null
                    && keyActions[i].action.bindings[0].effectivePath != ""
                    && keyActions[i].action.bindings[0].effectivePath != "N/A")
                {
                    if (keyActions[currentIndex].action.bindings[currentBindingIndex].effectivePath == keyActions[i].action.bindings[0].effectivePath)
                    {
                        keyActions[i].action.ApplyBindingOverride(0, "N/A");
                    }
                }

                if (keyActions[i].action.bindings.Count < 2) continue;

                if (keyActions[i].action.bindings[1] != null
                   && keyActions[i].action.bindings[1].effectivePath != ""
                   && keyActions[i].action.bindings[0].effectivePath != "N/A")
                {
                    if (keyActions[currentIndex].action.bindings[currentBindingIndex].effectivePath == keyActions[i].action.bindings[1].effectivePath)
                    {
                        keyActions[i].action.ApplyBindingOverride(1, "N/A");
                    }
                }
            }
        }

        /// <summary>
        /// Resets the Keybinds to the original state
        /// </summary>
        public void ResetToDefault()
        {
            foreach (InputActionMap map in inputActionAsset.actionMaps)
            {
                map.RemoveAllBindingOverrides();
            }

            if (PlayerPrefs.HasKey(prefsSaveName))
            {
                PlayerPrefs.DeleteKey(prefsSaveName);
            }

            if (onFinishRebind != null) onFinishRebind();
        }

        /// <summary>
        /// Contains the Functionality of the Cancel Button when trying to Rebind
        /// </summary>
        public void CancelButton()
        {
            if (isOnBindingKey)
            {
                CancelBindings();
                isOnBindingKey = false;
            }

            if (onFinishRebind != null) onFinishRebind();
        }

        /// <summary>
        /// Returns the Rebind Operation
        /// </summary>
        public InputActionRebindingExtensions.RebindingOperation GetRebindOperation()
        {
            return rebindingOperation;
        }

        /// <summary>
        /// Gets the Key Actions List
        /// </summary>
        public IEnumerable<InputActionReference> GetKeyActions()
        {
            foreach (var keyAction in keyActions)
            {
                yield return keyAction;
            }
        }

        /// <summary>
        /// Used to stop the process of another binding or just closing window UI
        /// </summary>
        private void CancelBindings()
        {
            rebindingOperation.Cancel();
        }

        /// <summary>
        /// Updates the Keybind/Rebind Button Text
        /// </summary>
        private void UpdateUI()
        {
            if (currentIndex >= keyActions.Count)

                startRebindButton.text = InputControlPath.ToHumanReadableString(
                    keyActions[currentIndex].action.bindings[currentBindingIndex].effectivePath,
                    InputControlPath.HumanReadableStringOptions.OmitDevice);
        }

        /// <summary>
        /// Cleans up the Rebind Operation to avoid Memory Leak
        /// </summary>
        private void CleanUp()
        {
            rebindingOperation.Dispose();
        }

        /// <summary>
        /// Check if the Player is Rebinding any Keybind at the moment
        /// </summary>
        public bool IsRebinding()
        {
            return isOnBindingKey;
        }

        /// <summary>
        /// Waits around 3 frames to complete the Rebind
        /// </summary>
        IEnumerator WaitCompleteRebind()
        {
            yield return new WaitForFixedUpdate();
            yield return new WaitForFixedUpdate();
            yield return new WaitForEndOfFrame();
        }

		[Serializable]
		class BindingWrapperClass
		{
			public List<BindingSerializable> bindingList = new List<BindingSerializable>();
		}

		[Serializable]
		private struct BindingSerializable
		{
			public string id;
			public string path;

			public BindingSerializable(string bindingId, string bindingPath)
			{
				id = bindingId;
				path = bindingPath;
			}
		}
	}
}