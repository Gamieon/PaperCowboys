using UnityEngine;
using System.Collections;

/// <summary>
/// This component makes a non-character object flicker when they get hit.
/// </summary>
public class MeshFlasher : MonoBehaviour 
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
	
	Renderer myRenderer;
	
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
			existingEmissiveColor = renderer.material.GetColor("_Emission");
			myRenderer.material.SetColor("_Emission", emissiveColor);
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
			myRenderer.material.SetColor("_Emission", existingEmissiveColor);
			emissiveColorActive = false;
		}		
	}
	
	#region MonoBehavior

	// Use this for initialization
	void Start() 
	{
		myRenderer = renderer;
	}
	
	#endregion
}
