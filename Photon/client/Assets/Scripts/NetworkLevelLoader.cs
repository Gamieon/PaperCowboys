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
[RequireComponent(typeof(PhotonView))]
public class NetworkLevelLoader : Photon.MonoBehaviour 
{
	/// <summary>
	/// Called by the master client to load a level by name
	/// </summary>
	/// <param name='sceneName'>
	/// Scene name.
	/// </param>
	public static void LoadLevel(string sceneName)
	{
		((NetworkLevelLoader)FindObjectOfType(typeof(NetworkLevelLoader))).LoadLevelInternal(sceneName);
	}
	
	/// <summary>
	/// Called by any client before they join a game to ensure this component is operating on channel 1.
	/// </summary>
	public static void EnsureGroup()
	{
		((NetworkLevelLoader)FindObjectOfType(typeof(NetworkLevelLoader))).EnsureGroupInternal();
	}
	
	/// <summary>
	/// Called by any client before they join a game to reset the last level prefix. The last level prefix
	/// is a number that increments every time a level is loaded since the time a game is joined or created.
	/// </summary>
	public static void ResetLevelPrefix()
	{
		((NetworkLevelLoader)FindObjectOfType(typeof(NetworkLevelLoader))).lastLevelPrefix = 0;
		PhotonNetwork.SetLevelPrefix(0);
	}
	
	/// <summary>
	/// The last level prefix is a number that increments every time a level is loaded since the time a game
	/// is joined or created.
	/// </summary>
	short lastLevelPrefix = 0;
	
	#region MonoBehavior
	
	void Awake()
	{
		Debug.Log("in NetworkLevelLoader: Awake");
		// Network level loading is done in a separate channel.
	    DontDestroyOnLoad(this);
	}
	
	void OnLevelWasLoaded(int level)
	{
		Debug.Log("Level load complete");
		
		if (PhotonNetwork.connected)
		{
			// Allow receiving data again
			PhotonNetwork.isMessageQueueRunning = true;
			// Now the level has been loaded and we can start sending out data to clients
			PhotonNetwork.SetSendingEnabled(0, true);		
		}
		
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
	/// Ensures we do everything on channel 1
	/// </summary>
	void EnsureGroupInternal()
	{
	    photonView.group = 1;
		PhotonNetwork.SetSendingEnabled(1, true);
		PhotonNetwork.SetReceivingEnabled(1, true);			
	}
	
	/// <summary>
	/// Called by the static LoadLevel function to load a level (this is state-based)
	/// </summary>
	/// <param name='sceneName'>
	/// Scene name.
	/// </param>
	void LoadLevelInternal(string sceneName)
	{
		Debug.Log("in LoadLevelInternal: " + sceneName + " current lastLevelPrefix is " + lastLevelPrefix);		
		EnsureGroupInternal();
		PhotonNetwork.RemoveRPCsInGroup(0);
		PhotonNetwork.RemoveRPCsInGroup(1);
		Debug.Log("sending RPCLoadLevel");
		photonView.RPC("RPCLoadLevel", PhotonTargets.AllBuffered, sceneName, (short)(lastLevelPrefix + 1));
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
	public void RPCLoadLevel(string sceneName, short levelPrefix)
	{
		Debug.Log("in RPCLoadLevel: " + sceneName + " prefix: " + levelPrefix);
		lastLevelPrefix = levelPrefix;
		
		// There is no reason to send any more data over the network on the default channel,
		// because we are about to load the level, thus all those objects will get deleted anyway
		PhotonNetwork.SetSendingEnabled(0, false);
		
		// We need to stop receiving because first the level must be loaded first.
		// Once the level is loaded, rpc's and other state update attached to objects in the level are allowed to fire
		PhotonNetwork.isMessageQueueRunning = false;
		
		// All network views loaded from a level will get a prefix into their NetworkViewID.
		// This will prevent old updates from clients leaking into a newly created scene.
		PhotonNetwork.SetLevelPrefix(levelPrefix);
		
		// Now load the level
		Debug.Log("Loading level " + sceneName);
		PhotonNetwork.LoadLevel(sceneName);
	}
	
	#endregion
}
