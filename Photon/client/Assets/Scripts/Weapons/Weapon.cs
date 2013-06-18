using UnityEngine;
using System.Collections;

/// <summary>
/// This is the base class for all visible weapons in the game. Enemy and player
/// weapons are presently kept separate in case future changes require them to
/// behave entirely differently. So, you will see a player pistol and an enemy pistol
/// for example.
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class Weapon : MonoBehaviour 
{
	/// <summary>
	/// Instantiation data used when instantiating bullets
	/// </summary>
	public enum InstantiateParameters
	{
		/// <summary>
		/// True if owned by a player
		/// </summary>
		OwnedByPlayer = 0,
		
		/// <summary>
		/// The lifetime of the projectile that this weapon launches
		/// </summary>
		LifeTime = 1,
		
		/// <summary>
		/// The of force with which to launch the bullet
		/// </summary>
		LaunchForce = 2,
	}
	
	/// <summary>
	/// The fire sound
	/// </summary>
	public AudioClip sndFire;

	/// <summary>
	/// The muzzle tip where bullets shoot from, if any
	/// </summary>
	public Transform muzzleTip;	
	
	/// <summary>
	/// The audio source.
	/// </summary>
	public AudioSource audioSource;
	
	/// <summary>
	/// The character who owns the gun
	/// </summary>
	protected Character owningCharacter;
	
	/// <summary>
	/// Plays a sound.
	/// </summary>
	/// <param name='audioClip'>
	/// Audio clip.
	/// </param>
	protected virtual void PlaySound(AudioClip audioClip)
	{
		audioSource.Stop();
		audioSource.volume = AudioDirector.SFXVolume;
		audioSource.PlayOneShot(audioClip);
	}
	
	/// <summary>
	/// Launch a projectile
	/// </summary>
	protected virtual void LaunchProjectile(string prefabName, Vector3 position, Quaternion rotation, 
		float lifeTime, Vector3 forward)
	{
		if (null != sndFire) PlaySound(sndFire);
		PhotonNetwork.Instantiate(prefabName, muzzleTip.position, transform.parent.parent.localRotation, 0, 
			new object[] { false, lifeTime, forward }
		);
	}
	
	/// <summary>
	/// Launch a projectile
	/// </summary>
	protected void LaunchProjectile(string prefabName, Vector3 position, Quaternion rotation, 
		float lifeTime, Vector3 forward, bool ownedByPlayer)
	{
		if (null != sndFire) PlaySound(sndFire);
		PhotonNetwork.Instantiate(prefabName, muzzleTip.position, transform.parent.parent.localRotation, 0, 
			new object[] { ownedByPlayer, lifeTime, forward }
		);
	}
}
