using CoreSystems.InputSystem;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace UI.InputSystem
{
	public class KeybindsOption : MonoBehaviour
	{
		// ### Reference of the Text that will contain the Name of the Input Action
		[SerializeField] TextMeshProUGUI actionName;

		// ### Button that triggers the Rebind functionality
		[SerializeField] Button rebindButton;

		/// <summary>
		/// Setup the Text to have the name of the Action and the button to rebind
		/// </summary>
		public void Setup(InputAction inputAction, InputReader inputReader, int index)
		{
			actionName.text = inputAction.name;

			var buttonText = rebindButton.GetComponentInChildren<TextMeshProUGUI>();
			buttonText.text = InputControlPath.ToHumanReadableString(
						inputAction.bindings[0].effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);

			InputActionReference inpu = new InputActionReference();
			inpu.Set(inputAction);

			if (inputAction.bindings.Count == 0 || inputAction.bindings[0].effectivePath == null || inputAction.bindings[0].effectivePath == "" || inputAction.bindings[0].effectivePath == "N/A")
			{
				buttonText.text = " - ";
			}

			rebindButton.onClick.AddListener(() =>
			{
				RebindAction(buttonText, inputReader, index, 0);
			});
		}

		/// <summary>
		/// Calls the Rebind functionality
		/// </summary>
		void RebindAction(TextMeshProUGUI text, InputReader inputReader, int actionIndex, int bindingIndex)
		{
			inputReader.StartRebinding(text, actionIndex, bindingIndex);
		}
	}
}