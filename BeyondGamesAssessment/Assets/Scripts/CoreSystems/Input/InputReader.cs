using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace CoreSystems.InputSystem
{
	public class InputReader : MonoBehaviour//, InputActions.IGameplayActions
	{
		// ### Reference of the Input Actions Map
		//InputActions inputActions;

		// ### List of the Input Actions
		List<InputActionReference> inputActionReferences = new List<InputActionReference>();

		// ### Reference of the Player Input Component
		public PlayerInput PlayerInput { get; private set; }

		// ### Reference of the Control Rebind System
		public ControlRebindSystem ControlRebindSystem { get; private set; } = null;

		// ### Event called when Zoom In Camera Action is executed
		public event Action onCameraZoomIn;

		// ### Event called when Zoom Out Camera Action is executed
		public event Action onCameraZoomOut;

		/// <summary>
		/// Get's hold of the Reference of the Player Input Component
		/// </summary>
		private void Awake()
		{
			PlayerInput = GetComponent<PlayerInput>();
		}

		/// <summary>
		/// Create an instance of the Input Action Reference and Setup the necessary configurations
		/// </summary>
		private void Start()
		{
			/*inputActions = new InputActions();
			inputActions.Gameplay.SetCallbacks(this);

			inputActions.Gameplay.Enable();*/

			ControlRebindSystem = new ControlRebindSystem();
			ControlRebindSystem.Startup(this, PlayerInput.actions, out inputActionReferences);
		}

		/// <summary>
		/// When Destroying it is safe to disable the Input Actions
		/// </summary>
		private void OnDestroy()
		{
			//inputActions.Gameplay.Disable();
		}

		/// <summary>
		/// Enable/Disable the Input Actions
		/// </summary>
		public void SetActive(bool isActive)
		{
			/*if (isActive)
				inputActions.Gameplay.Enable();
			else
				inputActions.Gameplay.Disable();*/
		}

		/// <summary>
		/// Starts the Rebind Process
		/// </summary>
		public void StartRebinding(TextMeshProUGUI text, int actionIndex, int index)
		{
			ControlRebindSystem.StartRebinding(text, actionIndex, index);
		}

		/// <summary>
		/// Reset the Keybinds to their original state (Eliminating the Path overrides)
		/// </summary>
		public void ResetKeybindsToDefault()
		{
			ControlRebindSystem.ResetToDefault();
		}

		/// <summary>
		/// Save the Keybinds
		/// </summary>
		public void SaveKeybinds()
		{
			ControlRebindSystem.SaveBindings();
		}


		/// <summary>
		/// Called when the Camera Zoom In Action is Executed
		/// </summary>
		public void OnCameraZoomIn(UnityEngine.InputSystem.InputAction.CallbackContext context)
		{
			if (!context.performed)
				return;

			onCameraZoomIn?.Invoke();
		}

		/// <summary>
		/// Called when the Camera Zoom Out Action is Executed
		/// </summary>
		public void OnCameraZoomOut(UnityEngine.InputSystem.InputAction.CallbackContext context)
		{
			if (!context.performed)
				return;

			onCameraZoomOut?.Invoke();
		}
	}
}