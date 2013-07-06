using UnityEngine;
using System.Collections;

/// <summary>
/// Enemy knife behavior
/// </summary>
public class EnemyKnife : Weapon, IWeapon
{
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
		// Knives do not have projectiles
	}
	
	/// <summary>
	/// Ends the primary firing
	/// </summary>
	void IWeapon.EndFiringPrimary()
	{
		// Knives do not have projectiles
	}
	
	#endregion
}
