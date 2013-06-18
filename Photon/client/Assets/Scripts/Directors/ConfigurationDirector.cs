using UnityEngine;
using System.Collections;

/// <summary>
/// Local game preferences are managed here.
/// </summary>
public static class ConfigurationDirector
{
	public static class Host
	{
		/// <summary>
		/// Gets or sets the max players.
		/// </summary>
		/// <value>
		/// The max players.
		/// </value>
		public static int MaxPlayers
		{
			get {
				return PlayerPrefs.GetInt("Host.MaxPlayers", 8);
			}
			set {
				PlayerPrefs.SetInt("Host.MaxPlayers", value);
			}
		}		
		
		/// <summary>
		/// Gets or sets the name of the private room.
		/// </summary>
		/// <value>
		/// The name of the private room.
		/// </value>
		public static string RoomName
		{
			get {
				return PlayerPrefs.GetString("Host.RoomName", "");
			}
			set {
				PlayerPrefs.SetString("Host.RoomName", value);
			}			
		}
	}
	
	/// <summary>
	/// Audio configuration settings
	/// </summary>
	public static class Audio
	{
		/// <summary>
		/// Gets or sets the SFX volume.
		/// </summary>
		/// <value>
		/// The SFX volume.
		/// </value>
		public static float SFXVolume
		{
			get {
				return PlayerPrefs.GetFloat("Audio.SFXVolume", 0.8f);
			}
			set {
				PlayerPrefs.SetFloat("Audio.SFXVolume", value);
			}
		}
		
		/// <summary>
		/// Gets or sets the music volume.
		/// </summary>
		/// <value>
		/// The music volume.
		/// </value>
		public static float MusicVolume
		{
			get {
				return PlayerPrefs.GetFloat("Audio.MusicVolume", 0.5f);
			}
			set {
				PlayerPrefs.SetFloat("Audio.MusicVolume", value);
			}
		}	
	}
	
	/// <summary>
	/// Gets or sets a value indicating whether this instance has seen instructions.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance has seen instructions; otherwise, <c>false</c>.
	/// </value>
	public static bool HasSeenInstructions
	{
		get {
			return (PlayerPrefs.GetInt("HasSeenInstructions", 0) == 0) ? false : true;
		}
		set {
			PlayerPrefs.SetInt("HasSeenInstructions", value ? 1 : 0);
		}
	}	
}