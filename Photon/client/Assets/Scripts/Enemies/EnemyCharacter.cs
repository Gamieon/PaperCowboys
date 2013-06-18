using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Enemy characters must have a component inherited from this class.
/// 
/// Enemy characters are always owned by the master client.
/// 
/// An enemy object will have a component that is a subclass of this.
/// </summary>
public class EnemyCharacter : Character 
{
	/// <summary>
	/// The ID of the SpawnPoint component that created us; or null if there is none.
	/// I see no reason for this to ever be null in the current implementation.
	/// </summary>
	int? sourceEnemySpawnID = null;
	
	#region MonoBehavior
	
	protected override void Start()
	{
		base.Start();
		// Scale enemy hitpoints according to player count
		hitPoints = hitPoints * (float)PhotonNetwork.playerList.Length * 0.8f;
	}
	
	protected override void OnDisable()
	{
		// Inform our spawn source that we died. TODO: This is a slow operation; we really should cache
		// the list of spawn sources.
		if (sourceEnemySpawnID.HasValue)
		{
			SpawnPoint[] spawns = (SpawnPoint[])Object.FindObjectsOfType(typeof(SpawnPoint));
			foreach (SpawnPoint s in spawns)
			{
				if (s.SpawnID == sourceEnemySpawnID.Value)
				{
					// Inform ourselves with an RPC that the spawned enemy has died. 
					// TODO: Only the master client should be doing this, and there's no need to send
					// ourselves an RPC call.
					s.photonView.RPC("RPCSpawnedEnemyDied", PhotonTargets.MasterClient, photonView);
				}
			}
		}
		base.OnDisable();
	}		
	
	#endregion
	
	/// <summary>
	/// Gets the closest player.
	/// </summary>
	/// <returns>
	/// The closest player.
	/// </returns>
	protected PlayerCharacter GetClosestPlayer()
	{
		List<PlayerCharacter> playerCharacters = PlayerCharacter.AllPlayerCharacters;
		float? dNearest = null;
		PlayerCharacter result = null;
		foreach (PlayerCharacter pc in playerCharacters)
		{
			float d = Vector3.SqrMagnitude( myTransform.position - pc.transform.position );
			if (!dNearest.HasValue || d < dNearest.Value)
			{
				dNearest = d;
				result = pc;
			}
		}
		return result;
	}
	
	#region PhotonNetworkingMessage
	
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
		sourceEnemySpawnID = (int)photonView.instantiationData[0];
    }	
	
	#endregion
}
