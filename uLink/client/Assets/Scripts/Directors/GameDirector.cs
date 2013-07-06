using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The one and only game director. This is the nerve center for handling the in-game menu,
/// polling the "active" component for inputs, chat messages, players coming and leaving,
/// the screen HUD, level advancement, player spawning and leaving the game.
/// 
/// This component is destroyed when the scene changes. Every level should have its own
/// GameManager object with a GameDirector component.
/// </summary>
[RequireComponent(typeof(uLinkNetworkView))]
public class GameDirector : uLink.MonoBehaviour, IInputPoller
{	
	/// <summary>
	/// Physics layers (I felt it easier to deal with them when properly enumerated)
	/// </summary>
	public enum GameLayers
	{
		Default = 0,
		TransparentFX = 1,
		IgnoreRaycast = 2,
		Water = 4,
		
		/// <summary>
		/// Player character layer
		/// </summary>
		PlayerCharacter = 8,
		
		/// <summary>
		/// Enemy layer
		/// </summary>
		Enemy = 9,
		
		/// <summary>
		/// Special colliders that players can jump onto or drop from
		/// </summary>
		Ledge = 10,
		
		/// <summary>
		/// Player drop
		/// </summary>
		PlayerDrop = 11,
		
		/// <summary>
		/// Assign this to a collider that you want player drops to fall through
		/// such as train cars.
		/// </summary>
		PlayerDropIgnore = 12,
	}
	
	/// <summary>
	/// What the game director should show when it has focus
	/// </summary>
	public enum GameDirectorFocusMode
	{
		Menu,
		ChatBox,
	}
	
	/// <summary>
	/// The game version.
	/// </summary>
	public const string GameVersion = "1.1ulink";
	
	/// <summary>
	/// Foreground
	/// </summary>
	public const float ZPositions_Foreground = 0;
	
	/// <summary>
	/// Constant respawn time in seconds
	/// </summary>
	public const float RespawnTime = 3;

	/// <summary>
	/// The width of the weapon boxes at the top of the screen
	/// </summary>
	public const int guiWeaponBoxWidth = 96;

	/// <summary>
	/// The height of the weapon boxes at the top of the screen
	/// </summary>
	public const int guiWeaponBoxHeight = 48;
	
	/// <summary>
	/// Sets a value indicating whether the game is going to be online. This must be set by the main menu,
	/// and it defaults to false meaning we're offline by default.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is online; otherwise, <c>false</c>.
	/// </value>
	public static bool IsOnline = false;
	
	/// <summary>
	/// Gets the game director instance.
	/// </summary>
	/// <value>
	/// The game director instance.
	/// </value>
	public static GameDirector Instance
	{
		get {
			object gameDirector = Object.FindObjectOfType(typeof(GameDirector));
			return (GameDirector)gameDirector;
		}
	}
	
	/// <summary>
	/// Gets a value indicating whether the player is playing alone.
	/// </summary>
	/// <value>
	/// <c>true</c> if the player is playing alone; otherwise, <c>false</c>.
	/// </value>
	public static bool IsPlayingSolo {
		get {
			return (!IsOnline || NetworkDirector.Players.Count > 1);
		}
	}
	
	/// <summary>
	/// Called on the master client to warps to a scene.
	/// </summary>
	/// <param name='sceneName'>
	/// Scene name.
	/// </param>
	public static void WarpToScene(string sceneName)
	{
		AudioDirector.StopMusic();		
		NetworkLevelLoader.LoadLevel(sceneName);
	}
	
	/// <summary>
	/// Called on the master client to advance the current level
	/// </summary>
	/// <param name='sceneName'>
	/// Scene name.
	/// </param>
	public static void AdvanceCurrentLevel()
	{	
		if ("L1" == Application.loadedLevelName)
		{
			WarpToScene("L2");
		}
		else if ("L2" == Application.loadedLevelName)
		{
			WarpToScene("L3");
		}
		else if ("L3" == Application.loadedLevelName)
		{
			WarpToScene("L1");
		}		
	}
	
	/// <summary>
	/// The minimum camera x; the camera X position cannot go before this value.
	/// </summary>
	public float MinimumCameraX { get { return startingPoint.position.x; } }
	
	/// <summary>
	/// The maximum camera x; the camera X position cannot go past this value.
	/// </summary>
	public float MaximumCameraX { get { return endingPoint.position.x; } }
	
	/// <summary>
	/// The maximum camera y; the camera Y position cannot go past this value.
	/// </summary>
	public float MaximumCameraY { get { return maxCameraYPoint.position.y; } }
		
	/// <summary>
	/// The GUI style for respawning
	/// </summary>
	public GUIStyle respawnGUIStyle;
	
