using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The Network director which ensures all players are aware of each other.
/// </summary>
[RequireComponent(typeof(uLinkNetworkView))]
public class NetworkDirector : uLink.MonoBehaviour
{
	/* This object's network view operates on channel 2. This channel has no buffered messages.
	 * 
	 * 
	 * When a player's custom property changes, they send a packet to the server. Then the server will broadcast
	 * it to every client but the one who sent it.
	 * 
	 * 
	 * This is the process for a client joining:
	 * 
	 * Step 1: The client called Network.Connect() and connects to the server.
	 * 
	 * Step 2: The server send one RPCOnNetworkPlayerConnected message to the new client for every existing client
	 * plus the server.
	 *
	 * Step 3: The server calls RPCSetPlayerCustom* for every player to the new client.
	 * 
	 * Step 4: The server calls RPCValidationFinished to the client. The client will then join the scene.
	 * 
	 * Step 5: The client gets RPCValidationFinished and sends the server all its properties (which the server
	 * broadcasts to all players)
	 * 
	 * Step 6: The client calls RPCOnNetworkPlayerConnected to all other players to officially announce its existence.
	 * 
	 */
	
	public static NetworkDirector Instance
	{
		get {
			return _instance;
		}
	}
	static NetworkDirector _instance;

	/// <summary>
	/// The game identifier for the master server host listing
	/// </summary>
	public const string GameTypeName = ""; // TODO: Fill this in with your own unique name!
	
	/// <summary>
	/// The default port.
	/// </summary>
	public const int defaultPort = 19784;	
	
	/// <summary>
	/// The name of the game (applies to server only)
	/// </summary>
	static string hostedGameName;

	/// <summary>
	/// Gets the master client.
	/// </summary>
	/// <value>
	/// The master client.
	/// </value>
	public static uLink.NetworkPlayer MasterClient
	{
		get { return masterClient; }
	}
	/// <summary>
	/// Gets a value indicating whether this <see cref="NetworkDirector"/> is the master client.
	/// </summary>
	/// <value>
	/// <c>true</c> if is master client; otherwise, <c>false</c>.
	/// </value>
	public static bool isMasterClient
	{
		get { return uLink.Network.player == masterClient; }
	}
	static uLink.NetworkPlayer masterClient;
		
	/// <summary>
	/// The list of all players in the game
	/// </summary>
	public static List<uLink.NetworkPlayer> Players { get { return players; } }
	static List<uLink.NetworkPlayer> players = new List<uLink.NetworkPlayer>();
	
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
	
	#region Custom player properties
	
	/// <summary>
	/// Custom properties for all players
	/// </summary>
	static Dictionary<string,Hashtable> playerCustomProperties = new Dictionary<string, Hashtable>();
	
	/// <summary>
	/// Called by a server or a client to set the custom property for a player in the game.
	/// </summary>
	/// <param name='player'>
	/// The player who owns the proerty.
	/// </param>
	/// <param name='key'>
	/// Key.
	/// </param>
	/// <param name='data'>
	/// Data.
	/// </param>
	public static void SetPlayerCustomProperty(uLink.NetworkPlayer player, string key, object data)
	{
		if (!playerCustomProperties.ContainsKey(player.ToString()))
		{
			playerCustomProperties.Add(player.ToString(), new Hashtable());
		}
		Hashtable ht = playerCustomProperties[player.ToString()];
		if (!ht.ContainsKey(key))
		{
			ht.Add(key, data);
		}
		else
		{
			ht[key] = data;
		}
	}	
	
	/// <summary>
	/// Called by a server or a client to get a custom player property.
	/// </summary>
	/// <returns>
	/// The custom player property.
	/// </returns>
	/// <param name='player'>
	/// Player.
	/// </param>
	/// <param name='key'>
	/// Key.
	/// </param>
	public static object GetCustomPlayerProperty(uLink.NetworkPlayer player, string key)
	{
		if (playerCustomProperties.ContainsKey(player.ToString()))
		{
			if (playerCustomProperties[player.ToString()].ContainsKey(key))
			{
				return playerCustomProperties[player.ToString()][key];
			}
		}
		return null;
	}
	
