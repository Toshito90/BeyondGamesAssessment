using CoreSystems.InputSystem;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace UI.InputSystem
{
	public class KeybindsSettings : MonoBehaviour
	{
		// ### Reference of the Parent where the options for Rebinds should be instantiated
		[SerializeField] Transform content;

		// ### Reference of the original Prefab of the Options for the Rebinds
		[SerializeField] KeybindsOption keybindsOptionPrefab;

		// ### Reference of the Player Input Component
		[SerializeField] PlayerInput playerInput;

		// ### Reference of the InputReader Component
		InputReader inputReader;

		/// <summary>
		/// When visible Setup the UI
		/// </summary>
		private void OnEnable()
		{
			Setup();
		}

		/// <summary>
		/// Gets the InputReader reference and subscribe the event for when the Rebind is finished
		/// </summary>
		public void Setup()
		{
			inputReader = playerInput.GetComponent<InputReader>();

			UpdateUI();

			inputReader.ControlRebindSystem.onFinishRebind += UpdateRebind;
		}


		/// <summary>
		/// Un-Subscribe the event for when the Rebind is Finished
		/// </summary>
		private void OnDisable()
		{
			if (inputReader == null) return;
			if (inputReader.ControlRebindSystem == null) return;
			inputReader.ControlRebindSystem.onFinishRebind -= UpdateRebind;
		}

		/// <summary>
		/// Un-Subscribe the event for when the Rebind is Finished
		/// </summary>
		private void OnDestroy()
		{
			if (inputReader == null) return;
			if (inputReader.ControlRebindSystem == null) return;
			inputReader.ControlRebindSystem.onFinishRebind -= UpdateRebind;
		}

		/// <summary>
		/// Updates the Information with the correct Keybinds
		/// </summary>
		void UpdateRebind()
		{
			UpdateUI();
		}

		/// <summary>
		/// Refreshes the Options with the updated Keybinds
		/// </summary>
		void UpdateUI()
		{
			foreach (Transform obj in content)
			{
				Destroy(obj.gameObject);
			}

			var actionMaps = playerInput.actions.actionMaps;

			foreach (var actionMap in actionMaps)
			{
				var inputActions = actionMap.actions;
				int index = 0;
				foreach (var inputAction in inputActions)
				{
					var keybindsOption = Instantiate(keybindsOptionPrefab, content);
					keybindsOption.Setup(inputAction, inputReader, index);
					index++;
				}
			}
		}
	}
}