	/// <summary>
	/// The weapon hotkey GUI style.
	/// </summary>
	public GUIStyle weaponHotkeyGUIStyle;
	
	/// <summary>
	/// The weapon ammo GUI style.
	/// </summary>
	public GUIStyle weaponAmmoGUIStyle;
	
	/// <summary>
	/// The title style.
	/// </summary>
	public GUIStyle titleStyle;
	
	/// <summary>
	/// The menu button style.
	/// </summary>
	public GUIStyle menuButtonStyle;
	
	/// <summary>
	/// The menu options style.
	/// </summary>
	public GUIStyle menuOptionsStyle;
	
	/// <summary>
	/// The crosshair texture.
	/// </summary>
	public Texture2D crosshairTexture;
	
	/// <summary>
	/// A box on top of the UI to represent the status of a weapon
	/// </summary>
	public Texture2D weaponBoxTexture;
	
	/// <summary>
	/// The solid white texture.
	/// </summary>
	public Texture2D solidWhiteTexture;
	
	/// <summary>
	/// The skull and crossbones texture.
	/// </summary>
	public Texture2D skullCrossbonesTexture;
	
	/// <summary>
	/// The weapon box icon textures.
	/// </summary>
	public Texture2D[] weaponBoxIconTextures;
	
	/// <summary>
	/// The starting point.
	/// </summary>
	public Transform startingPoint;
	
	/// <summary>
	/// The ending point.
	/// </summary>
	public Transform endingPoint;
	
	public Transform maxCameraYPoint;
	
	/// <summary>
	/// The player character prefab.
	/// </summary>
	public string PlayerCharacterPrefab;

	/// <summary>
	/// The level song.
	/// </summary>
	public AudioClip levelSong;
	
	/// <summary>
	/// True if the game is over
	/// </summary>
	public bool IsGameOver;
	
	/// <summary>
	/// The component where all inputs are directed to. If this is not null,
	/// it means another component is responding to user inputs and we should
	/// ignore them.
	/// 
	/// The presence of this member solves the problem of the game being confused
	/// over whether a user is pressing a button because a menu is active or because
	/// they want to fire at the enemy.
	/// </summary>
	IInputPoller InputFocus 
	{
		get { return inputFocus; }
		set { 
			if (null != inputFocus)	
			{
				inputFocus.EndPolling();
				inputFocus = null;
			}
			if (null != value && value.BeginPolling())
			{
				inputFocus = value;
			}
		}
	}
	IInputPoller inputFocus = null;	
	
	/// <summary>
	/// What the game director should show when it has focus
	/// </summary>
	GameDirectorFocusMode focusMode;
	
	/// <summary>
	/// Gets a value indicating whether another component responding to inputs.
	/// </summary>
	/// <value>
	/// <c>true</c> if another component responding to inputs; otherwise, <c>false</c>.
	/// </value>
	bool IsOtherComponentRespondingToInputs { get { return (null != InputFocus); } }
	
	/// <summary>
	/// Gets a value indicating whether this instance is the first level.
	/// </summary>
	/// <value>
	/// <c>true</c> if this instance is the first level; otherwise, <c>false</c>.
	/// </value>
	bool IsFirstLevel { get { return "L1" == Application.loadedLevelName; } }
	
	/// <summary>
	/// The game instructions director.
	/// </summary>
	GameInstructionsDirector gameInstructionsDirector;
		
	/// <summary>
	/// The self character.
	/// </summary>
	PlayerCharacter selfCharacter;
	
	/// <summary>
	/// The time until the player respawns.
	/// </summary>
	float? timeUntilRespawn = null;
			
	/// <summary>
	/// The chat messages.
	/// </summary>
	List<ChatMessage> chatMessages;
	
	/// <summary>
	/// The chat message that the player is currently typing
	/// </summary>
	string typingChatMessage = "";
	
	/// <summary>
	/// True if we need to focus the chat box control
	/// </summary>
	bool needFocusControl;

	/// <summary>
	/// Gets my network view.
	/// </summary>
	/// <value>
	/// My network view.
	/// </value>
	public uLink.NetworkView myNetworkView
	{
		get { return uLink.NetworkView.Get(this); } 
	}
	
	/// <summary>
	/// Called by a class inherited from PlayerCharacter when our character is instantiated
	/// </summary>
	/// <param name='playerCharacter'>
	/// Player character.
	/// </param>
	public void SetSelfCharacter(PlayerCharacter playerCharacter)
	{
		// Now retain access to the character
		selfCharacter = playerCharacter.GetComponent<PlayerCharacter>();
		
		// Now make the character input our input focus
		InputFocus = (IInputPoller)selfCharacter;		
	}
	
