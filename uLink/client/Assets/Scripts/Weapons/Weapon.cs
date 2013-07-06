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
		float lifeTime, Vector3 forward, Color color)
	{
		if (null != sndFire) PlaySound(sndFire);
		NetworkDirector.Instance.Instantiate(prefabName, muzzleTip.position, transform.parent.parent.localRotation, 0, 
			new object[] { false, lifeTime, forward, color, color.r, color.g, color.b, color.a }
		);
	}
	
	/// <summary>
	/// Launch a projectile
	/// </summary>
	protected void LaunchProjectile(string prefabName, Vector3 position, Quaternion rotation, 
		float lifeTime, Vector3 forward, Color color, bool ownedByPlayer)
	{
		if (null != sndFire) PlaySound(sndFire);
		NetworkDirector.Instance.Instantiate(prefabName, muzzleTip.position, transform.parent.parent.localRotation, 0, 
			new object[] { ownedByPlayer, lifeTime, forward, color.r, color.g, color.b, color.a }
		);
	}
}
