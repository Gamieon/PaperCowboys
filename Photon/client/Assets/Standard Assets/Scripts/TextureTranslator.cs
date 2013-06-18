using UnityEngine;
using System.Collections;

/// <summary>
/// This component will make the attached renderer's material's texture transform
/// translate over time. In this project, it is used to make the train tracks move
/// quickly to give the illusion the players are riding fast.
/// </summary>
public class TextureTranslator : MonoBehaviour 
{
	/// <summary>
	/// The velocity of which to translate the texture.
	/// </summary>
	public Vector2 velocity;
	
	Renderer myRenderer;
	
	void Awake()
	{
		myRenderer = renderer;
	}
	
	void Update()
	{
		myRenderer.material.mainTextureOffset += velocity * Time.deltaTime;
	}
}