	/// <summary>
	/// Called by a server or a client to set several custom properties for your own player
	/// </summary>
	/// <param name='ht'>
	/// A hashtable of properties to change
	/// </param>
	/// <param name='broadcast'>
	/// True if we should broadcast the properties to all players
	/// </param>
	public static void SetPlayerCustomProperties(Hashtable ht, bool broadcast)
	{
		foreach (DictionaryEntry entry in ht)
		{
			SetPlayerCustomProperty(uLink.Network.player, (string)entry.Key, entry.Value);
		
			if (broadcast)
			{
				// If we're the server, then broadcast it to all other players. We don't have
				// to do it this way since we're only semi-authoritative; but this will make
				// porting to a fully authoritative project easier.
				if (uLink.Network.isServer)
				{
					Instance.BroadcastCustomProperty(uLink.RPCMode.Others, uLink.Network.player, (string)entry.Key, entry.Value);
				}
				// If we're a client, then send it to the server and it will do the broadcasting
				else if (uLink.Network.isClient)
				{
					Instance.BroadcastCustomProperty(uLink.RPCMode.Server, uLink.Network.player, (string)entry.Key, entry.Value);
				}			
			}
		}
	}
	
	/// <summary>
	/// Called by a client or a server to broadcast their custom property to all other players
	/// </summary>
	/// <param name='key'>
	/// Key.
	/// </param>
	/// <param name='data'>
	/// Data.
	/// </param>
	void BroadcastCustomProperty(uLink.RPCMode target, uLink.NetworkPlayer owner, string key, object data)
	{
		Debug.Log("in BroadcastCustomProperty target = " + target + " owner = " + owner.ToString() + " key = " + key);
		if (data.GetType() == typeof(string))
		{
			myNetworkView.RPC("RPCSetPlayerCustomString", target, uLink.Network.player, key, (string)data);
		}
		else if (data.GetType() == typeof(int))
		{
			myNetworkView.RPC("RPCSetPlayerCustomInt", target, uLink.Network.player, key, (int)data);
		}
		else if (data.GetType() == typeof(float))
		{
			myNetworkView.RPC("RPCSetPlayerCustomFloat", target, uLink.Network.player, key, (float)data);
		}
		else
		{
			Debug.LogError("Unsupported player property type:" + data.GetType().ToString());
		}
	}

	#endregion
	
	#region Master Server functions
	
	/// <summary>
	/// Clear the host list which was received by MasterServer.PollHostList.
	/// </summary>
	public static void ClearHostList()
	{
		uLink.MasterServer.ClearHostList();
	}
	
	/// <summary>
	/// Request a host list from the master server.
	/// </summary>
	/// <param name='gameTypeName'>
	/// Game type name.
	/// </param>
	public static void RequestHostList()
	{
		uLink.MasterServer.RequestHostList(GameTypeName);
	}
	
	/// <summary>
	/// Check for the latest host list received by using MasterServer.RequestHostList.
	/// </summary>
	/// <returns>
	/// The host list.
	/// </returns>
	public static uLink.HostData[] PollHostList()
	{
		return uLink.MasterServer.PollHostList();	
	}
	
	#endregion
	
	#region Hosting and connecting

	/// <summary>
	/// Initializes the server.
	/// </summary>
	/// <param name='connections'>
	/// Connections.
	/// </param>
	/// <param name='listenPort'>
	/// Listen port.
	/// </param>
	public static void InitializeServer(string gameName, int connections, int listenPort)
	{
		Debug.Log("in InitializeServer");
		hostedGameName = gameName;
		uLink.Network.InitializeServer(connections, listenPort);
	}
	
	/// <summary>
	/// Connect the specified IP and remotePort.
	/// </summary>
	/// <param name='IP'>
	/// IP address.
	/// </param>
	/// <param name='remotePort'>
	/// Connect to the specified host (ip or domain name) and server port.
	/// </param>
	public static uLink.NetworkConnectionError Connect(string IP, int remotePort)
	{
		Debug.Log("Step 1: Network.Connect");
		return uLink.Network.Connect(IP, remotePort); // (Step 1)
	}
	
