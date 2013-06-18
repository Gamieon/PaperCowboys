using UnityEngine;
using System.Collections;

/// <summary>
/// Main menu management director. Pretty much everything that goes
/// on in the main menu is controlled from here.
/// </summary>
public class MainMenuDirector : MonoBehaviour 
{
	/// <summary>
	/// Menu layer (you can drill down into submenus)
	/// </summary>
	enum MenuLayer
	{
		MainMenu,
		OptionsMenu,
		HostGameMenu,
		GameSearch,
	}
	MenuLayer currentMenuLayer = MenuLayer.MainMenu;
	
	/// <summary>
	/// The available game list scroll position.
	/// </summary>
	Vector2 scrollPos = Vector2.zero;
	
	/// <summary>
	/// True if we failed to connect to the photon master network
	/// </summary>
	bool connectFailed = false;
	
	public AudioClip mainMenuSong;
	public GUIStyle titleStyle;
	public GUIStyle menuButtonStyle;
	public GUIStyle menuOptionsStyle;
	public string firstLevelName;
	
	public Texture2D instructionTexture;
	
	public Texture2D indieDBIcon;
	public Texture2D twitterIcon;
	
	#region MonoBehavior
	
	void Awake()
	{		
		// Ensure the mouse cursor is visible
		Screen.showCursor = true;
				
        // Connect to the main photon server. This is the only IP and port we ever need to set(!)
        if (PhotonNetwork.connectionState == ConnectionState.Disconnected)
        {
#if UNITY_EDITOR
			PhotonNetwork.logLevel = PhotonLogLevel.Full; // turn on if needed
#endif						
            PhotonNetwork.ConnectUsingSettings(GameDirector.GameVersion);			
        }
		
		if (null != PhotonNetwork.room)
		{
			PhotonNetwork.LeaveRoom();
		}
		
		AudioDirector.MusicVolume = ConfigurationDirector.Audio.MusicVolume;
		AudioDirector.SFXVolume = ConfigurationDirector.Audio.SFXVolume;
		AudioDirector.PlayMusic(mainMenuSong);
	}
	
	void OnGUI()
	{
		GUI.color = Color.black;
		
		// Render the version
		GUI.Label(new Rect(4,Screen.height-24,200,20), "Version " + GameDirector.GameVersion);
		
		GUILayout.BeginArea(new Rect((Screen.width/2)-400, 20, 800, Screen.height));

		// Render the title
		GUILayout.Label("Paper Cowboys", titleStyle, GUILayout.Height(60), GUILayout.Width(800));
		
		// Render the subtitle
		GUILayout.BeginHorizontal();
		GUILayout.FlexibleSpace();
		GUILayout.Label("A somewhat functional game programmed in 48 total hours!");
		GUILayout.FlexibleSpace();
		GUILayout.EndHorizontal();
		GUILayout.Space(30);
		
		// If we're not connected to the master photon network, then render nothing except our
		// status to it because nothing else matters.
		if (!PhotonNetwork.connected)
        {
			GUILayout.EndArea();
			GUILayout.Space(250);
			
            if (PhotonNetwork.connectionState == ConnectionState.Connecting)
            {
                GUILayout.Label("Connecting " + PhotonNetwork.networkingPeer.ServerAddress);
                GUILayout.Label(Time.time.ToString());
            }
            else
            {
                GUILayout.Label("Not connected.");
                if (GUILayout.Button("Reconnect", GUILayout.Width(100)))
                {
					currentMenuLayer = MenuLayer.MainMenu;
                    this.connectFailed = false;
                    PhotonNetwork.ConnectUsingSettings(GameDirector.GameVersion);					
				}
            }

            if (this.connectFailed)
            {
                GUILayout.Label("Connection failed. Check setup and use Setup Wizard to fix configuration.");
                GUILayout.Label(string.Format("Server: {0}:{1}", new object[] {PhotonNetwork.PhotonServerSettings.ServerAddress, PhotonNetwork.PhotonServerSettings.ServerPort}));
                GUILayout.Label("AppId: " + PhotonNetwork.PhotonServerSettings.AppID);
                
                if (GUILayout.Button("Try Again", GUILayout.Width(100)))
                {
					currentMenuLayer = MenuLayer.MainMenu;
                    this.connectFailed = false;
                    PhotonNetwork.ConnectUsingSettings(GameDirector.GameVersion);
                }
            }
        }
		// If we are connected to the Photon master network, render the active menu
		else
		{
			switch (currentMenuLayer)
			{
			case MenuLayer.MainMenu:
				RenderMainMenu();
				break;
			case MenuLayer.OptionsMenu:
				RenderOptionsMenu();
				break;
			case MenuLayer.HostGameMenu:
				RenderHostGameMenu();
				break;
			case MenuLayer.GameSearch:
				RenderGameSearchMenu();
				break;
			}
		}
		
		RenderSocialLinks();
	}
	
	void Update()
	{
		// Back up a menu level if the user hits escape
		if (MenuLayer.MainMenu != currentMenuLayer && Input.GetKeyDown(KeyCode.Escape))
		{
			currentMenuLayer = MenuLayer.MainMenu;
		}
	}
	
