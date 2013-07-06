using UnityEngine;
using System.Collections;

/// <summary>
/// This component makes the attached mesh grow and shrink in regular intervals
/// </summary>
public class MeshPulser : MonoBehaviour 
{
	/// <summary>
	/// The rate at which the mesh pulses
	/// </summary>
	public float speed;
	
	/// <summary>
	/// The size of the pulse
	/// </summary>
	public float amplitude;
	
	Transform myTransform;
	Vector3 originalScale;

	// Use this for initialization
	void Start () 
	{
		myTransform = transform;
		originalScale = myTransform.localScale;
	}
	
	// Update is called once per frame
	void Update () 
	{
		// Note: This can be slow on mobile devices if the attached object has a collider
		myTransform.localScale = originalScale * (1.0f + Mathf.Sin(Time.time * speed) * amplitude);
	}
}
