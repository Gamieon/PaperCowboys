using UnityEngine;
using System.Collections;

/// <summary>
/// This component manages level transitions; including ones from the main menu into the game.
/// This component must persist throughout the lifetime of the game to function properly and exist
/// in the first scene opened by the application.
/// 
/// During the game, there are two active communication channels:
/// 	Channel 0: Game traffic (player synchronization, player RPC's, chat, etc.)
/// 	Channel 1: Level loading messages  (TODO: Chat should really be in this channel so players can talk during level transitions)
/// 
/// When the master client begins the game, a buffered RPC named "RPCLoadLevel" is posted on channel 1 with
/// the current level ID. Anyone who enters the game will immediately get that message and join the proper scene.
/// When the master client advances the level, the existing buffered "RPCLoadLevel" RPC gets destoyed, and then 
/// replaced with a new buffered "RPCLoadLevel" RPC with the new level ID. This way, a player can join at any time.
/// 
/// To avoid network "spillage" of messages from level 1 going to level 2, channel 0's message queue is turned off 
/// for a client when they begin to make the transition, and then turned back on after the level is fully loaded.
/// They will then get any buffered messages on channel 0 that they missed.
/// 
/// This source was largely copied from http://docs.unity3d.com/Documentation/Components/net-NetworkLevelLoad.html and
/// adjusted properly.
/// 
/// </summary>
[RequireComponent(typeof(uLinkNetworkView))]
public class NetworkLevelLoader : uLink.MonoBehaviour 
{
	public static NetworkLevelLoader Instance
	{
		get {
			return _instance;
		}
	}
	static NetworkLevelLoader _instance;
	
	/// <summary>
	/// Called by the master client to load a level by name
	/// </summary>
	/// <param name='sceneName'>
	/// Scene name.
	/// </param>
	public static void LoadLevel(string sceneName)
	{
		Instance.LoadLevelInternal(sceneName);
	}
	
	/// <summary>
	/// Called by any client before they join a game to reset the last level prefix. The last level prefix
	/// is a number that increments every time a level is loaded since the time a game is joined or created.
	/// </summary>
	public static void ResetLevelPrefix()
	{
		Instance.lastLevelPrefix = 0;
		//uLink.Network.SetLevelPrefix(0); // DEPRECATED
	}
	
	/// <summary>
	/// Gets the level prefix.
	/// </summary>
	/// <value>
	/// The level prefix.
	/// </value>
	public static int LevelPrefix
	{
		get { return Instance.lastLevelPrefix; }
	}
		
	/// <summary>
	/// Gets my network view.
	/// </summary>
	/// <value>
	/// My network view.
	/// </value>
	uLink.NetworkView myNetworkView
	{
		get { return uLink.NetworkView.Get(this); }  
	}
	
	/// <summary>
	/// The last level prefix is a number that increments every time a level is loaded since the time a game
	/// is joined or created.
	/// </summary>
	int lastLevelPrefix = 0;
	
	#region MonoBehavior
	
	void Awake()
	{
		// Assign our instance
		_instance = this;
		
		// Network level loading is done in a separate channel
	    myNetworkView.group = 1;
		//uLink.Network.SetSendingEnabled(1, true); // DEPRECATED
		
		// Keep persistent
	    DontDestroyOnLoad(this);
	}
	
	void OnLevelWasLoaded(int level)
	{
		Debug.Log("Level load complete");
		
		// Allow receiving data again
		uLink.Network.isMessageQueueRunning = true;
		// Now the level has been loaded and we can start sending out data to clients
		//uLink.Network.SetSendingEnabled(0, true); // DEPRECATED
		
		/*
		// Let all the game objects know
		GameObject[] gameObjects = (GameObject[])FindObjectsOfType(GameObject);
		foreach (GameObject go in gameObjects)
		{
			go.SendMessage("OnNetworkLoadedLevel", SendMessageOptions.DontRequireReceiver);	
		}
		*/
	}
	
	#endregion
		
	/// <summary>
	/// Called by the static LoadLevel function to load a level (this is state-based)
	/// </summary>
	/// <param name='sceneName'>
	/// Scene name.
	/// </param>
	void LoadLevelInternal(string sceneName)
	{
		Debug.Log("in LoadLevelInternal: " + sceneName + " current lastLevelPrefix is " + lastLevelPrefix);		
		uLink.Network.RemoveAllRPCs();
		uLink.Network.RemoveAllInstantiates();
		Debug.Log("sending RPCLoadLevel");
		myNetworkView.RPC("RPCLoadLevel", uLink.RPCMode.AllBuffered, sceneName, lastLevelPrefix + 1);
	}
	
	#region RPCs
	
	/// <summary>
	/// This is a buffered RPC call sent from the master client to all clients on channel 1 to inform them
	/// of a level change. There should be no buffered messages in channel 1 when this is called.
	/// </summary>
	/// <param name='sceneName'>
	/// Scene name.
	/// </param>
	/// <param name='levelPrefix'>
	/// Level prefix.
	/// </param>
	[RPC]
	public void RPCLoadLevel(string sceneName, int levelPrefix)
	{
		Debug.Log("in RPCLoadLevel: " + sceneName + " prefix: " + levelPrefix);
		
		// Ignore if we already loaded the level
		if (levelPrefix == lastLevelPrefix)
		{
			return;
		}
		
		lastLevelPrefix = levelPrefix;
		
		// There is no reason to send any more data over the network on the default channel,
		// because we are about to load the level, thus all those objects will get deleted anyway
		//uLink.Network.SetSendingEnabled(0, false); // DEPRECATED
		
		// We need to stop receiving because first the level must be loaded first.
		// Once the level is loaded, rpc's and other state update attached to objects in the level are allowed to fire
		uLink.Network.isMessageQueueRunning = false;
		
		// All network views loaded from a level will get a prefix into their NetworkViewID.
		// This will prevent old updates from clients leaking into a newly created scene.
		//uLink.Network.SetLevelPrefix(levelPrefix); // DEPRECATED
		
		// Now load the level
		Debug.Log("Loading level " + sceneName);
		Application.LoadLevel(sceneName);
	}
	
	#endregion
}