	/// <summary>
	/// Called from RPCRespawnDeadPlayer to tell a player that they need to spawn themselves.
	/// This is only called on the master client.
	/// </summary>
	/// <param name='player'>
	/// The player to respawn.
	/// </param>
	void RespawnDeadPlayer(uLink.NetworkPlayer player)
	{
		if (!IsGameOver)
		{
			float x = MinimumCameraX;
			float y = 7.0f;
			List<PlayerCharacter> playerCharacters = PlayerCharacter.AllPlayerCharacters;
			for (int i=0; i < playerCharacters.Count; i++)
			{
				if (playerCharacters[i].transform.position.x >= x)
				{
					x = playerCharacters[i].transform.position.x;
					y = playerCharacters[i].transform.position.y + 3.0f;
				}
			}
			// Instantiate the player
			GameObject prefab = (GameObject)Resources.Load(PlayerCharacterPrefab);
			NetworkDirector.Instance.InstantiateClientObject(player, PlayerCharacterPrefab, 
				new Vector3(x,y,ZPositions_Foreground), prefab.transform.localRotation,
				0);
		}
	}
	
	/// <summary>
	/// Called from a PlayerCharacter call to DestroyThisCharacter when our player character
	/// is destroyed. Here we clear all the player drops, reset the munitions and start the respawn timer.
	/// This is only called for the player who actually died.
	/// </summary>
	void OnSelfCharacterDestroyed()
	{
		Debug.Log("in OnSelfCharacterDestroyed");
		
		// Destroy all the drops on the screen (they're all ours)
		PlayerDrop[] playerDrops = (PlayerDrop[])GameObject.FindObjectsOfType(typeof(PlayerDrop));
		foreach (PlayerDrop drop in playerDrops)
		{
			Destroy(drop.gameObject);
		}
		// Reset local ammo stores. Ammo is maintained locally for the moment.
		PlayerDirector.ActiveSession.ResetPlayerMunitions();
		// Reset our self character
		if (InputFocus == selfCharacter) { InputFocus = null; }
		selfCharacter = null;
		// Respawn in 10 seconds
		timeUntilRespawn = (Time.time + RespawnTime);
	}
	
	#region MonoBehaviour
	
	void Awake()
	{
		// Assign default audio director settings
		AudioDirector.SFXVolume = ConfigurationDirector.Audio.SFXVolume;
		AudioDirector.MusicVolume = ConfigurationDirector.Audio.MusicVolume;
		// Stop any music
		AudioDirector.StopMusic();
		// Play the level song
		AudioDirector.PlayMusic(levelSong);
		
		Camera.main.transform.position = new Vector3(
			startingPoint.position.x, 
			Camera.main.transform.position.y,
			Camera.main.transform.position.z);
		gameInstructionsDirector = gameObject.GetComponent<GameInstructionsDirector>();
		
		// Initialize chat messages
		chatMessages = new List<ChatMessage>();
		
		// Ensure the mouse cursor is not visible
#if !UNITY_EDITOR
		Screen.showCursor = false;
#endif	
		
		// If this is the first level, then show the instructions on the top left
		if (IsFirstLevel && !ConfigurationDirector.HasSeenInstructions)
		{
			ConfigurationDirector.HasSeenInstructions = true;
			gameInstructionsDirector.Display();
		}
	}
	
	void Start()
	{	
		if (NetworkDirector.isMasterClient)
		{
			// Tell all active players they can spawn. New players will get an RPC from another
			// function when they come in later.
			GameObject prefab = (GameObject)Resources.Load(PlayerCharacterPrefab);
			float x = startingPoint.transform.position.x;
			float y = startingPoint.transform.position.y;
			foreach (uLink.NetworkPlayer player in NetworkDirector.Players)
			{
				NetworkDirector.Instance.InstantiateClientObject(player, PlayerCharacterPrefab, 
					new Vector3(x,y,ZPositions_Foreground), prefab.transform.localRotation,
					0);
			}
		}
	}
	
	// Update is called once per frame
	void Update() 
	{
		if (null != selfCharacter)
		{
			// Handle player inputs
			PollInputs();	
		}
		
		if (NetworkDirector.isMasterClient)
		{
			// Check for game over
			PollGameOverStatus();
		}
		
		if (timeUntilRespawn.HasValue && Time.time > timeUntilRespawn)
		{
			// Reset the respawn timer
			timeUntilRespawn = null;
			// Ask the master client to let us respawn
			myNetworkView.RPC("RPCRespawnDeadPlayer", NetworkDirector.MasterClient, uLink.Network.player);
		}
	}	

