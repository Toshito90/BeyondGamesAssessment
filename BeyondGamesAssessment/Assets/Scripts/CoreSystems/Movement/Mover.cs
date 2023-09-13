using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace CoreSystems.Movement
{
	public class Mover : MonoBehaviour
	{
		// ### Enable/Disable the flag to make the Agent rotate instantly when moving
		[SerializeField] private bool instantRotation = false;

		// ### Reference of the Particle System that appears when the Player click to move
		[SerializeField] private ParticleSystem clickParticle = null;

		// ### References of the Animators that are in each wheel
		[SerializeField] private Animator[] wheelsAnimators;

		// ### The Hash ID of the desired Trigger to trigger the movement wheels animation
		private readonly int animationMoveHash = Animator.StringToHash("MoveTrigger");

		// ### Reference of the NavMeshAgent component
		private NavMeshAgent navMeshAgent = null;

		// ### Disable/Enable the movement functionality in terms of input (Mouse Click)
		private bool disableInput = false;

		// ### Reference of the Particle created when the Player clicked on the ground
		ParticleSystem particleInstanciated = null;

		/// <summary>
		/// At the start get the references of the necessary components (NavMeshAgent on this case)
		/// </summary>
		private void Awake()
		{
			navMeshAgent = GetComponent<NavMeshAgent>();
		}
		
		/// <summary>
		/// For each frame it checks if the player pressed the left mouse key to move, 
		/// also checks of the input is enabled and if it should rotate instantly
		/// </summary>
		private void Update()
		{
			if (disableInput) return;

			RotateInstant();

			if (Input.GetMouseButtonDown(0))
			{
				if (IsEditingInputField()) return;

				if (EventSystem.current.IsPointerOverGameObject())
				{
					navMeshAgent.ResetPath();
					return;
				}

				Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
				RaycastHit hit;

				if (Physics.Raycast(ray, out hit))
				{
					MoveToPoint(hit.point);
				}
			}

			// ### Fix Hack to stop the Agent rotating like mad when it reaches the destination point
			if (navMeshAgent.remainingDistance <= navMeshAgent.stoppingDistance)
			{
				navMeshAgent.isStopped = true;
				RebindAnimators();

				if (particleInstanciated != null)
					Destroy(particleInstanciated.gameObject);
			}
		}

		/// <summary>
		/// Enable or Disable the Input (Used for situations like cinematics or other events 
		/// that require the player to be in place).
		/// </summary>
		public void DisableInput(bool toDisable)
		{
			disableInput = toDisable;
		}

		/// <summary>
		/// Checks if the point in the world is recheable in the navmesh 
		/// and if so moves the agent to the desired point in the world
		/// </summary>
		public bool MoveToPoint(Vector3 point)
		{
			// ### if the Point is not recheable in the terrain then it should do nothing
			if (!NavMesh.SamplePosition(point, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
				return false;

			navMeshAgent.isStopped = false;
			navMeshAgent.SetDestination(point);

			SetMoveAnimation();

			if(particleInstanciated != null)
				Destroy(particleInstanciated.gameObject);

			particleInstanciated = Instantiate(clickParticle);
			particleInstanciated.transform.position = point;

			return true;
		}

		/// <summary>
		/// Checks if the Input is Enabled/Disabled
		/// </summary>
		public bool IsInputDisabled()
		{
			return disableInput;
		}

		/// <summary>
		/// Check if the player is pressing any key when the focus is on an input Box, 
		/// so instead of moving, open menus, or doing other event it will try to write 
		/// on the input box instead
		/// </summary>
		public bool IsEditingInputField()
		{
			GameObject currentFocus = EventSystem.current.currentSelectedGameObject;
			if (currentFocus == null) return false;

			if (currentFocus.TryGetComponent(out InputField _))
			{
				return true;
			}

			if (currentFocus.TryGetComponent(out TMP_InputField _))
			{
				return true;
			}

			return false;
		}

		/// <summary>
		/// Makes the Agent rotate instantly when moving if the instantRotation field is true
		/// </summary>
		public void RotateInstant()
		{
			if (!instantRotation) return;

			// Rotate instantly when changing direction
			if (navMeshAgent.velocity.sqrMagnitude > Mathf.Epsilon)
			{
				transform.rotation = Quaternion.LookRotation(navMeshAgent.velocity.normalized);
			}
		}

		/// <summary>
		/// Resets all the animations
		/// </summary>
		private void RebindAnimators()
		{
			foreach (var wheelAnimator in wheelsAnimators)
			{
				wheelAnimator.Rebind();
			}
		}

		/// <summary>
		/// Sets the trigger animation to move the wheels
		/// </summary>
		private void SetMoveAnimation()
		{
			foreach (var wheelAnimator in wheelsAnimators)
			{
				wheelAnimator.SetTrigger(animationMoveHash);
			}
		}
	}
}