using UnityEngine;
using System.Collections;

/// <summary>
/// This is the base class for a projectile. Unlike a standard synchronization object,
/// we do NOT synchronize our position. This object is instantiated at the same place
/// for all players and every individual player moves it independently.
/// 
/// The projectile owner decides if this actually damages other objects; but the
/// master server, not this application, will need to do sanity checks to avoid 
/// cheating.
/// 
/// </summary>
[RequireComponent(typeof(uLinkNetworkView))]
public class Projectile : uLink.MonoBehaviour, IDamageDealer
{
	/// <summary>
	/// The amount of damage the projectile does. Damage is simply measured in "units."
	/// </summary>
	public float damage = 1.0f;
	
	/// <summary>
	/// True if the projectile is piercing (this means it won't destroy itself after making
	/// contact with another object)
	/// </summary>
	public bool piercing = false;
	
	/// <summary>
	/// True if the owner is destroying this object. This allows for subclasses to do special
	/// handling in their own DestroyProjectile overrides.
	/// </summary>
	protected bool isDestroying = false;
	
	/// <summary>
	/// True if this projectile is owned by a player.
	/// </summary>
	bool ownedByPlayer;
	
	/// <summary>
	/// The max time for this projectile to live.
	/// </summary>
	float maxTimeToLive;
			
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
	
	protected virtual void Update()
	{
		// If this projectile isn't being destroyed and we're the owner,
		// we need to see if it has outlived its lifespan.
		if (!isDestroying && myNetworkView.isMine)
		{
			if (Time.time > maxTimeToLive)
			{
				//Debug.Log("projectile expired");
				
				// Flag the fact we're being destroyed in case we're inherited
				// by a base class that doesn't destroy it right away.
				isDestroying = true;
				
				// Now begin the destruction sequence.
				DestroyProjectile(false);
			}
		}
		else
		{
			// It's already being destroyed, or we have to wait for the master client to destroy it
		}
	}
	
	protected virtual void OnTriggerEnter(Collider other)
	{
		// If the bullet "entered" anything and we own it, we have to deal with it.
		if (!isDestroying && myNetworkView.isMine)
		{
			if ((int)GameDirector.GameLayers.Ledge != other.gameObject.layer 
				&& other.gameObject.layer != gameObject.layer
				&& null == other.gameObject.GetComponent<Projectile>()
				&& null == other.gameObject.GetComponent<SpawnPoint>()
				)
			{
				//Debug.Log("projectile " + gameObject.layer + " triggered with " + other.name + " " + other.gameObject.layer);
				
				// See if there's a synchronized object attached to the collider
				SynchronizedObject synchronizedObject = other.gameObject.GetComponent<SynchronizedObject>();
				if (null != synchronizedObject)
				{
					// Yes; but can it take damage?
					IDamageTaker damageTaker = synchronizedObject as IDamageTaker;
					if (null != damageTaker)
					{
						// Yes it can! Blammo time.
						damageTaker.TakeDamage((IDamageDealer)this);
					}
				}
				if (!piercing)
				{
					isDestroying = true;
					DestroyProjectile(true);
				}
			}
		}
		else
		{
			// The projectile is already being destroyed, or we have no authority to make it hit an object
		}
	}
	
	#endregion
	
	/// <summary>
	/// Called by the owner to destroy the projectile.
	/// </summary>
	/// <param name='didCollideWithObject'>
	/// True if it collided with object; or false if its life expired.
	/// </param>
	protected virtual void DestroyProjectile(bool didCollideWithObject)
	{
		if (myNetworkView.isMine)
		{
			NetworkDirector.Destroy(gameObject);
		}
		else
		{
			Debug.LogError("Tried to destroy someone else's projectile!");
		}
	}
	
	#region Custom Networking
	
	/// <summary>
	/// Fired by uLink.Network.Instantiate when this object is created on the network
	/// </summary>
	/// <param name='info'>
	/// Info.
	/// </param>
    void uLink_OnNetworkInstantiate(uLink.NetworkMessageInfo info)
	{
		bool playerOwnsProjectile = myNetworkView.initialData.Read<bool>();
		float lifeTime = myNetworkView.initialData.Read<float>();
		Vector3 launchForce = myNetworkView.initialData.Read<Vector3>();
		Color c;
		c.r = myNetworkView.initialData.Read<float>();
		c.g = myNetworkView.initialData.Read<float>();
		c.b = myNetworkView.initialData.Read<float>();
		c.a = myNetworkView.initialData.Read<float>();
		
		// Assign the projectile color and layer based on its owner
		ownedByPlayer = playerOwnsProjectile;
		if (ownedByPlayer)
		{
			renderer.material.SetColor("_Emission", c);
			gameObject.layer = (int)GameDirector.GameLayers.PlayerCharacter;
		}
		else
		{
			renderer.material.SetColor("_Emission", c);
			gameObject.layer = (int)GameDirector.GameLayers.Enemy;
		}
		
		// Assign the projectile lifetime
		maxTimeToLive = Time.time + lifeTime;

		// Now make it launch
		rigidbody.AddForce(launchForce);
    }
	
	#endregion
	
	#region RPC calls
	
	/// <summary>
	/// Called by the NetworkDirector to destroy this game object.
	/// </summary>
	[RPC]
	public void RPCDestroy()
	{
		Destroy(gameObject);
	}
	
	#endregion
	
	#region IDamageDealer
	
	float IDamageDealer.GetDamage()
	{
		return damage;
	}
	
	bool IDamageDealer.IsOwnedByPlayer()
	{
		return ownedByPlayer;
	}
	
	object IDamageDealer.GetOwningPlayer()
	{
		return myNetworkView.owner;
	}
	
	#endregion
}
