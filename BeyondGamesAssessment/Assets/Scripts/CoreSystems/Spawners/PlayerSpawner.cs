using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace CoreSystems.Spawners
{
	public class PlayerSpawner : MonoBehaviour
	{
		// ### Should it spawn on Start
		[SerializeField] private bool spawnOnStart = true;

		// ### The Reference of the Player (On this case the Core Game Object)
		[SerializeField] private Transform playerPrefab = null;

		// ### The Array of Spawn Points to be used as positions to teleport the player to
		[SerializeField] private Transform[] spawnPoints = new Transform[0];

		/// <summary>
		/// If the spawnOnStart field is true then Spawn the Player on Start
		/// </summary>
		private void Start()
		{
			if (!spawnOnStart)
				return;

			SpawnPlayer();
		}

		/// <summary>
		/// Spawns the Player and position it to a random position
		/// </summary>
		public void SpawnPlayer()
		{
			if (spawnPoints.Length == 0)
			{
				Debug.LogError("No spawn points available. Please assign spawn points in the Inspector.");
				return;
			}

			int randomPoint = Random.Range(0, spawnPoints.Length);

			var player = Instantiate(playerPrefab);
			player.transform.position = spawnPoints[randomPoint].position;
		}
	}
}