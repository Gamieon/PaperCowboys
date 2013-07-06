using UnityEngine;
using System.Collections;

/// <summary>
/// Directs how the projectile should be launched in BeginFiringPrimary
/// </summary>
public enum WeaponFiringDirection
{
	/// <summary>
	/// Fire in the direction that we are being aimed
	/// </summary>
	LocalAxis,
	
	/// <summary>
	/// Fire in the X direction only
	/// </summary>
	XAxis,
};

/// <summary>
/// A component that inherits this interface should belong to a weapon. Whether it's a pistol or
/// a bazooka or a knife, all weapons must have these functions.
/// 
/// It is intended for this project that a component which inherits an interface will
/// not have a subclass. I find the project easier to manage when there are no components
/// with interfaces that have subclasses which may need to have their own overloads to
/// call when interface functions are invoked.
/// </summary>
public interface IWeapon
{	
	/// <summary>
	/// Setup the weapon up for usage
	/// </summary>
	/// <param name='owner'>
	/// The owning character
	/// </param>
	void Setup(object owner);

	/// <summary>
	/// Called when this weapon is taken away from the player
	/// </summary>
	void Teardown();
	
	/// <summary>
	/// Poll this instance once per frame
	/// </summary>
	void Poll();	
	
	/// <summary>
	/// Fires the weapon's primary feature
	/// </summary>
	void BeginFiringPrimary(WeaponFiringDirection firingDirection, Color color);
	
	/// <summary>
	/// Ends the primary firing
	/// </summary>
	void EndFiringPrimary();
}
