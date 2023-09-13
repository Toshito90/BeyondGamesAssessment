using CoreSystems.Extensions.Attributes;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Contents.General
{
	public class SceneChanger : MonoBehaviour
	{
		// ### Reference of the Scene that the player should go
		[SerializeField, Scene] string destinationScene;

		/// <summary>
		/// Change Scene, it has an option to have an override argument
		/// </summary>
		public void ChangeScene(string scene = "")
		{
			if (string.IsNullOrEmpty(scene) || string.IsNullOrWhiteSpace(scene))
			{
				var finalScene = GetScene(scene);
				if (string.IsNullOrEmpty(finalScene.name) || string.IsNullOrWhiteSpace(finalScene.name))
					return;
					
				SceneManager.LoadScene(finalScene.name);
				return;
			}

			ChangeScene();
		}

		/// <summary>
		/// Overloaded version of the ChangeScene, uses directly the destinationScene field
		/// </summary>
		public void ChangeScene()
		{
			var finalScene = GetSceneName(this.destinationScene);
			if (string.IsNullOrEmpty(finalScene) || string.IsNullOrWhiteSpace(finalScene))
				return;

			SceneManager.LoadScene(finalScene);
		}

		/// <summary>
		/// Get's only the name of the Scene
		/// </summary>
		public static string GetSceneName(string fullPath)
		{
			return Path.GetFileNameWithoutExtension(fullPath);
		}

		/// <summary>
		/// Get's the Scene Reference if it exists on the Project
		/// </summary>
		public static Scene GetScene(string fullPath)
		{
			var sceneName = GetSceneName(fullPath);
			var scene = SceneManager.GetSceneByName(sceneName);
			return scene;
		}
	}
}