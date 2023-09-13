using CoreSystems.Extensions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Analytics;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Contents.MainMenu
{
	public class MainMenuBehavior : MonoBehaviour
	{
		// ### Define the speed of the Fade In/Fade Out of the Text
		[SerializeField] private float speed = 1f;

		// ### Reference of the Canvas Group of the "Press Space" Text
		[SerializeField] private CanvasGroup textCanvasGroup = null;

		// ### Define the Easing Animation, you can check it here further: https://easings.net/
		[SerializeField] private EasingFunction.Ease easingMode;

		// ### Reference of the Button to Exit the Game
		[SerializeField] private Button exitGameButton = null;

		/// <summary>
		/// Starts the Fading Animation of the Text
		/// </summary>
		private void Start()
		{
			if(exitGameButton != null)
			{
				exitGameButton.onClick.RemoveAllListeners();
				exitGameButton.onClick.AddListener(ExitGame);
			}

			StartCoroutine(nameof(Fading));
		}

		/// <summary>
		/// Checks if the player presses the Space Key
		/// </summary>
		private void Update()
		{
			if (Input.GetKeyDown(KeyCode.Space))
			{
				SceneManager.LoadScene(1);
			}
		}

		/// <summary>
		/// Exits the Game to the Desktop
		/// </summary>
		private void ExitGame()
		{
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false; // Stop play mode in the Editor
#else
			Application.Quit(); // Quit the application in build mode
#endif
		}

		/// <summary>
		/// Does the Fading Animation Process
		/// </summary>
		private IEnumerator Fading()
		{
			float progress = 0f;
			float percentage = 0f;

			var easeFunction = EasingFunction.GetEasingFunction(easingMode);

			bool isFadingIn = false;

			while (true)
			{
				progress += Time.deltaTime;
				percentage = progress / speed;

				if (isFadingIn)
					textCanvasGroup.alpha = easeFunction(0f, 1f, percentage);
				else
					textCanvasGroup.alpha = easeFunction(1f, 0f, percentage);

				if (percentage >= 1f)
				{
					isFadingIn = !isFadingIn;
					progress = 0f;
					percentage = 0f;
				}
	
				yield return null;
			}
		}
	}
}