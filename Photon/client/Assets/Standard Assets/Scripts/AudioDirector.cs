using UnityEngine;
using System.Collections;

/// <summary>
/// The Audio director is responsible for managing music in the game as a persistent
/// object throughout each scene; but is otherwise hastily put together. 
/// 
/// In a better implementation, all sound effect playing would pass through here as well,
/// and it would work PlayerPrefs.GetFloat().
/// </summary>
public class AudioDirector : MonoBehaviour
{
	/// <summary>
	/// Gets the global AudioDirector instance.
	/// </summary>
	/// <value>
	/// The global AudioDirector instance.
	/// </value>
	static AudioDirector GlobalInstance
	{
		get {
			if (null == _globalInstance)
			{
				GameObject go = new GameObject();
				go.name = "AudioManager";
				DontDestroyOnLoad(go);
				go.AddComponent<AudioSource>();
				_globalInstance = go.AddComponent<AudioDirector>();
			}
			return _globalInstance;
		}
	}
	static AudioDirector _globalInstance;
	
	/// <summary>
	/// Gets the music audio source.
	/// </summary>
	/// <value>
	/// The music audio source.
	/// </value>
	static AudioSource MusicAudioSource
	{
		get {
			return GlobalInstance.GetComponent<AudioSource>();
		}
	}
		
	#region Public static members
	
	/// <summary>
	/// Plays the specified music
	/// </summary>
	/// <param name='audioClip'>
	/// The music
	/// </param>
	public static void PlayMusic(AudioClip audioClip)
	{
		AudioSource musicAudioSource = MusicAudioSource;
		musicAudioSource.Stop();
		musicAudioSource.clip = audioClip;
		musicAudioSource.loop = true;
		musicAudioSource.Play();
	}
	
	/// <summary>
	/// Stops the music from playing
	/// </summary>
	public static void StopMusic()
	{
		MusicAudioSource.Stop();
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
			return MusicAudioSource.volume;
		}
		set {
			MusicAudioSource.volume = value;
		}
	}

	/// <summary>
	/// Gets or sets the sound effect volume.
	/// </summary>
	/// <value>
	/// The sound effect volume.
	/// </value>
	public static float SFXVolume 
	{
		get {
			return GlobalInstance.sfxVolume;
		}
		set {
			// This is called from the MainMenuDirector and GameDirector as a player adjusts their preferences.
			GlobalInstance.sfxVolume = value;
		}
	}	
	
	#endregion
	
	/// <summary>
	/// The cached sound effect volume. This is changed by the SFXVolume setter.
	/// </summary>
	float sfxVolume;
}
