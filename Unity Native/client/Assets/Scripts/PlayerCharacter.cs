using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// Characters that belong to players must have a component inherited from this class.
/// What makes this class unique from Character is that it polls for commands from the owning player.
/// </summary>
public class PlayerCharacter : Character
{
	/// <summary>
	/// Gets all player characters in the scene.
	/// </summary>
	/// <value>
	/// All player characters in the scene.
	/// </value>
	static public List<PlayerCharacter> AllPlayerCharacters
	{
		get
		{
			if (null == allPlayerCharacters) 
			{
				allPlayerCharacters = new List<PlayerCharacter>();
			}
			return allPlayerCharacters;
		}
	}
	static List<PlayerCharacter> allPlayerCharacters;
	
	/// <summary>
	/// Adds the player character to AllPlayerCharacters.
	/// </summary>
	/// <param name='playerCharacter'>
	/// Player character.
	/// </param>
	static public void AddPlayerCharacter(PlayerCharacter playerCharacter)
	{
		AllPlayerCharacters.Add(playerCharacter);
	}
		
	/// <summary>
	/// Gets the minimum x that the player is allowed to go to.
	/// </summary>
	/// <value>
	/// The minimum x that the player is allowed to go to.
	/// </value>
	protected float MinimumScreenX { get { return cameraTransform.localPosition.x - 5.5f; } }
	
	/// <summary>
	/// Gets the maximum x that the player is allowed to go to.
	/// </summary>
	/// <value>
	/// The maximum x that the player is allowed to go to.
	/// </value>
	protected float MaximumScreenX { get { return cameraTransform.localPosition.x + 5.5f; } }	

	/// <summary>
	/// The cached camera transform.
	/// </summary>
	protected Transform cameraTransform;
	
	/// <summary>
	/// The cached game director.
	/// </summary>
	protected GameDirector gameDirector;
	
	#region MonoBehavior
	
	protected override void OnEnable()
	{
		base.OnEnable();
		AllPlayerCharacters.Add(this);
	}
	
	protected override void OnDisable()
	{
		AllPlayerCharacters.Remove(this);
		base.OnDisable();
	}

	protected override void Awake () 
	{
		base.Awake();
		// Cache objects
		cameraTransform = Camera.main.transform;
		gameDirector = GameDirector.Instance;
		if (networkView.isMine)
		{
			characterColor = ColorDirector.H2RGB( (float)NetworkDirector.GetCustomPlayerProperty(networkView.owner, "Player.Hue") );
		}
		characterTextureAnimator.mainColor = characterColor;
		primaryArm.renderer.material.SetColor("_Emission", characterColor);
	}	
	
	protected override void Update()
	{
		base.Update();
		if (networkView.isMine && !IsDying)
		{
			// If the player is too far to the left on the screen, they shouldn't be able to move back
			if (myTransform.position.x < MinimumScreenX)
			{
				myTransform.position = new Vector3(MinimumScreenX, myTransform.position.y, myTransform.position.z);
			}
			// Players should not be able to walk past the level boss
			else if (myTransform.position.x > MaximumScreenX)
			{
				myTransform.position = new Vector3(MaximumScreenX, myTransform.position.y, myTransform.position.z);
			}
		}
	}	
	
	#endregion
	
	/// <summary>
	/// Called by the owning client from the InputPoller.Poll function within a subclass to update
	/// the player's current weapon.
	/// </summary>
	protected virtual void UpdateGunAction()
	{	
		if (!networkView.isMine)
		{
			Debug.LogError("Attempting to call UpdateGunAction when we're not the owner!");
		}
		else if (!IsDying)
		{
			// Weapon selection
			for (KeyCode kc = KeyCode.Alpha1; (int)kc <= (int)KeyCode.Alpha4; kc++)
			{
				if (Input.GetKey(kc))
				{
					WeaponDirector.PlayerWeapon weapon = (WeaponDirector.PlayerWeapon)((int)kc - (int)KeyCode.Alpha0 - 1);
					if (PlayerDirector.ActiveSession.HasWeapon(weapon))
					{
						// Make sure the PlayerDirector is keen on the current weapon
						PlayerDirector.ActiveSession.CurrentWeapon = weapon;
						// Now assign the weapon to this character. Other clients will be updated through network serialization
						SetActiveWeapon("Player" + weapon.ToString());
					}
				}
			}		
			
			// Firing actions by mouse clicks or gamepad button presses
			if (Input.GetButtonDown("Fire1"))
			{
				// TODO: If the player fires with the gamepad, the bullets will always go on the X+ axis; but it looks
				// silly if their arm is not also on the X+ axis.
				bool fireLocalAxis = Input.GetMouseButtonDown(0);
				activeWeapon.BeginFiringPrimary(fireLocalAxis ? WeaponFiringDirection.LocalAxis : WeaponFiringDirection.XAxis);
			}
			else if (Input.GetButtonUp("Fire1"))
			{
				activeWeapon.EndFiringPrimary();
			}		
		}
	}
	
	/// <summary>
	/// Called by the owning client from the InputPoller.Poll function within a subclass to
	/// respond to keystrokes.
	/// </summary>	
	protected virtual void PollGUIKeys()
	{
		if (!networkView.isMine)
		{
			Debug.LogError("Attempting to call PollGUIKeys when we're not the owner!");
		}		
		else
		{
			// Menu invoke
			if (Input.GetKeyUp(KeyCode.Escape))
			{
				gameDirector.ShowGameMenu();
			}
			
			// Chat box invoke
			if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.KeypadEnter))
			{
				gameDirector.ShowChatBar();
			}
		}
	}
}