	/// <summary>
	/// Disconnects from the server.
	/// </summary>
	public static void Disconnect()
	{
		if (uLink.Network.isServer)
		{
			// Reset our game name
			hostedGameName = null;
			// Unregister from the master server
			uLink.MasterServer.UnregisterHost();
		}
		Debug.Log("Disconnecting");
		uLink.Network.Disconnect();		
	}
	
	#endregion
	
	#region Object instantation and destruction
	
	/// <summary>
	/// Instantiates a game object
	/// </summary>
    /// <param name="prefabName">Name of the prefab to instantiate.</param>
    /// <param name="position">Position Vector3 to apply on instantiation.</param>
    /// <param name="rotation">Rotation Quaternion to apply on instantiation.</param>
    /// <param name="group">The group for this view.</param>
	/// <param name="data">Optional instantiation data.</param>
	/// </param>
	public void Instantiate(string prefabName, Vector3 position, Quaternion rotation, int group, params object[] data)
	{
		// Have uLink instantiate the object
		uLink.Network.Instantiate(prefabName, position, rotation, group, data);
	}
	
	/// <summary>
	/// Instantiates a game object
	/// </summary>
	/// <param name="owner">The owning client</param>
    /// <param name="prefabName">Name of the prefab to instantiate.</param>
    /// <param name="position">Position Vector3 to apply on instantiation.</param>
    /// <param name="rotation">Rotation Quaternion to apply on instantiation.</param>
    /// <param name="group">The group for this view.</param>
	/// <param name="data">Optional instantiation data.</param>
	/// </param>
	public void InstantiateClientObject(uLink.NetworkPlayer owner, string prefabName, Vector3 position, Quaternion rotation, int group, params object[] data)
	{
		// Right now we only support this being called from the server, which is also the master client.
		if (isMasterClient)
		{
			// Have uLink instantiate the object
			Debug.Log(string.Format("in InstantiateClientObject of prefab {0} for owner {1}", prefabName, owner.ToString()));
			uLink.Network.Instantiate(owner, prefabName, position, rotation, group, data);
		}
		else
		{
			Debug.LogError("Called InstantiateClientObject from someone who isn't the master client!");
		}
	}	
	
    /// <summary>
    /// Instantiate a "scene-owned" prefab over the network. The idea is that it would persist even if the master client left the game.
    /// Because we are an authoritative client-server model, however, this function is pretty much the same as Instantiate except it
    /// absolutely must be called by the server. (The Photon networking model had a standalone server that designated players as "masters"
    /// to be an authority on things).
    /// </summary>
    /// <remarks>
    /// Only the master client can Instantiate scene objects.
    /// </remarks>
    /// <param name="prefabName">Name of the prefab to instantiate.</param>
    /// <param name="position">Position Vector3 to apply on instantiation.</param>
    /// <param name="rotation">Rotation Quaternion to apply on instantiation.</param>
    /// <param name="group">The group for this view.</param>
    /// <param name="data">Optional instantiation data.</param>
    public void InstantiateSceneObject(string prefabName, Vector3 position, Quaternion rotation, int group, params object[] data)	
	{
		// Right now we only support this being called from the server, which is also the master client.
		if (isMasterClient)
		{
			// Have uLink instantiate the object
			uLink.Network.Instantiate(prefabName, position, rotation, group, data);
		}
		else
		{
			Debug.LogError("Called InstantiateSceneObject from someone who isn't the master client!");
		}
	}
	
	/// <summary>
	/// Destroy the specified game object.
	/// </summary>
	/// <param name='go'>
	/// The game object
	/// </param>
	public static void Destroy(GameObject go)
	{
		// This call has to be buffered or else new players will see ghosts.
		uLink.NetworkView.Get(go).RPC("RPCDestroy", uLink.RPCMode.AllBuffered);
	}
	
	#endregion	
	
	#region MonoBehavior
	
	void Awake()
	{
		// Use a p2p model
		uLink.Network.isAuthoritativeServer = false;
		// Assign our instance
		_instance = this;
		// Player management is done in a separate channel.
	    myNetworkView.group = 2;
		// Keep persistent
	    DontDestroyOnLoad(this);
	}
	
