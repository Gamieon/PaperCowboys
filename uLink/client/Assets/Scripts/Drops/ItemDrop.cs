using UnityEngine;
using System.Collections;

/// <summary>
/// The global item drop type
/// </summary>
public enum ItemDropType
{
	Winchesta,
	Shotgun,
	RocketLauncher,
	Ammo,
	CowboyHat,
	EastwoodHat,
	TenGallonHat,
	Bandana,
}

/// <summary>
/// This represents an element in the item drop list maintained by the GameDropDirector.
/// This is NOT an actual drop in the scene (see PlayerDrop); it instead represents an
/// entry in the catalog of items that can be dropped.
/// </summary>
[System.Serializable]
public class ItemDrop
{
	/// <summary>
	/// The type of the drop
	/// </summary>
	public ItemDropType DropType;
	
	/// <summary>
	/// The chances of it being chosen relative to other item drops
	/// </summary>
	public float Weight;
	
	/// <summary>
	/// The item icon
	/// </summary>
	public Texture2D Icon;
}
