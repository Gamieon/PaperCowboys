using UnityEngine;
using System.Collections;

/// <summary>
/// All player weapon components must derive from this class. Those components will exist in the scene
/// attached to the visible player weapon.
/// </summary>
public class PlayerWeapon : Weapon 
{
	/// <summary>
	/// The weapon ID.
	/// </summary>
	public WeaponDirector.PlayerWeapon weaponID;
	
	/// <summary>
	/// The minimum number of seconds in-between shots. When a player fires, the canFire variable is
	/// reset, and not set again until this interval expires
	/// </summary>
	public float minSecondsBetweenShots = 0.33f;
	
	/// <summary>
	/// The time the player last fired
	/// </summary>
	float timeLastFired;
	
	/// <summary>
	/// Gets a value indicating whether this weapon can fire.
	/// </summary>
	/// <value>
	/// <c>true</c> if this weapon can fire; otherwise, <c>false</c>.
	/// </value>
	protected bool CanFire { 
		get { 
			return (PlayerDirector.ActiveSession.GetAmmo(weaponID) > 0) && 
				((Time.time - timeLastFired) > minSecondsBetweenShots); 
		} 
	}
	
	/// <summary>
	/// Launch a projectile
	/// </summary>
	protected override void LaunchProjectile(string prefabName, Vector3 position, Quaternion rotation, 
		float lifeTime, Vector3 forward, Color color)
	{
		if (CanFire)
		{
			// Launch the projectile
			base.LaunchProjectile(prefabName, position, rotation, lifeTime, forward, color, true);
			// Decrease the ammo by one
			PlayerDirector.ActiveSession.SetAmmo( weaponID, PlayerDirector.ActiveSession.GetAmmo(weaponID) - 1 );
			// If the player is out of ammo, revert them to their pistol
			if (PlayerDirector.ActiveSession.GetAmmo(PlayerDirector.ActiveSession.CurrentWeapon) <= 0)
			{
				PlayerDirector.ActiveSession.CurrentWeapon = WeaponDirector.PlayerWeapon.Pistol;
				owningCharacter.SetActiveWeapon("Player" + PlayerDirector.ActiveSession.CurrentWeapon.ToString());
			}
			// Set the time the weapon was fired last
			timeLastFired = Time.time;
		}
		else
		{
			// Still waiting from the last time we fired
		}
	}
	
	/// <summary>
	/// Resets the time last fired.
	/// </summary>
	protected void ResetTimeLastFired()
	{
		timeLastFired = 0;
	}
	
	#region MonoBehavior
	
	void Awake()
	{
		// The player should always be able to fire immediately
		timeLastFired = -minSecondsBetweenShots;
	}
	
	#endregion
}