	#endregion
	
	#region Server-only networking events
	
	/// <summary>
	/// Called on the server whenever a Network.InitializeServer was invoked and has completed.
	/// </summary>
	void uLink_OnServerInitialized()
	{
		Debug.Log("in uLink_OnServerInitialized");
		// Post ourselves in the master server list
		uLink.MasterServer.dedicatedServer = false;
		uLink.MasterServer.RegisterHost(GameTypeName, hostedGameName);
		// Add yourself to the player list
		RPCOnNetworkPlayerConnected(uLink.Network.player);
		// As the server, you are always the "master client"
		RPCOnMasterClientSwitched(uLink.Network.player);
    }
	
	/// <summary>
	/// Called on the server when approving a new player to join the game
	/// </summary>
	/// <param name='approval'>
	/// Approval.
	/// </param>
	void uLink_OnPlayerApproval(uLink.NetworkPlayerApproval approval)
	{
		// Unlike Photon or Unity, it seems that object instantiations are 
		// not blocked during level loads for newly connecting clients. That
		// means the server can spawn themselves in the middle of the initial
		// level load.
		
		// To avoid this, we have to include the level and its prefix in the
		// approval packet.
		approval.Approve(Application.loadedLevelName, NetworkLevelLoader.LevelPrefix);
	}
	
	/// <summary>
	/// Called on the server whenever a new player has successfully connected.
	/// </summary>
	/// <param name='player'>
	/// Player.
	/// </param>
	void uLink_OnPlayerConnected(uLink.NetworkPlayer player)
	{
		Debug.Log("in uLink_OnPlayerConnected");
		// Add the player to all communication groups (this is how uLink does it)
		uLink.Network.AddPlayerToGroup(player,1); // Level changes
		uLink.Network.AddPlayerToGroup(player,2); // NetworkDirector / Player properties		
		// Send a message to the new player that we are the master client
		myNetworkView.RPC("RPCOnMasterClientSwitched", player, uLink.Network.player);
		// Have the new player update their player list with all the players (Step 2)
		Debug.Log("Step 2: Updating new client's player list");
		for (int i=0; i < NetworkDirector.players.Count; i++)
		{
			myNetworkView.RPC("RPCOnNetworkPlayerConnected", player, NetworkDirector.players[i]);
		}
		// Send the new client all the custom properties (Step 3)
		Debug.Log("Step 3: Sending client all custom properties");
		for (int i=0; i < NetworkDirector.players.Count; i++)
		{		
			uLink.NetworkPlayer p = NetworkDirector.players[i];
			if (p != player && playerCustomProperties.ContainsKey(p.ToString()))
			{
				Debug.Log("Sending properties for " + p.ToString());
				Hashtable ht = playerCustomProperties[p.ToString()];
				foreach (DictionaryEntry entry in ht)
				{
					string key = (string)entry.Key;
					object data = entry.Value;
					if (data.GetType() == typeof(string))
					{
						myNetworkView.RPC("RPCSetPlayerCustomString", player, p, key, (string)data);
					}
					else if (data.GetType() == typeof(int))
					{
						myNetworkView.RPC("RPCSetPlayerCustomInt", player, p, key, (int)data);
					}
					else if (data.GetType() == typeof(float))
					{
						myNetworkView.RPC("RPCSetPlayerCustomFloat", player, p, key, (float)data);
					}
					else
					{
						Debug.LogError("Unsupported player property type:" + data.GetType().ToString());
					}
				}
			}
		}
		// Send the validation finished message (Step 4)
		Debug.Log("Step 4: Signaling completion of validation");
		myNetworkView.RPC("RPCValidationFinished", player);
	}
	
	/// <summary>
	/// Called on the server whenever a player is disconnected from the server.
	/// </summary>
	/// <param name='player'>
	/// Player.
	/// </param>
	void uLink_OnPlayerDisconnected(uLink.NetworkPlayer player)
	{
		Debug.Log("in uLink_OnPlayerDisconnected");
		// Remove all the bread crumbs the player left us
		uLink.Network.DestroyPlayerObjects(player);
		uLink.Network.RemoveRPCs(player);
		// Now tell everyone the player disconnected
		myNetworkView.RPC("RPCOnNetworkPlayerDisconnected", uLink.RPCMode.All, player);
	}
	
