using UnityEngine;
using System.Collections;

/// <summary>
/// This component makes a character object flicker when they get hit.
/// </summary>
public class CharacterTextureFlasher : MonoBehaviour 
{
	/// <summary>
	/// The color of the flash (usually white)
	/// </summary>
	public Color emissiveColor;	
	
	/// <summary>
	/// The color of the material when it's not flashing (note: if you have other
	/// components that also modify the color of the same material, then you need
	/// to coordinate all of them).
	/// </summary>
	Color existingEmissiveColor;
	
	/// <summary>
	/// True if the emissive color is active (true if we're flashing)
	/// </summary>
	bool emissiveColorActive;
	
	/// <summary>
	/// The character texture animator which is "king" of determining the material
	/// color. Rather than coloring the material directly, we tell the character texture
	/// animator to do it.
	/// </summary>
	CharacterTextureAnimator characterTextureAnimator;
	
	/// <summary>
	/// Flashs the object for the specified duration.
	/// </summary>
	/// <param name='duration'>
	/// Duration.
	/// </param>
	public void Flash(float duration)
	{
		if (!emissiveColorActive)
		{
			existingEmissiveColor = characterTextureAnimator.mainColor;
			characterTextureAnimator.mainColor = emissiveColor;
			emissiveColorActive = true;
			Invoke("Unflash", duration);
		}
	}
	
	/// <summary>
	/// Turns off the flash after the flash duration elapses
	/// </summary>
	void Unflash()
	{
		if (emissiveColorActive)
		{
			characterTextureAnimator.mainColor = existingEmissiveColor;
			emissiveColorActive = false;
		}		
	}
	
	#region MonoBehavior

	// Use this for initialization
	void Start() 
	{
		characterTextureAnimator = GetComponent<CharacterTextureAnimator>();
	}
	
	#endregion
}
