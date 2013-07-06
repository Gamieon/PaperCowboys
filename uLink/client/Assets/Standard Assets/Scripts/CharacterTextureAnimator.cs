using UnityEngine;
using System.Collections;

/// <summary>
/// This is a basic character texture animator. It's capable of rendering a character
/// in standstill form, animated moving form; and also supports texture scaling for
/// left and right directional movement.
/// </summary>
public class CharacterTextureAnimator : MonoBehaviour 
{
	/// <summary>
	/// The object is either moving or not moving
	/// </summary>
	public enum State { Still, Moving };
	
	/// <summary>
	/// The object animation frame will either be determined by time or by X-position on the screen
	/// </summary>
	public enum AnimationMotor { TimeDriven, XPosition };
		
	/// <summary>
	/// Determines how the animation will work (see enumeration)
	/// </summary>
	public AnimationMotor animationMotor;
	
	/// <summary>
	/// The material to show when the object is still
	/// </summary>
	public Material stillFrame;
	
	/// <summary>
	/// The materials to show when the object is moving
	/// </summary>
	public Material[] movingFrames;	
	
	/// <summary>
	/// The time phase for time-driven movement. If time is t, the motor pretends
	/// the time is (t + phase)
	/// </summary>
	public int phase;
	
	/// <summary>
	/// The primary character color. This is assigned to the material.
	/// </summary>
	public Color mainColor;
	
	/// <summary>
	/// The current moving/not-moving state
	/// </summary>
	public State state;
	
	/// <summary>
	/// The animation speed for time-driven animations
	/// </summary>
	public float speed;
	
	/// <summary>
	/// The texture scale. This is set by another component.
	/// </summary>
	public Vector2 textureScale = Vector2.one;
	
	/// <summary>
	/// The current animation frame.
	/// </summary>
	int currentFrame;	
	
	/// <summary>
	/// The time this object was instantiated; used for timed animations
	/// </summary>
	float t0;
	
	Renderer myRenderer;
	Transform myTransform;
		
	void Awake()
	{
		myRenderer = renderer;
		myTransform = transform;
		t0 = Time.time;
	}
	
	// Update is called once per frame
	void Update () 
	{
		switch (state)
		{
		case State.Still:
			currentFrame = 0;
			myRenderer.material = stillFrame;
			myRenderer.material.color = new Color(0,0,0,mainColor.a);
			myRenderer.material.SetColor("_Emission", mainColor);
			myRenderer.material.mainTextureScale = textureScale;
			break;
		case State.Moving:
			if (movingFrames.Length > 0)
			{
				switch (animationMotor)
				{
				case AnimationMotor.TimeDriven:
					currentFrame = (Mathf.FloorToInt((Time.time - t0) * speed) + phase) % movingFrames.Length;
					break;
				case AnimationMotor.XPosition:
					currentFrame = Mathf.CeilToInt(Mathf.Abs(myTransform.position.x) * 4.0f) % movingFrames.Length;
					if (myRenderer.material.mainTextureScale.x > 0) {
						currentFrame = (movingFrames.Length-1) - currentFrame;
					}
					break;
				}
				myRenderer.material = movingFrames[currentFrame];
				myRenderer.material.SetColor("_Emission", mainColor);
				myRenderer.material.mainTextureScale = textureScale;
			}
			break;	
		}
	}
}
