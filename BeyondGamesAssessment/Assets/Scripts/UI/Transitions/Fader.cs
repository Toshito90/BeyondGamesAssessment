using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace UI.Transitions
{
	public class Fader : MonoBehaviour
	{
		// ### Reference of the CanvasGroup Component
		CanvasGroup canvasGroup;

		/// <summary>
		/// Get's the Reference of the Canvas Group Component
		/// </summary>
		void Awake()
		{
			canvasGroup = GetComponent<CanvasGroup>();
		}

		/// <summary>
		/// Process the Animations of the Fade Out and the Fade Is
		/// </summary>
		public IEnumerator FadeOutIn(float _timeFadeIn = 1f, float _timeFadeOut = 1f)
		{
			yield return FadeIn(_timeFadeIn);
			yield return FadeOut(_timeFadeOut);
		}

		/// <summary>
		/// Process the Animation for Fade In
		/// </summary>
		public IEnumerator FadeIn(float _time)
		{
			while (canvasGroup.alpha < 1f)
			{
				canvasGroup.alpha += Time.deltaTime / _time;

				yield return null;
			}
		}

		/// <summary>
		/// Process the Animation for Fade Out
		/// </summary>
		public IEnumerator FadeOut(float _time)
		{
			while (canvasGroup.alpha > 0f)
			{
				canvasGroup.alpha -= Time.deltaTime / _time;

				yield return null;
			}
		}
	}
}