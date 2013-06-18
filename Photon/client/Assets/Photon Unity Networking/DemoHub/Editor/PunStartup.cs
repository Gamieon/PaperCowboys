using System.IO;
using UnityEngine;
using System.Collections;
using UnityEditor;

[InitializeOnLoad]
public class PunStartup : MonoBehaviour
{
    private static bool runOnce;

    // paths to demo scenes, to be included in a build. relative to "Assets/Photon Unity Networking/"
    private static string[] demoPaths = { "DemoHub/DemoHub-Scene.unity", "DemoBoxes/DemoBoxes-Scene.unity", "DemoWorker/DemoWorker-Scene.unity", "MarcoPolo-Tutorial/MarcoPolo-Scene.unity", "DemoSynchronization/DemoSynchronization-Scene.unity" };

    static PunStartup()
    {
        EditorApplication.update += OnLevelWasLoaded;
        runOnce = true;
    }

    static void OnLevelWasLoaded()
    {
        if (!runOnce) 
        {
            return;
        }

        runOnce = false;
        EditorApplication.update -= OnLevelWasLoaded;

        if (string.IsNullOrEmpty(EditorApplication.currentScene))
        {
            bool ret = EditorApplication.OpenScene("Assets/Photon Unity Networking/" + demoPaths[0]);
            if (ret)
            {
                Debug.Log("No scene was open. Loaded PUN Demo Hub. " + EditorApplication.currentScene);
                Selection.activeObject = AssetDatabase.LoadMainAssetAtPath("Assets/Photon Unity Networking/" + demoPaths[0]);
            }
        }

        if (EditorBuildSettings.scenes.Length == 0)
        {
            EditorBuildSettingsScene[] scenes = new EditorBuildSettingsScene[demoPaths.Length];
            for (int i = 0; i < demoPaths.Length; i++)
            {
                scenes[i] = new EditorBuildSettingsScene("Assets/Photon Unity Networking/" + demoPaths[i], true);
            }
            EditorBuildSettings.scenes = scenes;

            Debug.Log("Applied new scenes to build settings.");
        }
	}
}