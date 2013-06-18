using UnityEngine;
using System.Collections;

/// <summary>
/// This class is attached to an empty GameObject that is the parent of the camera in a scene.
/// Its primary purpose is to manage camera shaking.
/// </summary>
public class GameCameraDirector : MonoBehaviour
{
	/// <summary>
	/// Gets the game camera director instance.
	/// </summary>
	/// <value>
	/// The game camera director instance.
	/// </value>
	static public GameCameraDirector Instance
	{
		get {
			object gameCameraDirector = Object.FindObjectOfType(typeof(GameCameraDirector));
			return (GameCameraDirector)gameCameraDirector;
		}
	}
	
	/// <summary>
	/// The camera shake radius.
	/// </summary>
	float shakeRadius = 0;
	
	#region MonoBehavior
	
	// Update is called once per frame
	void Update () 
	{
		if (shakeRadius > 0)
		{
			shakeRadius -= Time.deltaTime * 5.0f;
			transform.localPosition = new Vector3(Random.Range(-1,1), Random.Range(-1,1), Random.Range(-1,1)) * shakeRadius;
		}
	}
	
	#endregion
	
	public void ShakeCamera()
	{
		shakeRadius += 2.0f;
	}
}
