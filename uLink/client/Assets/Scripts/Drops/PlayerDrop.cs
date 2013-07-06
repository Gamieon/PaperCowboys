using UnityEngine;
using System.Collections;

/// <summary>
/// This is a dropped object in the scene that appears at random after an enemy
/// dies. This can be a hat, bandanna, ammo or a weapon.
/// 
/// Drops are NOT synchronized objects. They exist solely on the player's instance
/// they were instantiated in, and no other player will see it.
/// 
/// Objects with the PlayerDrop component are created by GameDirector.RPCGiveDrop
/// which is invoked by the master client.
/// </summary>
public class PlayerDrop : MonoBehaviour 
{
	/// <summary>
	/// The player character receiving the drop
	/// </summary>
	public PlayerCharacter playerCharacter;
	
	/// <summary>
	/// The item that is being offered to the player.
	/// </summary>
	public ItemDrop itemDrop;
	
	/// <summary>
	/// The sound of the drop pickup.
	/// </summary>
	public AudioClip sndDropPickup;
		
	/// <summary>
	/// The icon renderer.
	/// </summary>
	public Renderer iconRenderer;
	
	/// <summary>
	/// The icon transform.
	/// </summary>
	public Transform iconTransform;
	
	/// <summary>
	/// Cached transform.
	/// </summary>
	Transform myTransform;
	
	/// <summary>
	/// The icon aspect ratio.
	/// </summary>
	float iconAspect;
	
	/// <summary>
	/// The position that this drop was picked up from
	/// </summary>
	Vector3 pickupPosition;
	
	/// <summary>
	/// The time the weapon was picked up
	/// </summary>
	float? tPickedUp = null;
	
	#region MonoBehavior
	
	// Use this for initialization
	void Start() 
	{
		// By this point the item drop member should have been populated. Get it set up.
		myTransform = transform;
		
		// Assign the icon aspect ratio for proper rendering
		iconAspect = (float)itemDrop.Icon.height / (float)itemDrop.Icon.width;
		
		// Set the color to match that of the player
		iconRenderer.material.SetColor("_Emission", PlayerDirector.PlayerColor);
		iconRenderer.material.mainTexture = itemDrop.Icon;
		
		// Now launch the drop
		rigidbody.AddForce(new Vector3(
			transform.position.x > playerCharacter.transform.position.x ? -200 : 200,
			300,
			0));
		
		// Start a timer to destroy ourselves
		Invoke("SelfDestruct", 10.0f);
	}
	
	void FixedUpdate()
	{
		float s;
		if (tPickedUp.HasValue)
		{
			// We were picked up, so the item should shrink toward the player
			s = 1.0f - (Time.time - tPickedUp.Value) * 4.0f;
			myTransform.position = Vector3.Slerp(playerCharacter.transform.position, pickupPosition, s);
		}
		else
		{
			// If we get here, the weapon was not picked up yet. Just have it scale-pulse in place.
			s = 1.2f + Mathf.Sin(Time.time * 3.0f) * 0.2f;			
		}
		
		// Perform animations and pickup detection here
		if (s > 0)
		{			
			// Make the renderer size to scale and proportional to the icon size
			iconTransform.localScale = new Vector3(s, s * iconAspect, 1);
			
			// See if we intersected with the player; if so, flag ourselves as having been picked up
			if (!tPickedUp.HasValue 
				&& null != collider 
				&& null != playerCharacter.collider)
			{
				Bounds bounds = collider.bounds;
				bounds.Expand(1.0f);
				if (bounds.Intersects( playerCharacter.collider.bounds ))
				{
					// Begin the visual process of the player picking up the weapon
					pickupPosition = myTransform.position;
					rigidbody.isKinematic = true;
					tPickedUp = Time.time;
					
					// Give the player the drop
					GiveDropToPlayer();
				}
			}
		}
		else
		{
			// Shrinking is complete and now this object can be destroyed
			Destroy(gameObject);
		}
	}
	
	#endregion
	
	/// <summary>
	/// Called when the drop has been on the ground so long that it must be removed automatically.
	/// We can't just leave these in the scene forever.
	/// </summary>
	void SelfDestruct()
	{
		Destroy(gameObject);
	}		
	
	/// <summary>
	/// Called when the player runs into the drop
	/// </summary>
	void GiveDropToPlayer()
	{
		// Play the sound effect
		playerCharacter.PlaySound(sndDropPickup);
		
		// Now give them the drop
		switch (itemDrop.DropType)
		{
		case ItemDropType.Winchesta:
			GiveWeaponDropToPlayer(WeaponDirector.PlayerWeapon.Winchesta, 10);
			break;
		case ItemDropType.Shotgun:
			GiveWeaponDropToPlayer(WeaponDirector.PlayerWeapon.Shotgun, 50);
			break;
		case ItemDropType.RocketLauncher:
			GiveWeaponDropToPlayer(WeaponDirector.PlayerWeapon.RocketLauncher, 10);
			break;

		case ItemDropType.Ammo:
			GiveAmmoDropToPlayer();
			break;
			
		case ItemDropType.CowboyHat:
		case ItemDropType.EastwoodHat:
		case ItemDropType.TenGallonHat:
			GiveCowboyHatToPlayer();
			break;
			
		case ItemDropType.Bandana:
			GiveMouthpieceToPlayer();
			break;
		}
	}
	
	/// <summary>
	/// Called by GiveDropToPlayer to give a weapon to the player
	/// </summary>
	/// <param name='weapon'>
	/// Weapon.
	/// </param>
	/// <param name='defaultAmmo'>
	/// Default ammo.
	/// </param>
	void GiveWeaponDropToPlayer(WeaponDirector.PlayerWeapon weapon, int defaultAmmo)
	{	
		if (!PlayerDirector.ActiveSession.HasWeapon(weapon))
		{
			// Flag ourselves as now having this weapon
			PlayerDirector.ActiveSession.SetHasWeapon(weapon, true);
			// Now set the active weapon
			PlayerDirector.ActiveSession.CurrentWeapon = weapon;
			playerCharacter.SendMessage("SetActiveWeapon", "Player" + weapon.ToString());			
			PlayerDirector.ActiveSession.SetAmmo(weapon, defaultAmmo);
		}
		else
		{
			PlayerDirector.ActiveSession.IncreaseAmmo(weapon, defaultAmmo);
		}
	}
	
	/// <summary>
	/// Called by GiveDropToPlayer to give ammo to the player
	/// </summary>
	void GiveAmmoDropToPlayer()
	{
		switch (itemDrop.DropType)
		{
		// Ammo
		case ItemDropType.Ammo:
			for (int i=0; i < WeaponDirector.PlayerWeaponCount; i++)
			{
				WeaponDirector.PlayerWeapon playerWeapon = (WeaponDirector.PlayerWeapon)i;
				if (PlayerDirector.ActiveSession.HasWeapon(playerWeapon))
				{
					PlayerDirector.ActiveSession.IncreaseAmmo(playerWeapon,10);
				}
			}
			break;
		}
	}
	
	/// <summary>
	/// Called by GiveDropToPlayer to give a hat to the player
	/// </summary>
	void GiveCowboyHatToPlayer()
	{
		playerCharacter.SendMessage("SetActiveHat", itemDrop.DropType.ToString());
	}
	
	/// <summary>
	/// Called by GiveDropToPlayer to give a mouthpiece (bandana) to the player
	/// </summary>
	void GiveMouthpieceToPlayer()
	{
		playerCharacter.SendMessage("SetActiveMouthpiece", itemDrop.DropType.ToString());
	}
}