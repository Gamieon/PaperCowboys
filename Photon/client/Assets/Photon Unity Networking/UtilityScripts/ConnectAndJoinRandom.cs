using System;
using UnityEngine;
using System.Collections;

/// <summary>
/// This script automatically connects to Photon (using the settings file), 
/// tries to join a random room and creates one if none was found (which is ok).
/// </summary>
public class ConnectAndJoinRandom : Photon.MonoBehaviour
{
    public bool AutoConnect = false;
    public int GuiSpace = 0;
    private bool ConnectInUpdate = true;

	// Use this for initialization
    public virtual void Start ()
    {
        PhotonNetwork.autoJoinLobby = false;
	}

    void Update()
    {
        if (ConnectInUpdate && AutoConnect)
        {
            ConnectInUpdate = false;

            Debug.Log("Update() was called by Unity. Scene is loaded. Let's connect to the Photon Master Server. Calling: PhotonNetwork.JoinRandomRoom();");
            PhotonNetwork.ConnectUsingSettings("1");
        }
    }
	
    public virtual void OnConnectedToMaster()
    {
        Debug.Log("OnConnectedToMaster() was called by PUN. Now this client is connected and could join a room. Calling: PhotonNetwork.JoinRandomRoom();");
        PhotonNetwork.JoinRandomRoom();
    }

    public virtual void OnPhotonRandomJoinFailed()
    {
        Debug.Log("OnPhotonRandomJoinFailed() was called by PUN. No random room available, so we create one. Calling: (null, true, true, 4);");
        PhotonNetwork.CreateRoom(null, true, true, 4);
    }

    public void OnJoinedRoom()
    {
        Debug.Log("OnJoinedRoom() called by PUN. Now this client is in a room. From here on, your game would be running. For reference, all callbacks are listed in enum: PhotonNetworkingMessage");
    }
}
