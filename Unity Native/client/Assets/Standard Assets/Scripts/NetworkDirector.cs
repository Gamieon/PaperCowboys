using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// The Network director which ensures all players are aware of each other.
/// </summary>
[RequireComponent(typeof(NetworkView))]
public class NetworkDirector : MonoBehaviour
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
	
	static NetworkDirector Instance
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
	public static NetworkPlayer MasterClient
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
		get { return Network.player == masterClient; }
	}
	static NetworkPlayer masterClient;
		
	/// <summary>
	/// The list of all players in the game
	/// </summary>
	public static List<NetworkPlayer> Players { get { return players; } }
	static List<NetworkPlayer> players = new List<NetworkPlayer>();
	
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
	public static void SetPlayerCustomProperty(NetworkPlayer player, string key, object data)
	{
		if (!playerCustomProperties.ContainsKey(player.guid))
		{
			playerCustomProperties.Add(player.guid, new Hashtable());
		}
		Hashtable ht = playerCustomProperties[player.guid];
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
	public static object GetCustomPlayerProperty(NetworkPlayer player, string key)
	{
		if (playerCustomProperties.ContainsKey(player.guid))
		{
			if (playerCustomProperties[player.guid].ContainsKey(key))
			{
				return playerCustomProperties[player.guid][key];
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
	public static void SetPlayerCustomProperties(Hashtable ht)
	{
		foreach (DictionaryEntry entry in ht)
		{
			SetPlayerCustomProperty(Network.player, (string)entry.Key, entry.Value);
		
			// If we're the server, then broadcast it to all other players
			if (Network.isServer)
			{
				Instance.BroadcastCustomProperty(RPCMode.Others, Network.player, (string)entry.Key, entry.Value);
			}
			// If we're a client, then send it to the server and it will do the broadcasting
			else if (Network.isClient)
			{
				Instance.BroadcastCustomProperty(RPCMode.Server, Network.player, (string)entry.Key, entry.Value);
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
	void BroadcastCustomProperty(RPCMode target, NetworkPlayer owner, string key, object data)
	{
		if (data.GetType() == typeof(string))
		{
			networkView.RPC("RPCSetPlayerCustomString", target, Network.player, key, (string)data);
		}
		else if (data.GetType() == typeof(int))
		{
			networkView.RPC("RPCSetPlayerCustomInt", target, Network.player, key, (int)data);
		}
		else if (data.GetType() == typeof(float))
		{
			networkView.RPC("RPCSetPlayerCustomFloat", target, Network.player, key, (float)data);
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
		MasterServer.ClearHostList();
	}
	
	/// <summary>
	/// Request a host list from the master server.
	/// </summary>
	/// <param name='gameTypeName'>
	/// Game type name.
	/// </param>
	public static void RequestHostList()
	{
		MasterServer.RequestHostList(GameTypeName);
	}
	
	/// <summary>
	/// Check for the latest host list received by using MasterServer.RequestHostList.
	/// </summary>
	/// <returns>
	/// The host list.
	/// </returns>
	public static HostData[] PollHostList()
	{
		return MasterServer.PollHostList();	
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
		hostedGameName = gameName;
		Network.InitializeSecurity();
		bool useNat = !Network.HavePublicAddress();
		Network.InitializeServer(connections, listenPort, useNat);
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
	public static NetworkConnectionError Connect(string IP, int remotePort)
	{
		Debug.Log("Step 1: Network.Connect");
		return Network.Connect(IP, remotePort); // (Step 1)
	}
	
	/// <summary>
	/// Disconnects from the server.
	/// </summary>
	public static void Disconnect()
	{
		if (Network.isServer)
		{
			// Reset our game name
			hostedGameName = null;
			// Unregister from the master server
			MasterServer.UnregisterHost();
		}
		Network.Disconnect();		
	}
	
	#endregion
	
	#region Object instantation and destruction
	
	/// <summary>
	/// Instantiates a game object and invokes a RPCNetworkInstantiate call
	/// if there are any additional arguments to send them to the object.
	/// </summary>
    /// <param name="prefabName">Name of the prefab to instantiate.</param>
    /// <param name="position">Position Vector3 to apply on instantiation.</param>
    /// <param name="rotation">Rotation Quaternion to apply on instantiation.</param>
    /// <param name="group">The group for this view.</param>
	/// <param name="data">Optional instantiation data.</param>
	/// </param>
	public static GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation, int group, params object[] data)
	{
		// Have Unity instantiate the object
		GameObject go = (GameObject)Network.Instantiate((GameObject)Resources.Load(prefabName), position, rotation, group);
		// Now send additional instantiation info. Since the creation was buffered, this
		// must be buffered as well.
		if (data.Length > 0)
		{
			List<object> objects = new List<object>();
			objects.Add(Network.player);
			objects.AddRange(data);
			go.networkView.RPC("RPCNetworkInstantiate", RPCMode.AllBuffered, objects.ToArray());
		}
		else
		{
			go.networkView.RPC("RPCNetworkInstantiate", RPCMode.AllBuffered, Network.player);
		}
		return go;
	}
	
    /// <summary>
    /// Instantiate a scene-owned prefab over the network. For Unity native networking purposes, this is a fancy way of
    /// telling the server to instantiate the object
    /// </summary>
    /// <remarks>
    /// Only the master client can Instantiate scene objects.
    /// </remarks>
    /// <param name="prefabName">Name of the prefab to instantiate.</param>
    /// <param name="position">Position Vector3 to apply on instantiation.</param>
    /// <param name="rotation">Rotation Quaternion to apply on instantiation.</param>
    /// <param name="group">The group for this view.</param>
    /// <param name="data">Optional instantiation data.</param>
    /// <returns>The new instance of a GameObject with initialized view.</returns>
    public static GameObject InstantiateSceneObject(string prefabName, Vector3 position, Quaternion rotation, int group, params object[] data)	
	{
		// Right now we only support this being called from the server, which is also the master client.
		if (isMasterClient)
		{
			// Have Unity instantiate the object
			GameObject go = (GameObject)Network.Instantiate((GameObject)Resources.Load(prefabName), position, rotation, group);
			// Now send additional instantiation info. Since the creation was buffered, this
			// must be buffered as well.
			if (data.Length > 0)
			{
				List<object> objects = new List<object>();
				objects.Add(Network.player);
				objects.AddRange(data);				
				go.networkView.RPC("RPCNetworkInstantiate", RPCMode.AllBuffered, objects.ToArray());
			}
			else
			{
				go.networkView.RPC("RPCNetworkInstantiate", RPCMode.AllBuffered, Network.player);
			}			
			return go;
		}
		else
		{
			Debug.LogError("Called InstantiateSceneObject from someone who isn't the master client!");
			return null;
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
		go.networkView.RPC("RPCDestroy", RPCMode.AllBuffered);
	}
	
	#endregion	
	
	#region MonoBehavior
	
	void Awake()
	{
		// Assign our instance
		_instance = this;
		// Player management is done in a separate channel.
	    networkView.group = 2;
		Network.SetSendingEnabled(2, true);
		// Keep persistent
	    DontDestroyOnLoad(this);
	}
	
	#endregion
	
	#region MonoBehavior - Server-only Unity networking events
	
	/// <summary>
	/// Called on the server whenever a Network.InitializeServer was invoked and has completed.
	/// </summary>
	void OnServerInitialized() 
	{
		// Post ourselves in the master server list
		MasterServer.RegisterHost(GameTypeName, hostedGameName);	
		// Add yourself to the player list
		RPCOnNetworkPlayerConnected(Network.player);
		// As the server, you are always the "master client"
		RPCOnMasterClientSwitched(Network.player);
    }
	
	/// <summary>
	/// Called on the server whenever a new player has successfully connected.
	/// </summary>
	/// <param name='player'>
	/// Player.
	/// </param>
	void OnPlayerConnected(NetworkPlayer player)
	{
		// Send a message to the new player that we are the master client
		networkView.RPC("RPCOnMasterClientSwitched", player, Network.player);
		// Have the new player update their player list with all the players (Step 2)
		Debug.Log("Step 2: Updating new client's player list");
		for (int i=0; i < NetworkDirector.players.Count; i++)
		{
			networkView.RPC("RPCOnNetworkPlayerConnected", player, NetworkDirector.players[i]);
		}
		// Send the new client all the custom properties (Step 3)
		Debug.Log("Step 3: Sending client all custom properties");
		for (int i=0; i < NetworkDirector.players.Count; i++)
		{		
			NetworkPlayer p = NetworkDirector.players[i];
			if (playerCustomProperties.ContainsKey(p.guid))
			{
				Hashtable ht = playerCustomProperties[p.guid];
				foreach (DictionaryEntry entry in ht)
				{
					string key = (string)entry.Key;
					object data = entry.Value;
					if (data.GetType() == typeof(string))
					{
						networkView.RPC("RPCSetPlayerCustomString", player, p, key, (string)data);
					}
					else if (data.GetType() == typeof(int))
					{
						networkView.RPC("RPCSetPlayerCustomInt", player, p, key, (int)data);
					}
					else if (data.GetType() == typeof(float))
					{
						networkView.RPC("RPCSetPlayerCustomFloat", player, p, key, (float)data);
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
		networkView.RPC("RPCValidationFinished", player);
	}
	
	/// <summary>
	/// Called on the server whenever a player is disconnected from the server.
	/// </summary>
	/// <param name='player'>
	/// Player.
	/// </param>
	void OnPlayerDisconnected(NetworkPlayer player)
	{
		// Remove all the bread crumbs the player left us
		Network.RemoveRPCs(player);
		Network.DestroyPlayerObjects(player);
		// Now tell everyone the player disconnected
		networkView.RPC("RPCOnNetworkPlayerDisconnected", RPCMode.All, player);
	}
	
	#endregion
	
	#region MonoBehavior - Client-only Unity networking events
	
	/// <summary>
	/// Called on the client when a connection attempt fails for some reason.
	/// </summary>
	/// <param name='error'>
	/// Error.
	/// </param>
    void OnFailedToConnect(NetworkConnectionError error) 
	{
        Debug.Log("Could not connect to server: " + error);
    }
	
	/// <summary>
	/// Called on the client when you have successfully connected to a server.
	/// </summary>
	void OnConnectedToServer() 
	{
        Debug.Log("Connected to server");
    }
	
	/// <summary>
	/// Called on the client when the connection was lost or you disconnected from the server.
	/// </summary>
	/// <param name='info'>
	/// Info.
	/// </param>
    void OnDisconnectedFromServer(NetworkDisconnection info)
	{
        Debug.Log("Disconnected from server: " + info);
		// Clear the custom properties
		playerCustomProperties = new Dictionary<string, Hashtable>();
		// Clear the player list
		players = new List<NetworkPlayer>();
    }
	
	#endregion

	#region Network Director RPCs
	
	/// <summary>
	/// This is an RPC sent from the server to everybody that a player connected to the server
	/// </summary>
	/// <param name='player'>
	/// Player.
	/// </param>
	[RPC]
	void RPCOnNetworkPlayerConnected(NetworkPlayer player)
	{
		// Add this player to our local list (even if it's ourself)
		players.Add(player);
	}
	
	/// <summary>
	/// This is an RPC sent from the server to everybody that the master client has changed
	/// </summary>
	/// <param name='player'>
	/// The new master client.
	/// </param>
	[RPC]
	void RPCOnMasterClientSwitched(NetworkPlayer player)
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
	void RPCOnNetworkPlayerDisconnected(NetworkPlayer player)
	{
		// Remove the player's properties
		playerCustomProperties.Remove(player.guid);
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
		if (playerCustomProperties.ContainsKey(Network.player.guid))
		{
			Hashtable ht = playerCustomProperties[Network.player.guid];
			foreach (DictionaryEntry entry in ht)
			{
				BroadcastCustomProperty(RPCMode.Server, Network.player, (string)entry.Key, entry.Value);
			}
		}
		
		// Send a message to everyone that you officially joined (Step 6). It's important this is done
		// AFTER everyone has your properties so that everyone can freely iterate the player list and
		// the properties therein.
		// 
		Debug.Log("Step 6: Broadcasting RPCOnNetworkPlayerConnected");
		networkView.RPC("RPCOnNetworkPlayerConnected", RPCMode.All, Network.player);
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
	void RPCSetPlayerCustomString(NetworkPlayer player, string key, string data)
	{
		// Update the property in local memory
		SetPlayerCustomProperty(player, key, data);
		// If we're the server, this needs to be sent to all the other clients
		if (Network.isServer)
		{
			foreach (NetworkPlayer p in NetworkDirector.Players)
			{
				if (p != player && p != Network.player)
				{
					networkView.RPC("RPCSetPlayerCustomString", p, key, data);
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
	void RPCSetPlayerCustomInt(NetworkPlayer player, string key, int data)
	{
		// Update the property in local memory
		SetPlayerCustomProperty(player, key, data);
		// If we're the server, this needs to be sent to all the other clients
		if (Network.isServer)
		{
			foreach (NetworkPlayer p in NetworkDirector.Players)
			{
				if (p != player && p != Network.player)
				{
					networkView.RPC("RPCSetPlayerCustomInt", p, key, data);
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
	void RPCSetPlayerCustomFloat(NetworkPlayer player, string key, float data)
	{
		// Update the property in local memory
		SetPlayerCustomProperty(player, key, data);
		// If we're the server, this needs to be sent to all the other clients
		if (Network.isServer)
		{
			foreach (NetworkPlayer p in NetworkDirector.Players)
			{
				if (p != player && p != Network.player)
				{
					networkView.RPC("RPCSetPlayerCustomFloat", p, key, data);
				}
			}
		}
	}
	
	#endregion
}
