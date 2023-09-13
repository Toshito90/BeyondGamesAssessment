using Cinemachine;
using CoreSystems.InputSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystems.Cameras
{
	public class CameraFunctionalities : MonoBehaviour
	{
		// ### Reference of the Virtual Camera
		[SerializeField] private CinemachineVirtualCamera virtualCamera;

		// ### The minimum Value that the Camera can Zoom In
		[SerializeField] private float minFOV = 5f;

		// ### The maximum Value that the Camera can Zoom Out
		[SerializeField] private float maxFOV = 10f;

		// ### The speed of the Zoom
		[SerializeField] private float zoomSpeed = 2f;

		// ### Reference of the Input Reader Component
		[SerializeField] private InputReader inputReader = null;

		/// <summary>
		/// Subscribe the Events of the Input Reader Component
		/// </summary>
		private void OnEnable()
		{
			if (inputReader == null)
			{
				Debug.LogError("There is no Input Reader attached to the Camera Functionalities.");
				return;
			}

			inputReader.onCameraZoomIn += CameraZoomIn;
			inputReader.onCameraZoomOut += CameraZoomOut;
		}

		/// <summary>
		/// Un-Subscribe the Events of the Input Reader Component
		/// </summary>
		private void OnDisable()
		{
			if (inputReader == null)
			{
				Debug.LogError("There is no Input Reader attached to the Camera Functionalities.");
				return;
			}

			inputReader.onCameraZoomIn -= CameraZoomIn;
			inputReader.onCameraZoomOut -= CameraZoomOut;
		}

		/// <summary>
		/// Zooms In the Camera
		/// </summary>
		public void CameraZoomIn()
		{
			float currentFOV = virtualCamera.m_Lens.FieldOfView;
			currentFOV -= zoomSpeed;
			currentFOV = Mathf.Clamp(currentFOV, minFOV, maxFOV);
			virtualCamera.m_Lens.FieldOfView = currentFOV;
		}

		/// <summary>
		/// Zooms Out the Camera
		/// </summary>
		public void CameraZoomOut()
		{
			float currentFOV = virtualCamera.m_Lens.FieldOfView;
			currentFOV += zoomSpeed;
			currentFOV = Mathf.Clamp(currentFOV, minFOV, maxFOV);
			virtualCamera.m_Lens.FieldOfView = currentFOV;
		}
	}
}