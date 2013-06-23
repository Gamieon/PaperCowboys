using UnityEngine;
using System.Collections;

/// <summary>
/// This component is attached to barrel objects; but it can be used for any
/// destructable non-characer object in the playfield. A more appropriate name for 
/// this component would be "SynchronizedDestructableObject"
/// 
/// The attached object is always owned by the master client, and can only be assigned damage
/// from or destroyed by the master client.
/// 
/// </summary>
[RequireComponent(typeof(MeshFlasher))]
public sealed class Barrel : SynchronizedObject, IDamageTaker
{
	/// <summary>
	/// Hit point count
	/// </summary>
	public float hitPoints = 5.0f;
	
	/// <summary>
	/// The audio source.
	/// </summary>
	public AudioSource audioSource;
	
	/// <summary>
	/// The sound that plays when the barrel is damaged
	/// </summary>
	public AudioClip sndDamage;
	
	/// <summary>
	/// The time that the barrel started to die
	/// </summary>
	float? dyingStartTime = null;
	
	/// <summary>
	/// The duration of time between when a barrel begins to die and the time
	/// it should be removed from the playfield.
	/// </summary>
	float dyingSpeed = 2.0f;
	
	/// <summary>
	/// Gets a value indicating whether this instance is dying.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is dying; otherwise, <c>false</c>.
	/// </value>
	bool IsDying { get { return (dyingStartTime.HasValue); } }
	
	/// <summary>
	/// Gets the minimum hitpoints before the object is considered "dead."
	/// </summary>
	/// <value>
	/// The minimum hitpoints before frag.
	/// </value>
	float MinHitpointsBeforeFrag { get { return 0.001f; } }
	
	/// <summary>
	/// The component that makes this object flash when it's hit
	/// </summary>
	MeshFlasher meshFlasher;
	
	#region MonoBehavior
	
	protected override void Awake()
	{
		base.Awake();
		meshFlasher = GetComponent<MeshFlasher>();
	}
	
	protected override void Update()
	{
		base.Update();
		
		// Update dying animation
		if (IsDying)
		{
			// Fade out by changing the material color
			float ttl = 1.0f - (Time.time - dyingStartTime.Value) * dyingSpeed;
			bool tryToUnspawn = false;
			if (ttl < 0) {
				ttl = 0; 
				tryToUnspawn = true;
			}
			renderer.material.color = new Color(
				renderer.material.color.r,
				renderer.material.color.g,
				renderer.material.color.b,ttl);
			
			// Try to destroy the object if we're the master client (since the
			// master client owns all inanimate objects)
			if (tryToUnspawn && NetworkDirector.isMasterClient)
			{
				NetworkDirector.Destroy(gameObject);
			}
		}
	}	

	#endregion
	
	#region RPC functions
		
	/// <summary>
	/// Sent from the master client observing something damaging this object to all clients
	/// to inform them that the object is being damaged.
	/// </summary>
	/// <param name='damage'>
	/// Damage.
	/// </param>
	/// <param name='owningPlayer'>
	/// The player who owns the object that dealt the damage, or null if it was an enemy.
	/// </param>
	[RPC]
	public void RPCBarrelTakeDamage(float damage, NetworkPlayer owningPlayer)
	{
		// Flash the barrel white
		meshFlasher.Flash(0.04f);
		
		// Play the damage clip
		PlaySound(sndDamage);

		// Deduct the hitpoint count
		hitPoints -= damage;

		// Handling for the master client only
		if (NetworkDirector.isMasterClient)
		{	
			// If the character is doomed, then begin its VISUAL death sequence. Other players
			// will discover this with the next serialization; and you, as the master client, 
			// will eventually destroy the character entirely.
			if (hitPoints - damage < MinHitpointsBeforeFrag)
			{
				// Tell everybody that this object is dying
				networkView.RPC("RPCBarrelBeginDying", RPCMode.All);
			}
		}
	}
	
	/// <summary>
	/// Sent from the master client to all clients to inform them this object is dying
	/// </summary>
	[RPC]
	public void RPCBarrelBeginDying()
	{
		if (!IsDying) 
		{
			BeginDying();
		}
		else
		{
			// This should never happen
		}
	}
	
	/// <summary>
	/// Called by the NetworkDirector to destroy this game object.
	/// </summary>
	[RPC]
	public void RPCDestroy()
	{
		Destroy(gameObject);
	}
	
	#endregion
	
	#region Custom Networking
	
    protected override void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
		base.OnSerializeNetworkView(stream, info);
        if (stream.isWriting)
        {
            // We own this object and we are the master client. Send the others our data.
			stream.Serialize(ref hitPoints);
        }
        else
        {
            // Network object, receive data. Every player needs the barrel's hitpoints in sync because
			// any one of them could be the new master client at any moment.
			stream.Serialize(ref hitPoints);
        }
    }
	
	/// <summary>
	/// Called from NetworkDirector.Instantiate to provide additional information
	/// </summary>
	/// <param name='owner'>
	/// The owning player.
	/// </param>
	/// <param name='sourceSpawnID'>
	/// Source spawn I.
	/// </param>
	[RPC]
    public void RPCNetworkInstantiate(NetworkPlayer owner, int sourceSpawnID)
    {
		// We don't care about this
    }	
	
	#endregion	
	
	#region IDamageTaker
	
	/// <summary>
	/// Called by the player observing their projectile making contact with this character to inflict
	/// damage to this character. This must never be called from an RPC.
	/// </summary>
	/// <param name='damager'>
	/// An interface to the object dealing the damage.
	/// </param>	
	void IDamageTaker.TakeDamage(IDamageDealer damageDealer)
	{
		if (!IsDying)
		{
			float damage = damageDealer.GetDamage();
			// Tell everybody that this barrel is taking damage
			networkView.RPC("RPCBarrelTakeDamage", RPCMode.All, damage, damageDealer.GetOwningPlayer() );
		}
		else
		{
			// Ignore if the character is already dying
		}
	}
	
	#endregion	
	
	/// <summary>
	/// Called for all players to begin the visible dying process
	/// </summary>
	void BeginDying()
	{
		// Set the dying start time
		dyingStartTime = Time.time;
		// "Disable" the rigidbody's effectiveness
		if (null != myRigidbody) 
		{
			myRigidbody.isKinematic = true;
		}
		// Destroy the collider
		if (null != collider)
		{
			Destroy(collider);
		}
	}	
	
	/// <summary>
	/// Plays a sound effect from this object's audio source.
	/// </summary>
	/// <param name='audioClip'>
	/// Audio clip.
	/// </param>
	void PlaySound(AudioClip audioClip)
	{
		audioSource.Stop();
		audioSource.volume = AudioDirector.SFXVolume;
		audioSource.pitch = Random.Range(0.8f, 1.4f);
		audioSource.PlayOneShot(audioClip);
	}	
}
