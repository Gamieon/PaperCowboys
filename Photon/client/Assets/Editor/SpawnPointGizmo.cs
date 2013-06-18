using UnityEngine;
using UnityEditor;
using System.Collections;

public class EnemySpawnGizmo : MonoBehaviour 
{
	[DrawGizmo (GizmoType.NotSelected | GizmoType.Pickable)]
    static void RenderSpawnGizmo (SpawnPoint spawnPoint, GizmoType gizmoType) 
	{
        // Draw the icon
		string iconName = spawnPoint.gizmoIcon;
		if (string.IsNullOrEmpty(iconName)) { iconName = "EnemySpawn"; }
        Gizmos.DrawIcon (spawnPoint.transform.position, iconName, true);
    }

	[DrawGizmo (GizmoType.NotSelected | GizmoType.Pickable)]
    static void RenderCheckpointGizmo (Checkpoint checkpoint, GizmoType gizmoType) 
	{
        // Draw the icon
        Gizmos.DrawIcon (checkpoint.transform.position, checkpoint.gizmoIcon, true);
    }
}