	void OnGUI()
	{
		if (IsGameOver)
		{
			RenderGameOver();
		}
		else if (null == InputFocus 
			|| InputFocus == (IInputPoller)selfCharacter
			|| (InputFocus == (IInputPoller)this && focusMode == GameDirectorFocusMode.ChatBox))
		{	
			// Render the scores
			RenderScores();
			
			// Render the weapons
			RenderWeapons();	
			
			// Nothing has the input focus, so render the respawn timer
			RenderRespawnTimer();			
		}
		
		// Render the input focus GUI
		if (null != InputFocus)
		{
			InputFocus.RenderGUI(new Rect(0,0,Screen.width,Screen.height));
		}
		
		// Render the player's crosshair since there's no mouse cursor
		RenderPlayerCrosshair();
		
		// Render chat messages
		RenderChatMessages();
		
		// Render the game instructions if necessary
		gameInstructionsDirector.RenderInstructions();
	}
	
	#endregion
	
	#region Game Over Management
	
	/// <summary>
	/// Called by the master client to poll the game over status.
	/// </summary>
	public void PollGameOverStatus()
	{
		if (!IsGameOver)
		{
			List<PlayerCharacter> playerCharacters = PlayerCharacter.AllPlayerCharacters;
			// If there are no characters, the game is over.
			if (0 == playerCharacters.Count)
			{
				IsGameOver = true;
				Debug.Log("GAME OVER!");
				myNetworkView.RPC("RPCGameOver", uLink.RPCMode.AllBuffered);
			}
		}
	}
		
	#endregion
	
	#region Menu Management
	
	/// <summary>
	/// Called by the local user from the PlayerCharacter component which handles input polling
	/// to display the game menu.
	/// </summary>
	public void ShowGameMenu()
	{
#if !UNITY_EDITOR
		Screen.showCursor = true;
#endif
		focusMode = GameDirectorFocusMode.Menu;
		InputFocus = (IInputPoller)this;
	}
	
	/// <summary>
	/// Called by this component to hide the game menu.
	/// </summary>
	void HideGameMenu()
	{
#if !UNITY_EDITOR
		Screen.showCursor = false;
#endif						
		InputFocus = (IInputPoller)selfCharacter;
	}
	
	/// <summary>
	/// Called by the local user from the PlayerCharacter component which handles input polling
	/// to display the chat bar to begin typing in a message to all players.
	/// </summary>
	public void ShowChatBar()
	{
		focusMode = GameDirectorFocusMode.ChatBox;
		typingChatMessage = "";
		needFocusControl = true;
		InputFocus = (IInputPoller)this;
	}
	
	/// <summary>
	/// Called in response to a RPCChatMessage message to clear the oldest chat message from
	/// the list ten seconds after a new message was added.
	/// </summary>
	void ClearOldestChatMessage()
	{	
		if (chatMessages.Count > 0)
		{
			chatMessages.RemoveAt(0);
		}
	}
	
	#endregion
	
	#region GUI
	
	/// <summary>
	/// Renders the game menu.
	/// </summary>
	void RenderGameMenu()
	{
		GUI.color = Color.black;
		GUILayout.BeginArea(new Rect((Screen.width/2)-400, 20, 800, Screen.height));
		
		// Title
		GUILayout.Label("Paper Cowboys", titleStyle, GUILayout.Height(60), GUILayout.Width(800));
		GUILayout.Space(30);
		
		// SFX and music volume (I don't want to do drilldowns in game mode because the game isn't paused)
		RenderGameOptions();
		
		// Resume button
		if (GUILayout.Button("Resume Game", menuButtonStyle, GUILayout.Height(60), GUILayout.Width(800)))
		{
			HideGameMenu();
		}
		GUILayout.Space(30);
		
		// Quit button
		if (GUILayout.Button("Quit", menuButtonStyle, GUILayout.Height(60), GUILayout.Width(800)))
		{
			NetworkDirector.Disconnect();
		}
		
		
#if UNITY_EDITOR
		// If the master client is running the Unity editor, we support warping to different levels
		if (NetworkDirector.isMasterClient)
		{
			GUILayout.Space(30);
			GUILayout.BeginHorizontal();
			if (GUILayout.Button("L1", menuButtonStyle, GUILayout.Height(60)))
			{	
				GameDirector.WarpToScene("L1");
			}
			
			GUILayout.Space(30);
			if (GUILayout.Button("L2", menuButtonStyle, GUILayout.Height(60)))
			{	
				GameDirector.WarpToScene("L2");
			}

			GUILayout.Space(30);
			if (GUILayout.Button("L3", menuButtonStyle, GUILayout.Height(60)))
			{	
				GameDirector.WarpToScene("L3");
			}

			GUILayout.EndHorizontal();
		}
#endif
		
		GUILayout.EndArea();		
	}
	
