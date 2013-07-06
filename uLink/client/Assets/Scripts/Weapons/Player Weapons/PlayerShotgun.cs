using UnityEngine;
using System.Collections;

/// <summary>
/// Player shotgun behavior
/// </summary>
public class PlayerShotgun : PlayerWeapon, IWeapon
{
	const string bulletPrefabName = "ShotgunBullet";
	
	const int shellsPerShot = 4;
	
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
		if (CanFire)
		{
			float lifeTime = 0.6f;
			float launchForceMagnitude = 10.0f;
			for (int i=0; i < shellsPerShot; i++)
			{
				Vector3 v = (firingDirection == WeaponFiringDirection.LocalAxis) ? transform.right : new Vector3(transform.right.x,0,0).normalized;
				Vector3 f = v * launchForceMagnitude;
				f = Quaternion.AngleAxis(Mathf.Lerp(-30.0f,30.0f,(float)i / (shellsPerShot-1)),Vector3.forward) * f;
				LaunchProjectile(bulletPrefabName, muzzleTip.position, transform.parent.parent.localRotation,
					lifeTime, f, color);
				// If this isn't the final shot, we have to reset the timer
				if (i < shellsPerShot - 1) {
					ResetTimeLastFired();
				}
			}
		}
	}
	
	/// <summary>
	/// Ends the primary firing
	/// </summary>
	void IWeapon.EndFiringPrimary()
	{
	}
	
	#endregion
}