	/// <summary>
	/// Called on the server whenever a player fails to connect to the server in a timely manner
	/// </summary>
	/// <param name='player'>
	/// Player.
	/// </param>	
	void uLink_OnHandoverTimeout(uLink.NetworkPlayer player)
	{
		Debug.Log("in uLink_OnHandoverTimeout");
		// Remove all the bread crumbs the player left us
		uLink.Network.DestroyPlayerObjects(player);
		uLink.Network.RemoveRPCs(player);
	}	
	
	/// <summary>
	/// Called on the server whenever a Disconnect was invoked and has completed.
	/// </summary>
	public void uLink_OnServerUninitialized()
	{
		Debug.Log("Server unhosted");
		// Clear the custom propertiess
		playerCustomProperties = new Dictionary<string, Hashtable>();
		// Clear the player list
		players = new List<uLink.NetworkPlayer>();		
	}
	
	#endregion
	
	#region Client-only networking events
	
	/// <summary>
	/// Called on the client when a connection attempt fails for some reason.
	/// </summary>
	/// <param name='error'>
	/// Error.
	/// </param>
    void uLink_OnFailedToConnect(uLink.NetworkConnectionError error) 
	{
		Debug.Log("in uLink_OnFailedToConnect");
        Debug.Log("Could not connect to server: " + error);
    }
	
	/// <summary>
	/// Called on the client when you have successfully connected to a server.
	/// </summary>
	void uLink_OnConnectedToServer() 
	{
		Debug.Log("in uLink_OnConnectedToServer");
        Debug.Log("Connected to server");	
		
		// Unlike Photon or Unity, it seems that object instantiations are 
		// not blocked during level loads for newly connecting clients. That
		// means the server can spawn themselves in the middle of the initial
		// level load.
		
		// To avoid this, we have to load the level right now.
		string levelName = uLink.Network.approvalData.ReadString();
		int levelPrefix = uLink.Network.approvalData.ReadInt32();
		NetworkLevelLoader.Instance.RPCLoadLevel(levelName, levelPrefix);
    }
	
	/// <summary>
	/// Called on the client when the connection was lost or you disconnected from the server.
	/// </summary>
	/// <param name='info'>
	/// Info.
	/// </param>
    void uLink_OnDisconnectedFromServer(uLink.NetworkDisconnection info)
	{
		Debug.Log("in uLink_OnDisconnectedFromServer");
        Debug.Log("Disconnected from server: " + info);
		// Clear the custom propertiess
		playerCustomProperties = new Dictionary<string, Hashtable>();
		// Clear the player list
		players = new List<uLink.NetworkPlayer>();
    }
	
	#endregion

	#region Network Director RPCs
	
	/// <summary>
	/// This is an RPC sent from a client to the server that they are officially connected to the server.
	/// The server then rebroadcasts this to everyone else.
	/// </summary>
	/// <param name='player'>
	/// Player.
	/// </param>
	[RPC]
	void RPCOnNetworkPlayerConnected(uLink.NetworkPlayer player)
	{
		// Add this player to our local list (even if it's ourself)
		players.Add(player);
		// Now tell everyone else
		if (uLink.Network.isServer)
		{
			foreach (uLink.NetworkPlayer p in NetworkDirector.Players)
			{
				if (p != player && p != uLink.Network.player)
				{
					myNetworkView.RPC("RPCOnNetworkPlayerConnected", player);
				}
			}				
		}
	}
	
	/// <summary>
	/// This is an RPC sent from the server to everybody that the master client has changed
	/// </summary>
	/// <param name='player'>
	/// The new master client.
	/// </param>
	[RPC]
	void RPCOnMasterClientSwitched(uLink.NetworkPlayer player)
	{
		masterClient = player;
	}
	