	/// <summary>
	/// Renders the chat box (but not historic chat messages) and handles chat box typing
	/// </summary>
	void RenderChatBox()
	{
		int fontHeight = 20;
		GUI.color = Color.white;
		GUI.SetNextControlName ("MyTextField");
		typingChatMessage = GUI.TextField(new Rect(5, Screen.height - fontHeight, Screen.width - 10, fontHeight), typingChatMessage);
		if (needFocusControl)
		{
			GUI.FocusControl ("MyTextField");
			needFocusControl = false;
		}
		
		if (typingChatMessage.Length > 0 && Event.current.isKey)
		{
			if (Event.current.keyCode == KeyCode.Return || Event.current.keyCode == KeyCode.KeypadEnter)
			{
				// Send the chat message
				Color color = PlayerDirector.PlayerColor;
				myNetworkView.RPC("RPCChatMessage", uLink.RPCMode.All, PlayerDirector.Name + ": " + typingChatMessage, 
					color.r, color.g, color.b);
				// Go back to playing
				InputFocus = (IInputPoller)selfCharacter;
			}
			else if (Event.current.keyCode == KeyCode.Escape)
			{
				// Go back to playing
				InputFocus = (IInputPoller)selfCharacter;
			}
		}		
	}
	
	/// <summary>
	/// Renders the chat message history (no message will be older than ten seconds though)
	/// </summary>
	void RenderChatMessages()
	{
		int fontHeight = 20;		
		int i, y;
		for (i = chatMessages.Count - 1, y = Screen.height - fontHeight * 2; i >= 0; i--, y -= fontHeight)
		{
			GUI.color = Color.white;
			GUI.DrawTexture(new Rect(5,y,Screen.width-10,fontHeight), solidWhiteTexture);
			GUI.color = chatMessages[i].color;
			GUI.Label(new Rect(5,y,Screen.width-10,fontHeight), chatMessages[i].message);
		}
	}
	
	/// <summary>
	/// Renders the game options menu
	/// </summary>
	void RenderGameOptions()
	{
		// SFX volume
		GUILayout.BeginHorizontal();
		GUILayout.Space(180); GUILayout.Label("SFX Volume", menuOptionsStyle, GUILayout.Width(100));
		AudioDirector.SFXVolume = ConfigurationDirector.Audio.SFXVolume = GUILayout.HorizontalSlider(ConfigurationDirector.Audio.SFXVolume, 0, 1, GUILayout.Width(300));
		GUILayout.Label( "   " + Mathf.CeilToInt(ConfigurationDirector.Audio.SFXVolume * 100.0f).ToString() + "%" );
		GUILayout.EndHorizontal();
		
		GUILayout.Space(20);
		
		// Music volume
		GUILayout.BeginHorizontal();
		GUILayout.Space(180); GUILayout.Label("Music Volume", menuOptionsStyle, GUILayout.Width(100));
		ConfigurationDirector.Audio.MusicVolume = GUILayout.HorizontalSlider(ConfigurationDirector.Audio.MusicVolume, 0, 1, GUILayout.Width(300));
		AudioDirector.MusicVolume = ConfigurationDirector.Audio.MusicVolume;
		GUILayout.Label( "   " + Mathf.CeilToInt(ConfigurationDirector.Audio.MusicVolume * 100.0f).ToString() + "%" );
		GUILayout.EndHorizontal();
		
		GUILayout.Space(50);		
	}
	
	/// <summary>
	/// Renders the game over screen
	/// </summary>
	void RenderGameOver()
	{
		GUI.color = Color.black;
		GUILayout.Space(60);
		GUILayout.Label("Game over", titleStyle, GUILayout.Height(60), GUILayout.Width(800));
		GUILayout.Space(150);
		
		// Try again button for the master client only
		if (NetworkDirector.isMasterClient)
		{
			if (GUILayout.Button("Try Again", menuButtonStyle, GUILayout.Height(60), GUILayout.Width(800)))
			{
				GameDirector.WarpToScene("L1"); // This should take everyone with you
			}
		}

		// Quit button
		if (GUILayout.Button("Quit", menuButtonStyle, GUILayout.Height(60), GUILayout.Width(800)))
		{
			NetworkDirector.Disconnect();
		}		
	}
	
	/// <summary>
	/// Renders the player crosshair.
	/// </summary>
	void RenderPlayerCrosshair()
	{
		// TODO: Detect gamepad input and hide this if used
		
		// Render the cursor position with the player's color
		if (null != selfCharacter && !timeUntilRespawn.HasValue)
		{
			if (!selfCharacter.IsDying)
			{
				Vector3 pos = Input.mousePosition;
				GUI.color = PlayerDirector.PlayerColor;
				GUI.DrawTexture(new Rect(pos.x - 12, Screen.height - pos.y - 16, 24, 24), crosshairTexture);
			}
		}
		else
		{
			// Still spawning
		}		
	}
	
