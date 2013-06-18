using System;
using UnityEngine;
using System.Collections;

public class TestBase : Photon.MonoBehaviour
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
            PhotonNetwork.ConnectUsingSettings("1");
        }
    }
	
    public virtual void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomRoom();
    }

    public virtual void OnPhotonRandomJoinFailed()
    {
        PhotonNetwork.CreateRoom(null, true, true, 4);
    }
}
