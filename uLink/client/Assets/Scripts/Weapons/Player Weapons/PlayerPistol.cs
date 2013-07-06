using UnityEngine;
using System.Collections;

/// <summary>
/// Player pistol behavior.
/// </summary>
public class PlayerPistol : PlayerWeapon, IWeapon
{
	const string bulletPrefabName = "PistolBullet";
	
	#region IWeapon
	
	/// <summary>
	/// Setup the weapon up for usage
	/// </summary>
	/// <param name='owner'>
	/// The owning character
	/// </param>
	void IWeapon.Setup(object owner)
	{
		owningCharacter = (Character)owner;
	}
	
	/// <summary>
	/// Called when this weapon is taken away from the player
	/// </summary>
	void IWeapon.Teardown()
	{
	}
	
	/// <summary>
	/// Poll this instance once per frame
	/// </summary>
	void IWeapon.Poll()
	{
	}
	
	/// <summary>
	/// Fires the weapon's primary feature
	/// </summary>
	void IWeapon.BeginFiringPrimary(WeaponFiringDirection firingDirection, Color color)
	{
		float lifeTime = 0.75f;
		float launchForceMagnitude = 10.0f;
		Vector3 v = (firingDirection == WeaponFiringDirection.LocalAxis) ? transform.right : new Vector3(transform.right.x,0,0).normalized;		
		LaunchProjectile(bulletPrefabName, muzzleTip.position, transform.parent.parent.localRotation,
			lifeTime, v * launchForceMagnitude, color);
	}
	
	/// <summary>
	/// Ends the primary firing
	/// </summary>
	void IWeapon.EndFiringPrimary()
	{
	}
	
	#endregion
}