	#endregion
	
	/// <summary>
	/// Renders the main menu.
	/// </summary>
	void RenderMainMenu()
	{
		if (GUILayout.Button("Join Online Game", menuButtonStyle, GUILayout.Height(60), GUILayout.Width(800)))
		{
			currentMenuLayer = MenuLayer.GameSearch;
		}
		GUILayout.Space(10);
			
		if (GUILayout.Button("Start New Online Game", menuButtonStyle, GUILayout.Height(60), GUILayout.Width(800)))
		{
			currentMenuLayer = MenuLayer.HostGameMenu;
		}
		GUILayout.Space(10);
		
		if (GUILayout.Button("Options", menuButtonStyle, GUILayout.Height(60), GUILayout.Width(800)))
		{
			currentMenuLayer = MenuLayer.OptionsMenu;
		}
		GUILayout.Space(10);		
		
		GUILayout.EndArea();
	}
	
	/// <summary>
	/// Renders the options menu.
	/// </summary>
	void RenderOptionsMenu()
	{
		// Player name
		GUILayout.BeginHorizontal();
		GUILayout.Label("Name", menuOptionsStyle, GUILayout.Width(100));
		PlayerDirector.Name = GUILayout.TextField(PlayerDirector.Name, GUILayout.Width(300));
		GUILayout.EndHorizontal();
		
		GUILayout.Space(20);
		
		// Player color
		GUILayout.BeginHorizontal();
		GUI.color = PlayerDirector.PlayerColor;
		GUILayout.Label("Color", GUILayout.Width(100));
		PlayerDirector.Hue = GUILayout.HorizontalSlider(PlayerDirector.Hue, 0, 1, GUILayout.Width(300));
		GUILayout.EndHorizontal();		
		GUI.color = new Color(0,0,0,1);
		
		GUILayout.Space(20);
		
		// SFX volume
		GUILayout.BeginHorizontal();
		GUILayout.Label("SFX Volume", menuOptionsStyle, GUILayout.Width(100));
		AudioDirector.SFXVolume = ConfigurationDirector.Audio.SFXVolume = GUILayout.HorizontalSlider(ConfigurationDirector.Audio.SFXVolume, 0, 1, GUILayout.Width(300));
		GUILayout.Label( "   " + Mathf.CeilToInt(ConfigurationDirector.Audio.SFXVolume * 100.0f).ToString() + "%" );
		GUILayout.EndHorizontal();
		
		GUILayout.Space(20);
		
		// Music volume
		GUILayout.BeginHorizontal();
		GUILayout.Label("Music Volume", menuOptionsStyle, GUILayout.Width(100));
		ConfigurationDirector.Audio.MusicVolume = GUILayout.HorizontalSlider(ConfigurationDirector.Audio.MusicVolume, 0, 1, GUILayout.Width(300));
		AudioDirector.MusicVolume = ConfigurationDirector.Audio.MusicVolume;
		GUILayout.Label( "   " + Mathf.CeilToInt(ConfigurationDirector.Audio.MusicVolume * 100.0f).ToString() + "%" );
		GUILayout.EndHorizontal();
		
		GUILayout.Space(50);
		
		if (GUILayout.Button("Back", menuButtonStyle, GUILayout.Height(60), GUILayout.Width(800)))
		{
			currentMenuLayer = MenuLayer.MainMenu;
		}
		GUILayout.Space(10);
		
		GUILayout.EndArea();

		// Render the instructions 
		float size = 300;
		float h = size * (float)instructionTexture.height / (float)instructionTexture.width;
		Rect instructionsRect = new Rect(Screen.width - size,Screen.height - h,size,h);
		GUI.color = Color.white;
		GUI.DrawTexture(instructionsRect, instructionTexture);					
	}
	
	/// <summary>
	/// Renders the 'host game' menu.
	/// </summary>
	void RenderHostGameMenu()
	{
		// Player count
		GUILayout.BeginHorizontal();
		GUILayout.Label("Players", menuOptionsStyle, GUILayout.Width(300));
		ConfigurationDirector.Host.MaxPlayers = (int)GUILayout.HorizontalSlider(ConfigurationDirector.Host.MaxPlayers, 1, 8, GUILayout.Width(300));
		GUILayout.Label( "   " + ConfigurationDirector.Host.MaxPlayers.ToString() );
		GUILayout.EndHorizontal();		
		
		// Room name
		GUILayout.Space(10);
		GUI.color = Color.white;
		GUILayout.BeginHorizontal();
		GUILayout.Label("Game Name", menuOptionsStyle, GUILayout.Width(300));
		ConfigurationDirector.Host.RoomName = GUILayout.TextField(ConfigurationDirector.Host.RoomName, GUILayout.Width(300));
		GUILayout.EndHorizontal();
		GUI.color = Color.black;
			
		// Buttons
		GUILayout.Space(30);
		GUILayout.BeginHorizontal();
		if (GUILayout.Button("Back", menuButtonStyle, GUILayout.Height(60)))
		{
			currentMenuLayer = MenuLayer.MainMenu;
		}
		
		if (GUILayout.Button("Start!", menuButtonStyle, GUILayout.Height(60)))
		{
			PlayerDirector.SetPhotonCustomPlayerProperties();
			NetworkLevelLoader.EnsureGroup();
			NetworkLevelLoader.ResetLevelPrefix();
			PhotonNetwork.CreateRoom(
				string.IsNullOrEmpty(ConfigurationDirector.Host.RoomName) ? PlayerDirector.Name : ConfigurationDirector.Host.RoomName
				,true
				,true
				,ConfigurationDirector.Host.MaxPlayers);
		}
		GUILayout.EndHorizontal();
		
		GUILayout.EndArea();
	}
	