	/// <summary>
	/// Renders the player names, scores, and whether they're alive
	/// </summary>
	void RenderScores()
	{
		List<PlayerCharacter> playerCharacters = PlayerCharacter.AllPlayerCharacters;
		GUILayout.BeginArea(new Rect(0,0,300,800));
		
		foreach (uLink.NetworkPlayer player in NetworkDirector.Players)
		{
			bool isAlive = false;
			foreach (PlayerCharacter p in playerCharacters)
			{
				if (p.myNetworkView.owner == player)
				{
					isAlive = true;
					break;
				}
			}
			
			GUILayout.BeginHorizontal();
			GUI.color = Color.white;
			if (isAlive)
			{
				GUILayout.Space(20);
			}
			else
			{
				GUILayout.Label(skullCrossbonesTexture, GUILayout.Width(20));
			}
			
			GUI.color = ColorDirector.H2RGB( (float)NetworkDirector.GetCustomPlayerProperty(player, "Player.Hue") );
			GUILayout.Label(NetworkDirector.GetCustomPlayerProperty(player, "Player.Name") + ": " + (int)NetworkDirector.GetCustomPlayerProperty(player, "Player.ActiveSession.Score") );
			GUILayout.EndHorizontal();
		}
		GUILayout.EndArea();
	}
	
	/// <summary>
	/// Renders the weapon boxes at the top of the screen
	/// </summary>
	void RenderWeapons()
	{
		const int availableWeaponCount = WeaponDirector.PlayerWeaponCount;
		int x = Screen.width / 2 - (guiWeaponBoxWidth * WeaponDirector.PlayerWeaponCount) / 2;
		for (int i=0; i < availableWeaponCount; i++, x += guiWeaponBoxWidth)
		{
			GUI.BeginGroup(new Rect(x,8,guiWeaponBoxWidth,guiWeaponBoxHeight));
			RenderWeaponBox((WeaponDirector.PlayerWeapon)i);
			GUI.EndGroup();
		}
	}
	
	/// <summary>
	/// Renders a single weapon box at the top of the screen
	/// </summary>
	/// <param name='playerWeapon'>
	/// Player weapon.
	/// </param>
	void RenderWeaponBox(WeaponDirector.PlayerWeapon playerWeapon)
	{
		// Render the box
		GUI.color = Color.white;
		GUI.DrawTexture(new Rect(0,0,guiWeaponBoxWidth,guiWeaponBoxHeight), weaponBoxTexture);
		
		// Render the icon 
		GUI.color = PlayerDirector.ActiveSession.HasWeapon(playerWeapon) ? Color.black : new Color(0,0,0,0.3f);
		if (PlayerDirector.ActiveSession.CurrentWeapon == playerWeapon)
		{
			GUI.color = PlayerDirector.PlayerColor;
		}
		GUI.DrawTexture(new Rect(3,3,guiWeaponBoxWidth-6,guiWeaponBoxHeight-6), weaponBoxIconTextures[(int)playerWeapon]);
		
		// Render the hotkey
		GUI.Label(new Rect(2,2,guiWeaponBoxWidth,18),((int)playerWeapon+1).ToString(), weaponHotkeyGUIStyle);
		
		// Render the player ammo
		if (WeaponDirector.PlayerWeapon.Pistol != playerWeapon)
		{
			weaponAmmoGUIStyle.normal.textColor = PlayerDirector.ActiveSession.HasWeapon(playerWeapon) ? Color.black : Color.gray;
			GUI.Label(new Rect(0,0,guiWeaponBoxWidth-4,guiWeaponBoxHeight-1),
				PlayerDirector.ActiveSession.GetAmmo(playerWeapon).ToString(), weaponAmmoGUIStyle);
		}
		else
		{
			// Pistols have infinite ammo
		}
	}
	
	/// <summary>
	/// Renders the respawning timer.
	/// </summary>
	void RenderRespawnTimer()
	{
		if (timeUntilRespawn.HasValue)
		{
			float timeRemaining = timeUntilRespawn.Value - Time.time;
			respawnGUIStyle.normal.textColor = PlayerDirector.PlayerColor;
			GUI.color = PlayerDirector.PlayerColor;
			GUI.Label(new Rect(0,80,Screen.width,10), "Respawning in " + (Mathf.FloorToInt(timeRemaining) + 1), respawnGUIStyle);
		}		
	}
	
	#endregion
	
	#region Input Management
	
	/// <summary>
	/// This is called by Update() every frame to poll user inputs. Its primary purpose
	/// is to coordinate user inputs so multiple input sinks don't try to act on the player
	/// (for example: If you had a scene with five components listening for MouseDown events,
	/// they could all act on them in ways that conflict with each other. We just want one
	/// component (this one) listening for MouseDown events and dispatching them to the proper
	/// component.
	/// </summary>
	void PollInputs()
	{				
		if (!IsOtherComponentRespondingToInputs)
		{
			// If nothing has input focus, there's nothing to do
		}
		else
		{
			// If something has input focus, poll it.
			if (!InputFocus.Poll())
			{
				// A return value of false means to release input focus
				InputFocus = null;
			}
		}		
	}
	
