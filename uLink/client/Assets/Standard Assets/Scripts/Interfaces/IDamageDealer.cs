using UnityEngine;
using System.Collections;

/// <summary>
/// A component with this interface must be able to assign damage to a damage taker.
/// The only damage dealer in this project is a projectile.
/// 
/// It is intended for this project that a component which inherits an interface will
/// not have a subclass. I find the project easier to manage when there are no components
/// with interfaces that have subclasses which may need to have their own overloads to
/// call when interface functions are invoked.
/// </summary>
public interface IDamageDealer 
{
	/// <summary>
	/// Gets the amount of damage this object does
	/// </summary>
	/// <returns>
	/// The amount of damage this object does
	/// </returns>
	float GetDamage();
	
	/// <summary>
	/// Determines if the damage dealer is owned by a player character
	/// </summary>
	/// <returns>
	/// True if the damage dealer is owned by the scene
	/// </returns>
	bool IsOwnedByPlayer();
	
	/// <summary>
	/// Gets the owning player.
	/// </summary>
	/// <returns>
	/// The owning player.
	/// </returns>
	object GetOwningPlayer();
}