	/// <summary>
	/// Renders the game search menu.
	/// </summary>
	void RenderGameSearchMenu()
	{
		if (GUILayout.Button("Back", menuButtonStyle, GUILayout.Height(60), GUILayout.Width(800)))
		{
			currentMenuLayer = MenuLayer.MainMenu;
		}
		GUILayout.Space(10);
		
		GUILayout.EndArea();
		GUILayout.Space(175);
		
		GUI.color = Color.black;
        if (PhotonNetwork.GetRoomList().Length == 0)
        {
            GUILayout.Label("Currently no games are available.");
            GUILayout.Label("Rooms will be listed here, when they become available.");
        }
        else
        {
            GUILayout.Label(PhotonNetwork.GetRoomList() + " currently available. Join either:");
			
			if (GUILayout.Button("Join Random Game"))
			{
				GameDirector.IsOnline = true;
				PlayerDirector.SetPhotonCustomPlayerProperties();
				NetworkLevelLoader.EnsureGroup();
				NetworkLevelLoader.ResetLevelPrefix();				
                PhotonNetwork.JoinRandomRoom();
			}
			GUILayout.Space(20);

            // Room listing: simply call GetRoomList: no need to fetch/poll whatever!
            this.scrollPos = GUILayout.BeginScrollView(this.scrollPos);
            foreach (RoomInfo roomInfo in PhotonNetwork.GetRoomList())
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(roomInfo.name + " " + roomInfo.playerCount + "/" + roomInfo.maxPlayers);
                if (GUILayout.Button("Join"))
                {
					GameDirector.IsOnline = true;
					PlayerDirector.SetPhotonCustomPlayerProperties();
					NetworkLevelLoader.EnsureGroup();
					NetworkLevelLoader.ResetLevelPrefix();
                    PhotonNetwork.JoinRoom(roomInfo.name);
                }

                GUILayout.EndHorizontal();
            }

            GUILayout.EndScrollView();
        }		
	}
	
	/// <summary>
	/// Renders the social links on the lower right-hand corner of the screen
	/// </summary>
	void RenderSocialLinks()
	{
		float iconSize = 36;
		float x = Screen.width - iconSize - 4;
		float y = Screen.height - 110;		
		GUI.color = Color.white;
		
		GUI.Label(new Rect(x-90,y-2,200,iconSize), "Official Home", menuOptionsStyle);
		if (GUI.Button(new Rect(x-5,y-5,iconSize+10,iconSize+10), indieDBIcon, menuOptionsStyle))
		{
			Application.OpenURL("http://www.indiedb.com/games/paper-cowboys");
		}
		y += iconSize + 15;
		
		GUI.Label(new Rect(x-85,y,200,iconSize), "Twitter Feed", menuOptionsStyle);
		if (GUI.Button(new Rect(x,y,iconSize,iconSize), twitterIcon, menuOptionsStyle))
		{
			Application.OpenURL("http://www.twitter.com/gamieon");
		}
		y += iconSize + 15;	
	}
	
	
	#region PhotonNetworkingMessage
	
	private void OnPhotonCreateRoomFailed()
	{
		Debug.Log("OnPhotonCreateRoomFailed");
	}
	
   // We have two options here: we either joined(by title, list or random) or created a room.
    private void OnJoinedRoom()
    {
		// No need to do this; the RPCLoadLevel buffered call from the master client
		// should take care of room synchronization
        //this.StartCoroutine(this.MoveToGameScene());
    }

    private void OnCreatedRoom()
    {
        Debug.Log("OnCreatedRoom");
        this.StartCoroutine(this.MoveToGameScene());
    }

	private void OnConnectedToPhoton()
    {
        Debug.Log("Connected to Photon.");
		GameDirector.IsOnline = true;
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

	/// <summary>
	/// Called for a player creating a room to go to start the game once the room
	/// has been created.
	/// </summary>
	/// <returns>
	/// The to game scene.
	/// </returns>
    private IEnumerator MoveToGameScene()
    {
		// Wait until our room is created
        while (PhotonNetwork.room == null)
        {
            yield return 0;
        }

		// Ensure that no music is playing
		AudioDirector.StopMusic();
		
		// Go to level 1 using the network level loader component
		NetworkLevelLoader.LoadLevel("L1");
    }
	
	#endregion
}