	#endregion	
	
	#region InputPoller
	
	/// <summary>
	/// Called to begin polling this input object
	/// </summary>
	/// <returns>
	/// False if this object cannot be made active
	/// </returns>	
	bool IInputPoller.BeginPolling()
	{
		return true;
	}
	
	/// <summary>
	/// Poll is called once per frame if this object has input focus. This
	/// function transforms the camera based on user inputs
	/// </summary>
	/// <returns>
	/// False if this object is no longer active
	/// </returns>	
	bool IInputPoller.Poll()
	{
		if (Input.GetKeyUp(KeyCode.Escape))
		{
			HideGameMenu();
		}
		return true;
	}
	
	/// <summary>
	/// Called when polling has finished
	/// </summary>	
	void IInputPoller.EndPolling()
	{
	}
	
	/// <summary>
	/// Called to render the GUI
	/// </summary>
	/// <param name='screenArea'>
	/// The available screen area. You should call GUI.BeginGroup before rendering.
	/// </param>
	void IInputPoller.RenderGUI(Rect screenArea)
	{
		if (GameDirectorFocusMode.Menu == focusMode)
		{
			if (!IsGameOver)
			{
				RenderGameMenu();
			}
		}
		else
		{
			RenderChatBox();
		}
	}
	
	#endregion
	
	#region RPCs
	
	/// <summary>
	/// Sent from a dead player client to the master client basically 
	/// asking the master for permission to respawn.
	/// </summary>
	/// <param name='player'>
	/// Player.
	/// </param>
	[RPC]
	public void RPCRespawnDeadPlayer(uLink.NetworkPlayer player)
	{
		RespawnDeadPlayer(player);
	}
	
	/// <summary>
	/// Sent from the master server to this client to change their score
	/// </summary>
	/// <param name='newScore'>
	/// The new score.
	/// </param>
	[RPC]
	public void RPCSetScore(int newScore)
	{
		Hashtable hashTable = new Hashtable();
		hashTable.Add("Player.ActiveSession.Score", newScore);
		NetworkDirector.SetPlayerCustomProperties(hashTable, true);
	}
	
	/// <summary>
	/// Sent from the master server to this client to give them a drop
	/// after an enemy died
	/// </summary>
	/// <param name='itemDropType'>
	/// The type of drop
	/// </param>
	[RPC]
	public void RPCGiveDrop(Vector3 position)
	{
		if (null != selfCharacter)
		{
			// The type of item the player is getting. If all else fails, get ammo.
			ItemDropType itemDropType = ItemDropType.Ammo;
			// The weights we use in calculating what to give the player
			float weaponWeight = 0.33f;
			float ammoWeight = 0.33f;
			float accessoryWeight = 0.33f;
			
			// Decerease weapon weight if the player has weapons
			switch (PlayerDirector.ActiveSession.AvailableWeaponCount)
			{
			case 1:
				weaponWeight = 0.4f;
				ammoWeight = 0;
				break;
			case 2:
				weaponWeight = 0.2f;
				ammoWeight = 0.1f;
				break;
			case 3:
				weaponWeight = 0.1f;
				ammoWeight = 0.2f;
				break;
			case 4:
				weaponWeight = 0;
				ammoWeight = 0.33f;
				break;
			}
			
			// Decrease accessory weight if the player has an accessory
			if (!string.IsNullOrEmpty(selfCharacter.ActiveHatName))
			{
				accessoryWeight -= 0.1f;
			}
			if (!string.IsNullOrEmpty(selfCharacter.ActiveMouthpieceName))
			{
				accessoryWeight -= 0.1f;
			}
			
			// Determine the kind of drop
			float dropType = Random.Range(0, weaponWeight + ammoWeight + accessoryWeight);			
			if (dropType < weaponWeight)
			{
				// Dropping a weapon
				switch (Random.Range(0,5))
				{
				case 0:
				case 1:
					itemDropType = ItemDropType.Winchesta;
					break;
				case 2:
				case 3:
					itemDropType = ItemDropType.Shotgun;
					break;
				case 4:
					itemDropType = ItemDropType.RocketLauncher;
					break;
				}
			}
			else if (dropType - weaponWeight < ammoWeight)
			{
				itemDropType = ItemDropType.Ammo;
			}
			else
			{
				switch (Random.Range(0,4))
				{
				case 0:
					itemDropType = ItemDropType.CowboyHat;
					break;
				case 1:
					itemDropType = ItemDropType.EastwoodHat;
					break;
				case 2:
					itemDropType = ItemDropType.TenGallonHat;
					break;
				case 3:
					itemDropType = ItemDropType.Bandana;
					break;
				}
			}
			
			Debug.Log("Drop being created for " + itemDropType.ToString());
			
			// Create the drop LOCALLY. No other player will see it, and it will have
			// no effect on any other player.itemDropType
			GameObject o = (GameObject)GameObject.Instantiate((GameObject)Resources.Load("PlayerDrop"), position, Quaternion.identity);					
			PlayerDrop playerDrop = o.GetComponent<PlayerDrop>();
			playerDrop.playerCharacter = selfCharacter;
			playerDrop.itemDrop = GameDropDirector.Instance.GetAvailableDrop(itemDropType);
		}
	}
	
