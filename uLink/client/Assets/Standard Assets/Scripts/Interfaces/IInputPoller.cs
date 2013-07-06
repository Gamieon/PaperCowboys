using UnityEngine;

/// <summary>
/// A component with this interface is one that accepts user inputs, such as keystrokes, mouse
/// movements and gamepad buttons. There should only be one component polling for inputs at a time.
/// Otherwise the game can get mixed up. For instance, a mouse click could be construed as both
/// a button press and the player firing their weapon.
/// 
/// It is intended for this project that a component which inherits an interface will
/// not have a subclass. I find the project easier to manage when there are no components
/// with interfaces that have subclasses which may need to have their own overloads to
/// call when interface functions are invoked.
/// </summary>
public interface IInputPoller 
{	
	/// <summary>
	/// Called to begin polling this input object
	/// </summary>
	/// <returns>
	/// False if this object cannot be made active
	/// </returns>	
	bool BeginPolling();
	
	/// <summary>
	/// Poll is called once per frame if this object has input focus. This
	/// function transforms the camera based on user inputs
	/// </summary>
	/// <returns>
	/// False if this object is no longer active
	/// </returns>	
	bool Poll();
	
	/// <summary>
	/// Called when polling has finished
	/// </summary>	
	void EndPolling();
	
	/// <summary>
	/// Called to render the GUI
	/// </summary>
	/// <param name='screenArea'>
	/// The available screen area. You should call GUI.BeginGroup before rendering.
	/// </param>
	void RenderGUI(Rect screenArea);
}
