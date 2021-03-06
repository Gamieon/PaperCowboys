using UnityEngine;
using System.Collections;

/// <summary>
/// Enemy dynamite behavior
/// </summary>
public class EnemyDynamite : Weapon, IWeapon
{
	const string bulletPrefabName = "FlyingDynamite";
	
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
		float lifeTime = 1.0f;
		float launchForceMagnitude = 5.0f;
		LaunchProjectile(bulletPrefabName, muzzleTip.position, transform.parent.parent.localRotation,
			lifeTime, Vector3.up * 10.0f + transform.right * launchForceMagnitude, color);
	}
	
	/// <summary>
	/// Ends the primary firing
	/// </summary>
	void IWeapon.EndFiringPrimary()
	{
	}
	
	#endregion
}