	/// <summary>
	/// Sent from any client to all clients to display a chat message
	/// </summary>
	/// <param name='message'>
	/// Message.
	/// </param>
	/// <param name='color'>
	/// Color.
	/// </param>
	[RPC]
	void RPCChatMessage(string message, float r, float g, float b)
	{
		// Add the chat message to our list
		chatMessages.Add(new ChatMessage(message, new Color(r,g,b)));
		// Do an invoke to clear the message
		Invoke("ClearOldestChatMessage", 10);		
	}
	
	/// <summary>
	/// Sent from the master client to all clients to signal the game is at an end.
	/// This is a buffered call, so if any players join, they'll get the message.
	/// </summary>
	[RPC]
	public void RPCGameOver()
	{
		IsGameOver = true;
#if !UNITY_EDITOR
		Screen.showCursor = true;
#endif
	}
	
	/// <summary>
	/// Sent from the master client as a buffered message to all clients to signal the boss has been defeated
	/// </summary>
	[RPC]
	public void RPCBossDefeated()
	{
		if (NetworkDirector.isMasterClient)
		{
			// Stop all respawning
			SpawnPoint[] spawnPoints = (SpawnPoint[])GameObject.FindObjectsOfType(typeof(SpawnPoint));
			foreach (SpawnPoint s in spawnPoints)
			{
				s.respawn = false;
				s.enabled = false;
			}
			
			// Delete all the enemies
			Debug.Log("Deleting all enemies");
			EnemyCharacter[] enemyCharacters = (EnemyCharacter[])GameObject.FindObjectsOfType(typeof(EnemyCharacter));
			foreach (EnemyCharacter c in enemyCharacters)
			{
				// Safety check; though this should always be true!
				if (c.myNetworkView.isMine)
				{
					NetworkDirector.Destroy(c.gameObject);
				}
			}
			
			// We don't have time to do anything fancy; just warp to the next level
			Debug.Log("Advancing level");
			GameDirector.AdvanceCurrentLevel();
		}
	}
	
	#endregion
	
	#region Custom Networking
	
	/// <summary>
	/// Called on the server whenever a new player has successfully connected.
	/// </summary>
	/// <param name='player'>
	/// Player.
	/// </param>
	void uLink_OnPlayerConnected(uLink.NetworkPlayer player)
	{
		// Master clients should notify everyone when a player has connected
		if (NetworkDirector.isMasterClient)
		{
			Color black = Color.black;
			myNetworkView.RPC("RPCChatMessage", uLink.RPCMode.All, "A new player has joined the game.", 
				black.r, black.g, black.b);
			
			// Spawn the player
			RespawnDeadPlayer(player);
		}
		else
		{
			Debug.LogError("OnPlayerConnected called for a client...?");
		}
	}
	
	/// <summary>
	/// Called on the server whenever a player is disconnected from the server.
	/// </summary>
	/// <param name='player'>
	/// Player.
	/// </param>
	void uLink_OnPlayerDisconnected(uLink.NetworkPlayer player)
	{
		// Master clients should notify everyone when a player has disconnected
		if (NetworkDirector.isMasterClient)
		{
			Color black = Color.black;
			myNetworkView.RPC("RPCChatMessage", uLink.RPCMode.All, "A player has left the game.", 
				black.r, black.g, black.b);
		}
		else
		{
			Debug.LogError("OnPlayerDisconnected called for a client...?");
		}
	}
	
	/// <summary>
	/// Called on the client when the connection was lost or you disconnected from the server.
	/// </summary>
	/// <param name='info'>
	/// Info.
	/// </param>
    void uLink_OnDisconnectedFromServer(uLink.NetworkDisconnection info)
	{
        // Back to main menu
        Application.LoadLevel("MainMenu");		
    }
	
	/// <summary>
	/// Called on the server whenever a Disconnect was invoked and has completed.
	/// </summary>
	void uLink_OnServerUninitialized()
	{
		// Back to main menu
		Application.LoadLevel("MainMenu");		
	}	
	
	#endregion
}