	/// <summary>
	/// This is an RPC sent from the server to everybody that a player disconnected from the server
	/// </summary>
	/// <param name='player'>
	/// Player.
	/// </param>
	[RPC]
	void RPCOnNetworkPlayerDisconnected(uLink.NetworkPlayer player)
	{
		// Remove the player's properties
		playerCustomProperties.Remove(player.ToString());
		// Remove the player
		players.Remove(player);
	}
	
	/// <summary>
	/// Sent from the server to this client after they've been sent all setup data to join the game
	/// </summary>
	[RPC]
	void RPCValidationFinished()
	{
		// Send everyone your properties (Step 5)
		Debug.Log("Step 5: Broadcasting our properties");
		if (playerCustomProperties.ContainsKey(uLink.Network.player.ToString()))
		{
			Hashtable ht = playerCustomProperties[uLink.Network.player.ToString()];
			foreach (DictionaryEntry entry in ht)
			{
				BroadcastCustomProperty(uLink.RPCMode.Server, uLink.Network.player, (string)entry.Key, entry.Value);
			}
		}
		
		// Send a message to everyone that you officially joined (Step 6). It's important this is done
		// AFTER everyone has your properties so that everyone can freely iterate the player list and
		// the properties therein.
		// 
		Debug.Log("Step 6: Broadcasting RPCOnNetworkPlayerConnected");
		myNetworkView.RPC("RPCOnNetworkPlayerConnected", uLink.RPCMode.Server, uLink.Network.player);
	}
	
	/// <summary>
	/// This is an RPC sent from a client to the server, or from the server to all clients to
	/// update a custom property in memory.
	/// </summary>
	/// <param name='player'>
	/// Player.
	/// </param>
	/// <param name='key'>
	/// Key.
	/// </param>
	/// <param name='data'>
	/// Data.
	/// </param>
	[RPC]
	void RPCSetPlayerCustomString(uLink.NetworkPlayer player, string key, string data)
	{
		Debug.Log("in RPCSetPlayerCustomString: p=" + player.ToString() + " k=" + key);
		
		// Update the property in local memory
		SetPlayerCustomProperty(player, key, data);
		// If we're the server, this needs to be sent to all the other clients
		if (uLink.Network.isServer)
		{
			foreach (uLink.NetworkPlayer p in NetworkDirector.Players)
			{
				if (p != player && p != uLink.Network.player)
				{
					myNetworkView.RPC("RPCSetPlayerCustomString", p, key, data);
				}
			}				
		}
	}
	
	/// <summary>
	/// This is an RPC sent from a client to the server, or from the server to all clients to
	/// update a custom property in memory.
	/// </summary>
	/// <param name='player'>
	/// Player.
	/// </param>
	/// <param name='key'>
	/// Key.
	/// </param>
	/// <param name='data'>
	/// Data.
	/// </param>	
	[RPC]
	void RPCSetPlayerCustomInt(uLink.NetworkPlayer player, string key, int data)
	{
		// Update the property in local memory
		SetPlayerCustomProperty(player, key, data);
		// If we're the server, this needs to be sent to all the other clients
		if (uLink.Network.isServer)
		{
			foreach (uLink.NetworkPlayer p in NetworkDirector.Players)
			{
				if (p != player && p != uLink.Network.player)
				{
					myNetworkView.RPC("RPCSetPlayerCustomInt", p, key, data);
				}
			}				
		}
	}
	
	/// <summary>
	/// This is an RPC sent from a client to the server, or from the server to all clients to
	/// update a custom property in memory.
	/// </summary>
	/// <param name='player'>
	/// Player.
	/// </param>
	/// <param name='key'>
	/// Key.
	/// </param>
	/// <param name='data'>
	/// Data.
	/// </param>	
	[RPC]
	void RPCSetPlayerCustomFloat(uLink.NetworkPlayer player, string key, float data)
	{
		// Update the property in local memory
		SetPlayerCustomProperty(player, key, data);
		// If we're the server, this needs to be sent to all the other clients
		if (uLink.Network.isServer)
		{
			foreach (uLink.NetworkPlayer p in NetworkDirector.Players)
			{
				if (p != player && p != uLink.Network.player)
				{
					myNetworkView.RPC("RPCSetPlayerCustomFloat", p, key, data);
				}
			}
		}
	}

	#endregion
}
