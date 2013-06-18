using System.Threading;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class TestGuiInstantiate : Photon.MonoBehaviour
{
    public string PrefabToInstantiate = "BoxPrefab";
    public bool HideUI = false;
    public int GuiSpace = 0;

    private GameObject lastInstantiateMine;
    private GameObject lastInstantiateScene;
    private int prefix;

    public void OnGUI()
    {
        if (HideUI)
        {
            return;
        }
        GUILayout.Space(GuiSpace);

        GUILayout.Label(PhotonNetwork.connectionStateDetailed.ToString());

        if (PhotonNetwork.connectionState == ConnectionState.Disconnected)
        {
            if (GUILayout.Button("Connect"))
            {
                PhotonNetwork.ConnectUsingSettings("1");
            }
        }
        else
        {
            if (GUILayout.Button("Disconnect"))
            {
                PhotonNetwork.Disconnect();
            }
        }
    }
}
