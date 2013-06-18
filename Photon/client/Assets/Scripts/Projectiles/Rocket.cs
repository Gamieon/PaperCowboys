using UnityEngine;
using System.Collections;

/// <summary>
/// This is a special kind of projectile that has a particle system attached to it.
/// It is otherwise identical in all other ways.
/// </summary>
public class Rocket : Projectile 
{
	// TODO: Needs splash damage

	/// <summary>
	/// The smoke particle system
	/// </summary>
	public ParticleSystem smokeParticleSystem;	
	
	/// <summary>
	/// The explosion particle system.
	/// </summary>
	public ParticleSystem explosionParticleSystem;
	
	/// <summary>
	/// The audio source.
	/// </summary>
	public AudioSource audioSource;
	
	/// <summary>
	/// The explosion sound.
	/// </summary>
	public AudioClip sndExplosion;

	/// <summary>
	/// Called by the owner to destroy the projectile.
	/// </summary>
	/// <param name='didCollideWithObject'>
	/// True if it collided with an object; or false if its life expired.
	/// </param>
	protected override void DestroyProjectile(bool didCollideWithObject)
	{
		if (photonView.isMine)
		{
			// Don't destroy the rocket; otherwise all the particles will just disappear. Just
			// send an RPC call to everyone so that it gets hidden while the particles run their course.
			photonView.RPC("RPCDestroyRocket", PhotonTargets.All, didCollideWithObject);
		}
		else
		{
			Debug.LogError("Tried to destroy someone else's rocket!");
		}		
	}
	
	/// <summary>
	/// Sent to all players when a rocket is being destroyed
	/// </summary>
	/// <param name='didCollideWithObject'>
	/// True if the rocket collided with an object according to the player who shot it.
	/// </param>
	[RPC]
	void RPCDestroyRocket(bool didCollideWithObject)
	{
		// All non-master clients need to set this flag.
		isDestroying = true;
		// Hide the mesh renderer
		GetComponent<MeshRenderer>().enabled = false;
		// Stop emitting new smoke particles
		smokeParticleSystem.enableEmission = false;
		// Handle collisions
		if (didCollideWithObject)
		{
			// Emit an explosion if there was a collision
			explosionParticleSystem.Emit(200);
			// Shake the camera for fun!
			GameCameraDirector.Instance.ShakeCamera();
		}
		
		// Play the explosion sound
		audioSource.Stop();
		audioSource.volume = AudioDirector.SFXVolume;		
		audioSource.PlayOneShot(sndExplosion);

		// Stop the object from moving if it's ours
		rigidbody.velocity = Vector3.zero;
		
		// Destroy the rocket after a second. That should be plenty of time
		// for all the particle systems to exhaust themselves. It doesn't
		// matter if we're the master client right now; it matters if we're
		// the master client one second from now.
		if (photonView.isMine)
		{
			Invoke("OnDestroyRocket", 1.0f);
		}
	}
		
	/// <summary>
	/// Raises the destroy rocket event.
	/// </summary>
	void OnDestroyRocket()
	{
		if (photonView.isMine)
		{
			PhotonNetwork.Destroy(gameObject);
		}
		else
		{
			Debug.LogError("Tried to destroy someone else's rocket!");
		}		
	}
}
