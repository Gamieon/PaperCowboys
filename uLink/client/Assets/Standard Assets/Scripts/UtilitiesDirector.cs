using UnityEngine;
using System.Collections;

/// <summary>
/// General non-state-based utility functions should go in here
/// </summary>
public static class UtilitiesDirector
{
	/// <summary>
	/// Assigns the layer for a tree of game objects
	/// </summary>
	/// <param name='t'>
	/// The root transform
	/// </param>
	/// <param name='layer'>
	/// The layer
	/// </param>
	public static void SetLayerRecurse(Transform t, int layer)
	{
		// Check if the layer is a default layer first. If it's not the default,
		// then it must be reserved for something else; so leave it alone.
		if (0 == t.gameObject.layer)
		{
			t.gameObject.layer = layer;
		}
		foreach (Transform c in t)
		{
			SetLayerRecurse(c, layer);
		}
	}
}
