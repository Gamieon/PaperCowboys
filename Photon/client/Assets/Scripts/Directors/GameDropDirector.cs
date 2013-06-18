using UnityEngine;
using System.Collections;

/// <summary>
/// This component should be attached to the GameManager game object of a game scene.
/// It is used by the GameDirector component to decide what item to drop when a player 
/// earns one.
/// 
/// Developers can modify this component's values in the scene from the Unity editor's 
/// inspector window to dictate what can drop and how often. For example, you don't want 
/// players getting high level weapons in level 1; likewise getting pistols at level 20 
/// is rather useless.
/// </summary>
public class GameDropDirector : MonoBehaviour 
{
	/// <summary>
	/// Gets the game drop director instance.
	/// </summary>
	/// <value>
	/// The game drop director instance.
	/// </value>
	static public GameDropDirector Instance
	{
		get {
			object gameDropDirector = Object.FindObjectOfType(typeof(GameDropDirector));
			return (GameDropDirector)gameDropDirector;
		}
	}
	
	/// <summary>
	/// The available weapon drops for this level.
	/// </summary>
	public ItemDrop[] availableWeaponDrops;
	
	/// <summary>
	/// The available hat drops for this level.
	/// </summary>
	public ItemDrop[] availableHatDrops;

	/// <summary>
	/// The available mouth accessory drops for this level.
	/// </summary>
	public ItemDrop[] availableMouthDrops;
	
	/// <summary>
	/// The available ammo drops for this level.
	/// </summary>
	public ItemDrop[] availableAmmoDrops;
	
	/// <summary>
	/// Gets the available drop.
	/// </summary>
	/// <returns>
	/// The available drop.
	/// </returns>
	/// <param name='itemDropType'>
	/// Item drop type.
	/// </param>
	public ItemDrop GetAvailableDrop(ItemDropType itemDropType)
	{
		foreach (ItemDrop drop in availableWeaponDrops)
		{
			if (drop.DropType == itemDropType) {
				return drop;
			}
		}
		foreach (ItemDrop drop in availableHatDrops)
		{
			if (drop.DropType == itemDropType) {
				return drop;
			}
		}		
		foreach (ItemDrop drop in availableMouthDrops)
		{
			if (drop.DropType == itemDropType) {
				return drop;
			}
		}
		foreach (ItemDrop drop in availableAmmoDrops)
		{
			if (drop.DropType == itemDropType) {
				return drop;
			}
		}
		return null;
	}
}
