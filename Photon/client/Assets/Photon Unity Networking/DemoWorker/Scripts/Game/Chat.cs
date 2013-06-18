using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// This simple chat example showcases the use of RPC targets and targetting certain players via RPCs.
/// </summary>
public class Chat : Photon.MonoBehaviour
{

    public static Chat SP;
    public List<string> messages = new List<string>();

    private int chatHeight = (int)140;
    private Vector2 scrollPos = Vector2.zero;
    private string chatInput = "";

    void Awake()
    {
        SP = this;
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(0, Screen.height - chatHeight, Screen.width, chatHeight));
        
        //Show scroll list of chat messages
        scrollPos = GUILayout.BeginScrollView(scrollPos);
        GUI.color = Color.black;
        for (int i = messages.Count - 1; i >= 0; i--)
        {
            GUILayout.Label(messages[i]);
        }
        GUILayout.EndScrollView();
        GUI.color = Color.white;

        //Chat input
        chatInput = GUILayout.TextField(chatInput);

        //Group target buttons
        GUILayout.BeginHorizontal();
        GUI.color = Color.black; GUILayout.Label("Send to:", GUILayout.Width(60)); GUI.color = Color.white;
        if (GUILayout.Button("ALL", GUILayout.Height(17)))
            SendChat(PhotonTargets.All);
        if (GUILayout.Button("ALLBUF", GUILayout.Height(17)))
            SendChat(PhotonTargets.AllBuffered);
        if (GUILayout.Button("OTHER", GUILayout.Height(17)))
            SendChat(PhotonTargets.Others);
        if (GUILayout.Button("OTHERBUF", GUILayout.Height(17)))
            SendChat(PhotonTargets.OthersBuffered);
        if (GUILayout.Button("MASTER", GUILayout.Height(17)))
            SendChat(PhotonTargets.MasterClient);
        GUILayout.EndHorizontal();

        //Player target buttons
        GUILayout.BeginHorizontal();
        GUI.color = Color.black; GUILayout.Label("Send to:", GUILayout.Width(60)); GUI.color = Color.white;
        foreach (PhotonPlayer player in PhotonNetwork.playerList)
            if (GUILayout.Button("" + player, GUILayout.MaxWidth(100), GUILayout.Height(17)))
                SendChat(player);
        GUILayout.EndHorizontal();

        GUILayout.EndArea();
    }

    public static void AddMessage(string text)
    {
        SP.messages.Add(text);
        if (SP.messages.Count > 15)
            SP.messages.RemoveAt(0);
    }


    [RPC]
    void SendChatMessage(string text, PhotonMessageInfo info)
    {
        AddMessage("[" + info.sender + "] " + text);
    }

    void SendChat(PhotonTargets target)
    {
        photonView.RPC("SendChatMessage", target, chatInput);
        chatInput = "";
    }

    void SendChat(PhotonPlayer target)
    {
        chatInput = "[PM] " + chatInput;
        photonView.RPC("SendChatMessage", target, chatInput);
        chatInput = "";
    }
}
