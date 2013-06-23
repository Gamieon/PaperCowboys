using UnityEngine;
using System.Collections;

/// <summary>
/// Local player-specific preferences are managed here.
/// </summary>
public static class PlayerDirector
{
	/// <summary>
	/// Gets or sets the name of the player.
	/// </summary>
	/// <value>
	/// The name of the player.
	/// </value>
	public static string Name
	{
		get {
			string result = PlayerPrefs.GetString("Player.Name", "");
			if (string.IsNullOrEmpty(result)) {
				Name = result = "Guest" + Random.Range(1, 9999);
			}
			return result;
		}
		set {
			PlayerPrefs.SetString("Player.Name", value);
		}
	}
	
	/// <summary>
	/// Gets or sets the hue of the player color.
	/// </summary>
	/// <value>
	/// The hue.
	/// </value>
	public static float Hue
	{
		get {
			float result = PlayerPrefs.GetFloat("Player.Hue", -1);
			if (-1 == result)
			{
				Hue = result = Random.Range(0.0f, 1.0f);
			}
			return result;
		}
		set {
			PlayerPrefs.SetFloat("Player.Hue", value);
		}
	}
	
	/// <summary>
	/// Gets the color of the player.
	/// </summary>
	/// <value>
	/// The color of the player.
	/// </value>
	public static Color PlayerColor
	{
		get {
			return ColorDirector.H2RGB(Hue);
		}
	}
	
	/// <summary>
	/// Active session properties
	/// </summary>
	public class ActiveSession
	{
		/// <summary>
		/// Gets or sets the score for the player since joining a game room
		/// </summary>
		/// <value>
		/// The active score.
		/// </value>
		public static int Score { get; set; }
		
		/// <summary>
		/// Determines if this player has a weapon in this active sesson
		/// </summary>
		/// <returns>
		/// True if the player has the weapon
		/// </returns>
		/// <param name='weapon'>
		/// If set to <c>true</c> weapon.
		/// </param>
		public static bool HasWeapon(WeaponDirector.PlayerWeapon weapon)
		{
			if (WeaponDirector.PlayerWeapon.Pistol == weapon) 
			{
				return true;
			}
			else
			{
				return _availableWeapons[(int)weapon];
			}
		}

		/// <summary>
		/// Gives or takes a weapon from the player
		/// </summary>
		/// <param name='weapon'>
		/// Weapon.
		/// </param>
		/// <param name='hasWeapon'>
		/// Has weapon.
		/// </param>
		public static void SetHasWeapon(WeaponDirector.PlayerWeapon weapon, bool hasWeapon)
		{
			if (WeaponDirector.PlayerWeapon.Pistol != weapon)
			{
				_availableWeapons[(int)weapon] = hasWeapon;
			}
		}
		
		/// <summary>
		/// Gets the available weapon count.
		/// </summary>
		/// <value>
		/// The available weapon count.
		/// </value>
		public static int AvailableWeaponCount
		{
			get 
			{
				int count = 0;
				for (int i=0; i < (int)WeaponDirector.PlayerWeaponCount; i++)
				{
					if (HasWeapon((WeaponDirector.PlayerWeapon)i))
					{
						count++;
					}
				}
				return count;
			}
		}
		
		static bool[] _availableWeapons = new bool[WeaponDirector.PlayerWeaponCount];
		
		/// <summary>
		/// Gets or sets the active weapon.
		/// </summary>
		/// <value>
		/// The active weapon.
		/// </value>
		public static WeaponDirector.PlayerWeapon CurrentWeapon
		{
			get {
				return _currentWeapon;
			}
			set
			{
				_currentWeapon = value;
			}
		}
		
		static WeaponDirector.PlayerWeapon _currentWeapon = WeaponDirector.PlayerWeapon.Pistol;
		
		/// <summary>
		/// Gets the player's active ammo.
		/// </summary>
		/// <returns>
		/// The ammo.
		/// </returns>
		/// <param name='weapon'>
		/// Weapon.
		/// </param>
		public static int GetAmmo(WeaponDirector.PlayerWeapon weapon)
		{
			if (WeaponDirector.PlayerWeapon.Pistol == weapon) 
			{
				return 1; // Infinite ammo; will never be zero
			}
			return _ammo[(int)weapon];
		}
		
		/// <summary>
		/// Sets the player's active ammo.
		/// </summary>
		/// <param name='weapon'>
		/// Weapon.
		/// </param>
		/// <param name='ammoValue'>
		/// Ammo value.
		/// </param>
		public static void SetAmmo(WeaponDirector.PlayerWeapon weapon, int ammoAmount)
		{
			if (WeaponDirector.PlayerWeapon.Pistol != weapon) 
			{
				_ammo[(int)weapon] = ammoAmount;
			}
		}
		
		/// <summary>
		/// Increases the player's active ammo.
		/// </summary>
		/// <param name='weapon'>
		/// Weapon.
		/// </param>
		/// <param name='ammoValue'>
		/// Ammo value.
		/// </param>
		public static void IncreaseAmmo(WeaponDirector.PlayerWeapon weapon, int ammoAmount)
		{
			if (WeaponDirector.PlayerWeapon.Pistol != weapon) 
			{
				_ammo[(int)weapon] += ammoAmount;
			}
		}		
		
		static int[] _ammo = new int[WeaponDirector.PlayerWeaponCount];
		
		/// <summary>
		/// Takes all bullets and weapons away from the player. A session within the
		/// Unity editor self will give the player all weapons and ammo.
		/// </summary>
		public static void ResetPlayerMunitions()
		{
			// Reset local ammo stores. Ammo is maintained locally for the moment.
			for (int i=0; i < WeaponDirector.PlayerWeaponCount; i++)
			{
#if UNITY_EDITOR
				ActiveSession.SetHasWeapon((WeaponDirector.PlayerWeapon)i, true);
				ActiveSession.SetAmmo((WeaponDirector.PlayerWeapon)i, 100);			
#else
				ActiveSession.SetHasWeapon((WeaponDirector.PlayerWeapon)i, false);
				ActiveSession.SetAmmo((WeaponDirector.PlayerWeapon)i, 0);
#endif
			}
			CurrentWeapon = WeaponDirector.PlayerWeapon.Pistol;
		}
	}
	
	/// <summary>
	/// This must be called before joining a game
	/// </summary>
	public static void PopulateCustomPlayerProperties()
	{
		Hashtable hashTable = new Hashtable();
		hashTable.Add("Player.Name", Name);
		hashTable.Add("Player.Hue", Hue);
		hashTable.Add("Player.ActiveSession.Score", (ActiveSession.Score = 0));
		NetworkDirector.SetPlayerCustomProperties(hashTable);
		// Reset local ammo stores. Ammo is maintained locally for the moment.
		ActiveSession.ResetPlayerMunitions();
	}	
}
