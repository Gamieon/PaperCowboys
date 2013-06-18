using System.Collections;
using UnityEngine;

public class GameManager : Photon.MonoBehaviour
{
    public Transform playerPrefab;

    public void Awake()
    {
        // PhotonNetwork.logLevel = NetworkLogLevel.Full;
        if (!PhotonNetwork.connected)
        {
            // We must be connected to a photon server! Back to main menu
            Application.LoadLevel(Application.loadedLevel - 1);
            return;
        }

        PhotonNetwork.isMessageQueueRunning = true;

        // Spawn our local player
        PhotonNetwork.Instantiate(this.playerPrefab.name, transform.position, Quaternion.identity, 0);
    }

    public void OnGUI()
    {
        if (GUILayout.Button("Return to Lobby"))
        {
            PhotonNetwork.LeaveRoom();
        }
    }

    public void OnLeftRoom()
    {
        Debug.Log("OnLeftRoom (local)");
        
        // back to main menu        
        Application.LoadLevel(Application.loadedLevelName);
    }

    public void OnMasterClientSwitched(PhotonPlayer player)
    {
        Debug.Log("OnMasterClientSwitched: " + player);

        if (PhotonNetwork.connected)
        {
            photonView.RPC("SendChatMessage", PhotonNetwork.masterClient, "Hi master! From:" + PhotonNetwork.player);
            photonView.RPC("SendChatMessage", PhotonTargets.All, "WE GOT A NEW MASTER: " + player + "==" + PhotonNetwork.masterClient + " From:" + PhotonNetwork.player);
        }
    }

    public void OnDisconnectedFromPhoton()
    {
        Debug.Log("OnDisconnectedFromPhoton");

        // Back to main menu        
        Application.LoadLevel(Application.loadedLevelName);
    }

    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerConnected: " + player);
    }

    public void OnPhotonPlayerDisconnected(PhotonPlayer player)
    {
        Debug.Log("OnPlayerDisconneced: " + player);
    }

    public void OnReceivedRoomList()
    {
        Debug.Log("OnReceivedRoomList");
    }

    public void OnReceivedRoomListUpdate()
    {
        Debug.Log("OnReceivedRoomListUpdate");
    }

    public void OnConnectedToPhoton()
    {
        Debug.Log("OnConnectedToPhoton");
    }

    public void OnFailedToConnectToPhoton()
    {
        Debug.Log("OnFailedToConnectToPhoton");
    }

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("OnPhotonInstantiate " + info.sender);
    }
}
