using UnityEngine;
using System.Collections;

/// <summary>
/// This component should be attached to a GameObject in the "Startup" scene to take
/// the player directly to the main menu. 
/// 
/// The point of the Starup scene is to ensure that the NetworkLevelLoader game object 
/// exists once and only once throughout all scenes without requiring a dynamic creation
/// through a call to Instantiate. Because the NetworkLevelLoader component is set to
/// DontDestroyOnLoad, we must be sure to never again revisit the scene it was created
/// in or else it will duplicate.
/// </summary>
public class StartupScene : MonoBehaviour 
{
	void Start()
	{
		Application.LoadLevel("MainMenu");
	}
}
