#define PLAYER_TAKES_DAMAGE

using UnityEngine;
using System.Collections;

/// <summary>
/// Characters that belong to players may have this as a component. What makes this
/// unique from PlayerCharacter is it polls for movements, as well as damage management.
/// </summary>
public sealed class PlayerOnFoot : PlayerCharacter,
	IInputPoller, IDamageTaker
{		
	/// <summary>
	/// The minimum X position we can go to
	/// </summary>
	float minimumX;
	
	/// <summary>
	/// The minimum camera Y position we can go to
	/// </summary>
	float minimumCameraY;
	
	#region MonoBehavior

	protected override void Awake () 
	{
		base.Awake();
		// Assign the minimum x
		minimumX = myTransform.position.x;
		minimumCameraY = cameraTransform.localPosition.y;
		// Assign player color
		characterTextureAnimator.mainColor = GetColor();
		primaryArm.renderer.material.SetColor("_Emission", GetColor());
	}
	
	void LateUpdate()
	{
		// If we're controlling this player, we need to update the camera
		if (photonView.isMine && !IsDying)
		{
			// Update the minimum x
			minimumX = Mathf.Max(minimumX, myTransform.localPosition.x);
			minimumCameraY = Mathf.Max(minimumCameraY, myTransform.localPosition.y);
			
			// Calibrate the camera so it's x-centered on the player's position
			cameraTransform.localPosition = new Vector3(
				Mathf.Min(gameDirector.MaximumCameraX, Mathf.Max(minimumX, myTransform.localPosition.x)), 
				Mathf.Min(gameDirector.MaximumCameraY, Mathf.Max(minimumCameraY, myTransform.localPosition.y)), 
				cameraTransform.localPosition.z);					
			
			// See if we're beneath the screen. If so, that's an auto-death.
			if (myTransform.position.y < cameraTransform.localPosition.y - 6.0f) 
			{
				// Tell everybody that this character is dying
				photonView.RPC("RPCPlayerOnFootBeginDying", PhotonTargets.All);
			}
		}
		
		// This can only be run by the master client		
		if (PhotonNetwork.isMasterClient)
		{
			// See if we're touching an enemy in front of or behind us. If so, that's an auto-death.
			RaycastHit hitInfo;
			if (Physics.Raycast(myTransform.position, new Vector3(1,0,0), out hitInfo, 1.0f, (1 << (int)GameDirector.GameLayers.Enemy)) ||
				Physics.Raycast(myTransform.position, new Vector3(-1,0,0), out hitInfo, 1.0f, (1 << (int)GameDirector.GameLayers.Enemy)) )
			{
				EnemyCharacter enemyCharacter = hitInfo.collider.gameObject.GetComponent<EnemyCharacter>();
				if (null != enemyCharacter && !enemyCharacter.IsDying)
				{
#if PLAYER_TAKES_DAMAGE
					// Tell everybody that this character is dying
					photonView.RPC("RPCPlayerOnFootBeginDying", PhotonTargets.All);
#endif
				}
			}			
		}
	}
	
	#endregion
	
	#region Character functions
	
	/// <summary>
	/// Called by the owning client to destroy this character
	/// </summary>	
	protected override void DestroyThisCharacter()
	{
		if (photonView.isMine)
		{
			// If we get here, we must be the owner. So have the game director start a respawn timer.
			GameDirector.Instance.SendMessage("OnSelfCharacterDestroyed");
			
			// This must always be called at the end because it destroys the game object
			base.DestroyThisCharacter();
		}
		else
		{
			Debug.LogError("Tried to destroy a character that wasn't ours!");
		}
	}
	
	#endregion
	
	#region PlayerCharacter functions
	
	/// <summary>
	/// Called by the owning client from IInputPoller.Poll to update player movement states
	/// </summary>
	void UpdatePlayerMovements()
	{		
		// Only do movements if the player is upright
		if (IsCharacterGrounded)
		{			
			// Crouching
			crouchActive = Input.GetAxisRaw("Vertical") < 0;
			
			// Walking to the left
			walkLeftActive = Input.GetAxisRaw("Horizontal") < 0;
			
			// Walking to the right
			walkRightActive = Input.GetAxisRaw("Horizontal") > 0;
			
			// Jumping
			jumpActive = Input.GetButton("Jump");
			
			// Ledge jumping (up direction)
			ledgeJumpIntentActive = Input.GetAxisRaw("Vertical") > 0;
		}	
	}
	
	/// <summary>
	/// Called by the owning client from IInputPoller.Poll to update the gun aim.
	/// </summary>
	void UpdateAim()
	{
		Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
		Plane screenPlane = new Plane(new Vector3(0,0,1), new Vector3(0,0,GameDirector.ZPositions_Foreground));
		float d;
		screenPlane.Raycast(ray, out d);
		Vector3 screenPoint = (ray.origin + ray.direction * d);
		
		float angle = Vector3.Angle(Vector3.right, screenPoint - primaryArmPivot.position);
		if (screenPoint.y < primaryArmPivot.position.y) { angle = 360.0f - angle; }
		
		PrimaryArmPivot = angle;
	}
	
	#endregion
	
	#region Character functions (RPCs)
		
	/// <summary>
	/// Sent from the master client observing something damaging this object to all clients
	/// to inform them that the object is being damaged.
	/// </summary>
	/// <param name='damage'>
	/// Damage.
	/// </param>
	[RPC]
	public void RPCPlayerOnFootTakeDamage(float damage)
	{
		// Deduct the hitpoint count
		hitPoints -= damage;

		if (PhotonNetwork.isMasterClient)
		{
#if PLAYER_TAKES_DAMAGE
			// If the character is doomed, then begin its VISUAL death sequence. Other players
			// will discover this with the next serialization; and you, as the master client, 
			// will eventually destroy the character entirely.
			if (hitPoints - damage < MinHitpointsBeforeFrag)
			{
				// Tell everybody that this character is dying
				photonView.RPC("RPCPlayerOnFootBeginDying", PhotonTargets.All);
			}
#endif
		}
	}
	
	/// <summary>
	/// Sent from the master client to all clients to inform them this object is dying
	/// </summary>
	[RPC]
	public void RPCPlayerOnFootBeginDying()
	{
		if (!IsDying) 
		{
			BeginDying();
		}
		else
		{
			// This should never happen
		}
	}
	
	#endregion
	
	#region PhotonNetworkingMessage

    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        Debug.Log("OnPhotonInstantiate PLAYERCHARACTER " + info.sender);
		gameObject.name = info.sender.name;
    }
	
	#endregion
	
	#region IInputPoller
	
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
		// Don't do anything if the player is dying
		if (!IsDying)
		{
			// Check for keystrokes that affect the GUI
			PollGUIKeys();			
			
			// Player movements
			UpdatePlayerMovements();
			
			// Player gun aiming
			UpdateAim();
			
			// Update gun actions
			UpdateGunAction();			
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
	}
	
	#endregion
	
	#region IDamageTaker
	
	/// <summary>
	/// Called by the player observing their projectile making contact with this character to inflict
	/// damage to this character. This must never be called from an RPC.
	/// </summary>
	/// <param name='damager'>
	/// An interface to the object dealing the damage.
	/// </param>	
	void IDamageTaker.TakeDamage(IDamageDealer damageDealer)
	{
		if (!IsDying)
		{
#if PLAYER_TAKES_DAMAGE
			float damage = damageDealer.GetDamage();
			// Tell everybody that this barrel is taking damage
			photonView.RPC("RPCPlayerOnFootTakeDamage", PhotonTargets.All, damage);
#endif
		}
		else
		{
			// Ignore if the character is already dying
		}
	}
	
	#endregion	
}
