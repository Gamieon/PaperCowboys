using UnityEngine;
using System.Collections;

public class EnemyBoss : EnemyCharacter 
{
	protected float maxHitpoints;
	
	public GUIStyle bossHealthStyle;
	
	public Texture2D boxTextureSolid;
	
	#region MonoBehavior
	
	protected override void Start() 
	{
		base.Start();
		// Retain the max hitpoint count
		maxHitpoints = hitPoints;
	}

	protected virtual void OnGUI()
	{
		Rect bossHealthRect = new Rect(Screen.width*0.2f, 30, Screen.width*0.6f, 30);
		Rect labelRect = new Rect(0,0,120,bossHealthRect.height);
		Rect barRect = new Rect(120,0,bossHealthRect.width-labelRect.width,bossHealthRect.height);
		
		GUI.BeginGroup(new Rect(Screen.width*0.2f, Screen.height - 50, Screen.width*0.6f, 80));
		// Draw the "BOSS" label
		GUI.Label(labelRect, "BOSS", bossHealthStyle);
		// Draw the health bar outline
		GUI.color = new Color(1,1,1,0.4f);
		GUI.DrawTexture(barRect, boxTextureSolid);
		// Draw the health remaining
		GUI.color = new Color(1,0,0,0.4f);
		GUI.DrawTexture(new Rect(barRect.xMin+1, barRect.yMin+1,
			(hitPoints / maxHitpoints) * (barRect.width-2), barRect.height-2), boxTextureSolid);
		GUI.EndGroup();
	}	
	
	#endregion
}
