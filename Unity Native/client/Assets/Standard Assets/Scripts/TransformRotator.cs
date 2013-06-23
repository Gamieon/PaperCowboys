using UnityEngine;
using System.Collections;

/// <summary>
/// This component will make the attached object rotate over time.
/// </summary>
public class TransformRotator : MonoBehaviour 
{
	/// <summary>
	/// The rotation velocity.
	/// </summary>
	public Vector3 velocity;
	
	Transform myTransform;
	float t0;
	
	void Awake()
	{
		myTransform = transform;
		t0 = Time.time;
	}
	
	void Update()
	{
		myTransform.localEulerAngles = velocity * (Time.time - t0);
	}
}
