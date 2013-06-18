// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MainMenu.cs" company="Exit Games GmbH">
//   Part of: Photon Unity Networking
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


using System.Collections;
using System.Threading;
using UnityEngine;

public class MainMenu : MonoBehaviour
{
    private string roomName = "myRoom";

    private Vector2 scrollPos = Vector2.zero;

    private bool connectFailed = false;

    private void Awake()
    {
		// Ensure scenes are automatically synced
		PhotonNetwork.automaticallySyncScene = true;
		
        // Connect to the main photon server. This is the only IP and port we ever need to set(!)
        if (!PhotonNetwork.connected)
        {
            PhotonNetwork.ConnectUsingSettings("1.0");
        }

        // PhotonNetwork.logLevel = NetworkLogLevel.Full; // turn on if needed

        // generate a name for this player, if none is assigned yet
        if (string.IsNullOrEmpty(PhotonNetwork.playerName))
        {
            PhotonNetwork.playerName = "Guest" + Random.Range(1, 9999);
        }
    }

    private void OnGUI()
    {
        if (!PhotonNetwork.connected)
        {
            if (PhotonNetwork.connectionState == ConnectionState.Connecting)
            {
                GUILayout.Label("Connecting " + PhotonNetwork.networkingPeer.ServerAddress);
                GUILayout.Label(Time.time.ToString());
            }
            else
            {
                GUILayout.Label("Not connected. Check console output.");
            }

            if (this.connectFailed)
            {
                GUILayout.Label("Connection failed. Check setup and use Setup Wizard to fix configuration.");
                GUILayout.Label(string.Format("Server: {0}:{1}", new object[] {PhotonNetwork.PhotonServerSettings.ServerAddress, PhotonNetwork.PhotonServerSettings.ServerPort}));
                GUILayout.Label("AppId: " + PhotonNetwork.PhotonServerSettings.AppID);
                
                if (GUILayout.Button("Try Again", GUILayout.Width(100)))
                {
                    this.connectFailed = false;
                    PhotonNetwork.ConnectUsingSettings("1.0");
                }
            }

            return;
        }


        GUI.skin.box.fontStyle = FontStyle.Bold;
        GUI.Box(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 300), "Join or Create a Room");
        GUILayout.BeginArea(new Rect((Screen.width - 400) / 2, (Screen.height - 350) / 2, 400, 300));

        GUILayout.Space(25);

        // Player name
        GUILayout.BeginHorizontal();
        GUILayout.Label("Player name:", GUILayout.Width(100));
        PhotonNetwork.playerName = GUILayout.TextField(PhotonNetwork.playerName);
        GUILayout.Space(105);
        if (GUI.changed)
        {
            // Save name
            PlayerPrefs.SetString("playerName", PhotonNetwork.playerName);
        }
        GUILayout.EndHorizontal();

        GUILayout.Space(15);

        // Join room by title
        GUILayout.BeginHorizontal();
        GUILayout.Label("Roomname:", GUILayout.Width(100));
        this.roomName = GUILayout.TextField(this.roomName);
        
        if (GUILayout.Button("Create Room", GUILayout.Width(100)))
        {
            PhotonNetwork.CreateRoom(this.roomName, true, true, 10);
        }

        GUILayout.EndHorizontal();

        // Create a room (fails if exist!)
        GUILayout.BeginHorizontal();
        GUILayout.FlexibleSpace();
        //this.roomName = GUILayout.TextField(this.roomName);
        if (GUILayout.Button("Join Room", GUILayout.Width(100)))
        {
            PhotonNetwork.JoinRoom(this.roomName);
        }

        GUILayout.EndHorizontal();


        GUILayout.Space(15);

        // Join random room
        GUILayout.BeginHorizontal();

        GUILayout.Label(PhotonNetwork.countOfPlayers + " users are online in " + PhotonNetwork.countOfRooms + " rooms.");
        GUILayout.FlexibleSpace();
        if (GUILayout.Button("Join Random", GUILayout.Width(100)))
        {
            PhotonNetwork.JoinRandomRoom();
        }
        

        GUILayout.EndHorizontal();

        GUILayout.Space(15);
        if (PhotonNetwork.GetRoomList().Length == 0)
        {
            GUILayout.Label("Currently no online games are available.");
            GUILayout.Label("Rooms will be listed here, when they become available.");
        }
        else
        {
            GUILayout.Label(PhotonNetwork.GetRoomList() + " currently available. Join either:");

            // Room listing: simply call GetRoomList: no need to fetch/poll whatever!
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
            foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(roomInfo.name + " " + roomInfo.playerCount + "/" + roomInfo.maxPlayers);
                if (GUILayout.Button("Join"))
                {
                    PhotonNetwork.JoinRoom(roomInfo.name);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }

        GUILayout.EndArea();
    }

    // We have two options here: we either joined(by title, list or random) or created a room.
    private void OnJoinedRoom()
    {
        //this.StartCoroutine(this.MoveToGameScene());
    }

    private void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
        this.StartCoroutine(this.MoveToGameScene());
    }

    private void OnDisconnectedFromPhoton()
    {
        Debug.Log("Disconnected from Photon.");
    }

    private void OnFailedToConnectToPhoton(object parameters)
    {
        this.connectFailed = true;
        Debug.Log("OnFailedToConnectToPhoton. StatusCode: " + parameters);
    }

    private IEnumerator MoveToGameScene()
    {
        Debug.Log("MoveToGameScene");
        while (PhotonNetwork.room == null)
        {
            yield return 0;
        }

        // Temporary disable processing of futher network messages
        PhotonNetwork.isMessageQueueRunning = false;
        Application.LoadLevel(Application.loadedLevelName);
    }
}