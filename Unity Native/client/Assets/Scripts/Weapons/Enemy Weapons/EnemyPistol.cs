using UnityEngine;
using System.Collections;

/// <summary>
/// Enemy pistol behavior
/// </summary>
public class EnemyPistol : Weapon, IWeapon
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
	void IWeapon.BeginFiringPrimary(WeaponFiringDirection firingDirection)
	{
		float lifeTime = 1.0f;
		float launchForceMagnitude = 5.0f;
		LaunchProjectile(bulletPrefabName, muzzleTip.position, transform.parent.parent.localRotation,
			lifeTime, transform.right * launchForceMagnitude);
	}
	
	/// <summary>
	/// Ends the primary firing
	/// </summary>
	void IWeapon.EndFiringPrimary()
	{
	}
	
	#endregion
}
