using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Describes a point in space where enemies can spawn onto the playfield. Spawn points
/// are controlled by the master client, and a spawn point can spawn more than one enemy
/// in its lifetime although they must be spawned one-at-a-time.
/// </summary>
[RequireComponent(typeof(uLinkNetworkView))]
public class SpawnPoint : uLink.MonoBehaviour 
{
	/// <summary>
	/// List of available prefabs to spawn by name
	/// </summary>
	public enum SpawnPrefab
	{
		Barrel,
		Drum,
		L1Boss,
		ThugCrawling,
		ThugCrouching,
		ThugStanding,
		ThugWalking,
		ThugWindow,
		ThugWithKnife,
		WeakPlank,
		L2Boss,
		L3Boss,
	}
	
	/// <summary>
	/// The name of the prefab to spawn
	/// </summary>
	public SpawnPrefab spawnPrefab;
	
	/// <summary>
	/// The gizmo icon.
	/// </summary>
	public string gizmoIcon;
	
	/// <summary>
	/// The minimum x that a player needs to be in order for an enemy to spawn
	/// </summary>
	public float xMin = -12;
	
	/// <summary>
	/// The maximum x that a player needs to be in order for an enemy to spawn
	/// </summary>
	public float xMax = 12;
	
	/// <summary>
	/// True if we support respawning
	/// </summary>
	public bool respawn;
	
	/// <summary>
	/// The amount of time before an enemy is respawned
	/// </summary>
	public float respawnWait;
	
	/// <summary>
	/// The random amount of time before an enemy is respawned
	/// </summary>
	public float respawnRandomWait;
	
	/// <summary>
	/// Cached transform.
	/// </summary>
	Transform myTransform;
	
	/// <summary>
	/// The wait time for the next object to respawn
	/// </summary>
	float nextRespawnWait;
	
	/// <summary>
	/// The spawned enemy count.
	/// </summary>
	int totalSpawnedObjects;
	
	/// <summary>
	/// True if there is presently a spawned enemy in the playfield
	/// </summary>
	bool spawneeActive;
	
	/// <summary>
	/// The time the enemy was destroyed last.
	/// </summary>
	float timeEnemyDestroyed;
	
	/// <summary>
	/// Gets the new spawn ID.
	/// </summary>
	/// <value>
	/// The new spawn ID.
	/// </value>
	static int NewGlobalSpawnID
	{
		get {
			return nextGlobalSpawnID++;
		}
	}
	static int nextGlobalSpawnID = 1;
	
	/// <summary>
	/// The unique ID of this spawner
	/// </summary>
	public int SpawnID
	{
		get { return spawnID; }
	}
	int spawnID;
	
	/// <summary>
	/// Gets my network view.
	/// </summary>
	/// <value>
	/// My network view.
	/// </value>
	public uLink.NetworkView myNetworkView
	{
		get { return uLink.NetworkView.Get(this); } 
	}
	
	#region MonoBehavior
	
	void Awake()
	{
		myTransform = transform;
		myNetworkView.observed = this;
		myNetworkView.stateSynchronization = uLink.NetworkStateSynchronization.ReliableDeltaCompressed;
		spawnID = NewGlobalSpawnID;
		nextRespawnWait = respawnWait + Random.value * respawnRandomWait;
	}
	
	void Update()
	{
		// Enemy spawning happens here		
		if (NetworkDirector.isMasterClient // We're the master client
			&& !spawneeActive // We don't have an active enemy at the moment
			// We're ready to spawn an enemy
			&& ( (0 == totalSpawnedObjects) || (0 == totalSpawnedObjects && !respawn) || (respawn && Time.time >= timeEnemyDestroyed + nextRespawnWait) )
			)
		{
			// Ok, so we should spawn if a player is in range...but is a player in range?
			List<PlayerCharacter> playerCharacters = PlayerCharacter.AllPlayerCharacters;
			foreach (PlayerCharacter c in playerCharacters)
			{
				if (c.transform.position.x > myTransform.position.x + xMin &&
					c.transform.position.x < myTransform.position.x + xMax)
				{
					// Update the local values now. If an exception is thrown later, we don't want it to
					// try to respawn anything.
					spawneeActive = true;
					totalSpawnedObjects++;					
					// Now spawn the enemy
					GameObject spawnObject = (GameObject)Resources.Load(spawnPrefab.ToString());
					NetworkDirector.Instance.InstantiateSceneObject(spawnPrefab.ToString(), transform.position,
						spawnObject.transform.localRotation, 0, new object[] { spawnID } );
					break;					
				}
			}
			nextRespawnWait = respawnWait + Random.value * respawnRandomWait;
		}
	}
					
	#endregion
	
	#region RPCs
	
	/// <summary>
	/// Sent to the master client (which is also the owning player)
	/// when the spawned enemy has died
	/// </summary>
	/// <param name='spawnedEnemy'>
	/// Spawned enemy.
	/// </param>
	[RPC]
	public void RPCSpawnedEnemyDied(uLink.NetworkView spawnedEnemy)
	{
		Debug.Log("in RPCSpawnedEnemyDied");
		// Reset the flag locally
		spawneeActive = false;
		// Set the time that the enemy was destroyed
		timeEnemyDestroyed = Time.time;
	}
	
	#endregion
	
	#region Custom Networking
	
    protected void uLink_OnSerializeNetworkView(uLink.BitStream stream, uLink.NetworkMessageInfo info)
    {
        if (stream.isWriting)
        {
            // We own this player: send the others our data
			stream.Write(totalSpawnedObjects);
			stream.Write(spawneeActive);
			stream.Write(timeEnemyDestroyed);
			stream.Write(spawnID);
        }
        else
        {
            // Network player, receive data
			totalSpawnedObjects = stream.Read<int>();
			spawneeActive = stream.Read<bool>();
			timeEnemyDestroyed = stream.Read<float>();
			spawnID = stream.Read<int>();
        }
    }
	
	#endregion	
}
