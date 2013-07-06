#define PLAYER_TAKES_DAMAGE

using UnityEngine;
using System.Collections;

/// <summary>
/// Characters that belong to players may have this as a component. What makes this
/// unique from PlayerCharacter is it polls for movements, as well as damage management.
/// </summary>
public sealed class PlayerHorseback : PlayerCharacter,
	IInputPoller, IDamageTaker
{		
	/// <summary>
	/// The horse speed
	/// </summary>
	float horseRunSpeed = 1.5f;
	
	/// <summary>
	/// The X position of the horse
	/// </summary>
	float horseX;
	
	/// <summary>
	/// The minimum y for the horse.
	/// </summary>
	float MinimumY = 0.3525353f;
	
	/// <summary>
	/// The maximum y for the horse.
	/// </summary>
	float MaximumY = 2.487146f;
	
	#region MonoBehavior

	protected override void Start () 
	{
		base.Start();
		// Assign horse position
		horseX = myTransform.position.x;
		// Assign accessory offset; for horses this needs to be down and to the right
		accessoryPositionOffset = new Vector3(0.057308373f, -0.061727f, 0);
		// Override necessary magic numbers
		primaryArmPivotDefault = primaryArmPivot.transform.localPosition;
		// Override speeds
		walkingSpeed = 5.0f;
	}
	
	protected override void Update()
	{
		// Keep the character moving right
		if (horseX > gameDirector.MaximumCameraX) { horseRunSpeed = 0; }
		if (null != myCharacterController) { myCharacterController.Move(new Vector3(horseRunSpeed * Time.deltaTime,0,0)); }
		horseX += horseRunSpeed * Time.deltaTime;
		base.Update();	
		// Don't let the player Y go beyond the Y bounds
		if (myTransform.position.y < MinimumY) {
			myTransform.position = new Vector3(myTransform.position.x, MinimumY, myTransform.position.z);
		}
		if (myTransform.position.y > MaximumY) {
			myTransform.position = new Vector3(myTransform.position.x, MaximumY, myTransform.position.z);
		}
	}
	
	void LateUpdate()
	{
		// If we're controlling this player, we need to update the camera
		if (myNetworkView.isMine && !IsDying)
		{
			// Calibrate the camera so it's x-centered on the player's position
			cameraTransform.localPosition = new Vector3(
				horseX, 
				cameraTransform.localPosition.y, 
				cameraTransform.localPosition.z);					
		}
		
		if (myNetworkView.isMine)
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
					myNetworkView.RPC("RPCPlayerCharacterHorseBeginDying", uLink.RPCMode.All);
#endif
				}
			}		
		}
	}
	
	#endregion
	
	#region Character functions
	
	protected override Posture CalculatePostureFromPlayerMovement()
	{
		return Posture.WalkingRight;
	}
	
	protected override void DestroyThisCharacter()
	{
		if (myNetworkView.isMine)
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
		// Moving up
		walkUpActive = Input.GetAxisRaw("Vertical") > 0;
	
		// Moving down
		walkDownActive = Input.GetAxisRaw("Vertical") < 0;
		
		// Moving to the left
		walkLeftActive = Input.GetAxisRaw("Horizontal") < 0;
		
		// Moving to the right
		walkRightActive = Input.GetAxisRaw("Horizontal") > 0;
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
	public void RPCPlayerCharacterHorseTakeDamage(float damage)
	{
		// Deduct the hitpoint count
		hitPoints -= damage;
		
		if (NetworkDirector.isMasterClient)
		{
#if PLAYER_TAKES_DAMAGE
			// If the character is doomed, then begin its VISUAL death sequence. Other players
			// will discover this with the next serialization; and you, as the master client, 
			// will eventually destroy the character entirely.
			if (hitPoints - damage < MinHitpointsBeforeFrag)
			{
				// Tell everybody that this character is dying
				myNetworkView.RPC("RPCPlayerCharacterHorseBeginDying", uLink.RPCMode.All);
			}
#endif
		}
	}
	
	/// <summary>
	/// Invoked for everybody but the master client (who already knows this character is dying) to
	/// inform them the character is dying.
	/// </summary>
	[RPC]
	public void RPCPlayerCharacterHorseBeginDying()
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
	
	/// <summary>
	/// Called by the NetworkDirector to destroy this game object.
	/// </summary>
	[RPC]
	public void RPCDestroy()
	{
		Destroy(gameObject);
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
			myNetworkView.RPC("RPCPlayerCharacterHorseTakeDamage", uLink.RPCMode.All, damage);
#endif
		}
		else
		{
			// Ignore if the character is already dying
		}
	}
	
	#endregion	
}
