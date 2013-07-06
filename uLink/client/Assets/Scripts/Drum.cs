using UnityEngine;
using System.Collections;

/// <summary>
/// This component is attached to rolling drum objects; but it can be used for any
/// non-destructable non-characer object in the playfield. A more appropriate name for 
/// this component would be "SynchronizedIndestructableObject"
/// 
/// This object has one other capability, and that is to destroy player and enemy
/// objects alike.
/// 
/// This object is always owned by the master client.
/// 
/// </summary>
public sealed class Drum : SynchronizedObject, IDamageDealer
{
	#region MonoBehavior
	
	void OnCollisionEnter(Collision collision)
	{
		if (NetworkDirector.isMasterClient)
		{
			// Did we hit an enemy?
			if ((int)GameDirector.GameLayers.Enemy == collision.gameObject.layer)
			{
				EnemyCharacter enemyCharacter = collision.gameObject.GetComponent<EnemyCharacter>();
				if (null != enemyCharacter)
				{
					IDamageTaker damageTaker = (IDamageTaker)enemyCharacter;
					if (null != damageTaker)
					{
						// Yes, we definitely hit a valid enemy. Lets damage it.
						damageTaker.TakeDamage(this);
					}
					else
					{
						// We should never get here
					}
				}
				else
				{
					// We should never get here
				}
			}
			// Are we falling onto a player with a significant vertical speed?
			else if ((int)GameDirector.GameLayers.PlayerCharacter == collision.gameObject.layer
				&& myRigidbody.velocity.y < -1.0f)
			{
				PlayerCharacter playerCharacter = collision.gameObject.GetComponent<PlayerCharacter>();
				if (null != playerCharacter)
				{
					IDamageTaker damageTaker = (IDamageTaker)playerCharacter;
					if (null != damageTaker)
					{
						// Yes we are, lets damage them.
						damageTaker.TakeDamage(this);
					}
				}					
			}
		}
	}

	#endregion
	
	#region IDamageDealer
	
	/// <summary>
	/// Gets the amount of damage this object does
	/// </summary>
	/// <returns>
	/// The amount of damage this object does
	/// </returns>
	float IDamageDealer.GetDamage()
	{
		// More than enough for an instafrag.
		return 99999;
	}
	
	/// <summary>
	/// Determines if the damage dealer is owned by a player character
	/// </summary>
	/// <returns>
	/// True if the damage dealer is owned by the scene
	/// </returns>
	bool IDamageDealer.IsOwnedByPlayer()
	{
		return false;
	}
	
	/// <summary>
	/// Gets the owning player.
	/// </summary>
	/// <returns>
	/// The owning player.
	/// </returns>
	object IDamageDealer.GetOwningPlayer()
	{
		// The master client owns environmental objects
		return NetworkDirector.MasterClient;
	}
	
	#endregion	
	
	#region Custom Networking
	
	/// <summary>
	/// Fired by uLink.Network.Instantiate when this object is created on the network
	/// </summary>
	/// <param name='info'>
	/// Info.
	/// </param>	
    void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info)
    {
		// We don't care about this
    }
	
	#endregion
}
