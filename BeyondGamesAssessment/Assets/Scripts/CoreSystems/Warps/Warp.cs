using System;
using System.Collections;
using UI.Transitions;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

namespace CoreSystems.Warps
{
	[ExecuteAlways]
	public class Warp : MonoBehaviour
	{
		[Header("Warp Properties")]

		// ### Warp Point used for teleportation,
		// so the player will be teleported to the position of this point
		[SerializeField] private Transform warpPoint;

		// ### The destionation Warp's ID that this warp will teleport the player to
		[SerializeField] private string destinationWarpIdentifier;

		// ### The ID of this Warp
		[SerializeField] private string identifier = string.Empty;

		// ### Reference of the Particle System that activates after the Warp was used
		[SerializeField] private ParticleSystem particles = null;

		[Header("Fading Properties")]
		// ### The time that the Fader will use to Fade In
		[SerializeField] private float fadeInTime = 1f;

		// ### The time that the Fader will use to Fade Out
		[SerializeField] private float fadeOutTime = 1f;

		// ### Time time that the Fader will wait before Fading out
		[SerializeField] private float fadeWaitTime = 0.5f;

		[Header("Transitions")]
		// ### The reference of the Prefab for the Transition Canvas (or the Fader Canvas)
		[SerializeField] private Canvas transitionCanvasPrefab;

		[Header("Visibility")]
		// ### The cooldown time that will be countdown to make the warps disappear
		private float cooldown = 10f;

		// ### Used to check if the player already used this warp
		private bool hasTeleported = false;

		// ### The reference of the interactor that interacted with this warp
		// (preferencially the Player's Transform reference)
		private Transform interactor;

		/// <summary>
		/// When another object collides with this Warp, if the Player then 
		/// triggers the transition and the teleportation
		/// </summary>
		private void OnTriggerEnter(Collider other)
		{
			if(other == null)
			{
				Debug.LogError("Other is null.");
				return;
			}

			if (!other.CompareTag("Player"))
			{
				Debug.Log(other.transform.name + " Other is not a player.");
				return;
			}

			interactor = other.transform;

			WarpInteract();
		}

		/// <summary>
		/// Generates a new ID for this Warp
		/// </summary>
		[ContextMenu("Generate ID")]
		private void GenerateID()
		{
			identifier = Guid.NewGuid().ToString();
		}

		/// <summary>
		/// Starts the Transition and the Teleportation
		/// </summary>
		public void WarpInteract()
		{
			if (hasTeleported) return;

			hasTeleported = true;

			StopAllCoroutines();
			StartCoroutine(nameof(Transition));
		}

		/// <summary>
		/// Play some particles when the Warp was used (When it has the cooldown to disappear)
		/// </summary>
		public void PlayParticles()
		{
			particles?.Play();
		}

		/// <summary>
		/// Process the Transitions (Faders) and the Player's teleportation
		/// </summary>
		private IEnumerator Transition()
		{
			Fader fader = null;
			if (transitionCanvasPrefab != null)
			{
				GameObject faderGO = Instantiate(transitionCanvasPrefab.gameObject);
				fader = faderGO.GetComponent<Fader>();
				yield return fader.FadeIn(fadeInTime);
			}

			Warp otherWarp = GetOtherWarp();
			UpdatePlayer(otherWarp);

			if (fader != null)
			{
				yield return new WaitForSeconds(fadeWaitTime);
				yield return fader.FadeOut(fadeOutTime);

				Destroy(fader.gameObject);
			}

			PlayParticles();
			otherWarp.PlayParticles();

			Invoke(nameof(CooldownTime), cooldown);
		}

		/// <summary>
		/// Update the Player position based
		/// </summary>
		private void UpdatePlayer(Warp otherPortal)
		{
			if(interactor == null)
			{
				Debug.LogError("Interactor is null");
				return;
			}

			NavMeshAgent navAgent = interactor.GetComponent<NavMeshAgent>();
			bool hasNavMesh = navAgent != null && navAgent.enabled;

			if (hasNavMesh) navAgent.enabled = false;

			interactor.transform.position = otherPortal.warpPoint.position;
			interactor.transform.rotation = otherPortal.warpPoint.rotation;

			/*
			 Alternatively it could be used this:
				
				navAgent.Warp(otherPortal.warpPoint.position);
			 */

			if (hasNavMesh) navAgent.enabled = true;
		}

		/// <summary>
		/// Get's the Destination Warp Reference (Based on the IDs)
		/// </summary>
		private Warp GetOtherWarp()
		{
			Warp[] portals = FindObjectsOfType<Warp>();

			foreach (Warp portal in portals)
			{
				if (portal == this) continue;

				if (portal.identifier == destinationWarpIdentifier)
				{
					return portal;
				}
			}

			return null;
		}

		/// <summary>
		/// Deactivates this Warp and the Destination Warp
		/// </summary>
		private void CooldownTime()
		{
			var otherWarp = GetOtherWarp();

			if (otherWarp != null)
				otherWarp.gameObject.SetActive(false);

			gameObject.SetActive(false);
		}

#if UNITY_EDITOR
		/// <summary>
		/// Runs only on the Editor, it generates a new ID 
		/// in case that the ID (identifer) field is empty
		/// </summary>
		private void Update()
		{
			if (Application.IsPlaying(gameObject)) return;
			if (string.IsNullOrEmpty(gameObject.scene.path)) return;

			SerializedObject serializedObject = new SerializedObject(this);
			SerializedProperty property = serializedObject.FindProperty("identifier");

			if (string.IsNullOrEmpty(property.stringValue))
			{
				GenerateID();
			}
		}
#endif
	}
}