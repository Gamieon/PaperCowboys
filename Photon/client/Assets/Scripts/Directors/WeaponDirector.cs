using UnityEngine;
using System.Collections;

/// <summary>
/// A simple class that defines what weapons are available in the game
/// </summary>
public static class WeaponDirector
{
	/// <summary>
	/// Available player weapons
	/// </summary>
	public enum PlayerWeapon
	{
		/// <summary>
		/// Regular pistol
		/// </summary>
		Pistol = 0,
		
		/// <summary>
		/// The winchesta armor-piercing rifle
		/// </summary>
		Winchesta,
		
		/// <summary>
		/// Shotgun
		/// </summary>
		Shotgun,
		
		/// <summary>
		/// Rocket launcher
		/// </summary>
		RocketLauncher,
		
		/// <summary>
		/// Weapon count.
		/// </summary>
		WeaponCount
	};
	
	/// <summary>
	/// Available enemy weapons
	/// </summary>
	public enum EnemyWeapon
	{
		/// <summary>
		/// Standard pistol
		/// </summary>
		Pistol,
		
		/// <summary>
		/// Machine gun.
		/// </summary>
		MachineGun,
	};
	
	/// <summary>
	/// The number of available player weapons
	/// </summary>
	public const int PlayerWeaponCount = (int)PlayerWeapon.WeaponCount;
}
