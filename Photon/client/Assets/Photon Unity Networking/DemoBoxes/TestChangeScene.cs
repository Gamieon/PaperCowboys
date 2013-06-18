using UnityEngine;
using System.Collections;

public class TestChangeScene : MonoBehaviour {

    public bool HideUI = false;
    public int GuiSpace = 200;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public void OnGUI()
    {
        if (HideUI)
        {
            return;
        }
        GUILayout.Space(GuiSpace);
        GUILayout.BeginHorizontal();
        GUILayout.Label("Load Scene");
        if (GUILayout.Button("one"))
        {
            Application.LoadLevel("00 TestScene1");
        }
        if (GUILayout.Button("two"))
        {
            Application.LoadLevel("00 TestScene2");
        }

        GUILayout.EndHorizontal();
    }
}
