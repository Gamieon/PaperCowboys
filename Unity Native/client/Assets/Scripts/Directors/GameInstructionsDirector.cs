using UnityEngine;
using System.Collections;

/// <summary>
/// This component is responsible for making the game instructions appear for a player when
/// the GameDirector component tells it to. 
/// </summary>
public class GameInstructionsDirector : MonoBehaviour 
{
	/// <summary>
	/// The instruction texture.
	/// </summary>
	public Texture2D instructionTexture;
	
	/// <summary>
	/// Null if the instructions are invisible, or the time the instructions were displayed.
	/// </summary>
	float? tDisplay = null;
	
	/// <summary>
	/// Called by the GameDirector to display the game instructions
	/// </summary>
	public void Display()
	{
		// Just set the display time; the RenderInstructions call will do the rest
		tDisplay = Time.time;
	}
	
	/// <summary>
	/// Called from the GameDirector's OnGUI() event to render the instructions
	/// </summary>
	public void RenderInstructions()
	{
		// If the instructions are visible, make them drop down, then pull up after a couple seconds
		if (tDisplay.HasValue)
		{
			float size = 300;
			Rect instructionsRect = new Rect(0,0,size,size * (float)instructionTexture.height / (float)instructionTexture.width);
			
			float dt = Time.time - tDisplay.Value; // How many seconds have the instructions been visible
			if (dt < 1)
			{
				// Make the instructions appear
				float d = (1.0f - dt) * 4.0f;
				if (d < 0) { d = 0; }
				instructionsRect.center = new Vector2(size * 0.5f, size * 0.5f - d * size);
				
			}
			else if (dt > 7)
			{
				// Make the instructions disappear
				float d = (dt - 7.0f) * 4.0f;
				instructionsRect.center = new Vector2(size * 0.5f, size * 0.5f - d * size);
			}
			if (dt > 8)
			{
				// The sequence is complete and the instructions should be off-screen
				tDisplay = null;
			}

			GUI.color = Color.white;
			GUI.DrawTexture(instructionsRect, instructionTexture);			
		}
		else
		{
			// Instructions should not be visible, so don't draw them
		}
	}

}
