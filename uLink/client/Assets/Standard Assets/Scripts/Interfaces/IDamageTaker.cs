using UnityEngine;
using System.Collections;

/// <summary>
/// A component with this interface must be able to take damage. This includes pretty much
/// every kind of character class and inanimate objects like barrels.
/// 
/// Note the absence of GetHitpoints(). It's not important for the damage dealer to know how
/// many hitpoints the damaged object has; and besides, a damage taker may not have any hitpoints
/// if it intends to die in one hit.
/// 
/// It is intended for this project that a component which inherits an interface will
/// not have a subclass. I find the project easier to manage when there are no components
/// with interfaces that have subclasses which may need to have their own overloads to
/// call when interface functions are invoked.
/// </summary>
public interface IDamageTaker
{
	/// <summary>
	/// Called to deal damage to a damage taker
	/// </summary>
	/// <param name='damager'>
	/// An interface to the object dealing the damage.
	/// </param>
	void TakeDamage(IDamageDealer damageDealer);
}
