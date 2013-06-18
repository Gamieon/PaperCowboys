using UnityEngine;
using System.Collections;

/// <summary>
/// This describes a single chat message. It can also describe a notification (such as
/// a player joining the game) since notifications are treated as chat messages.
/// </summary>
public class ChatMessage 
{
	/// <summary>
	/// The message.
	/// </summary>
	public string message;
	
	/// <summary>
	/// The message color.
	/// </summary>
	public Color color;
	
	public ChatMessage(string chatMessage, Color messageColor)
	{
		message = chatMessage;
		color = messageColor;
	}
}
