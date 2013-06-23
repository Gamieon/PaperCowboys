using UnityEngine;
using System.Collections;

/// <summary>
/// Any player or enemy person-like object must have a component inherited from this
/// class. This class dictates a character's appearance in the game.
/// 
/// The attached object can be owned by any client.
/// 
/// </summary>
[RequireComponent(typeof(CharacterTextureFlasher))]
[RequireComponent(typeof(AudioSource))]
public class Character : SynchronizedObject
{
	#region Magic Numbers
	
	protected float characterControllerHeightDefault = 1.0f;
	protected Vector3 characterControllerCenterDefault = Vector3.zero;
	protected Vector3 primaryArmPivotDefault = new Vector3(0,0.1843f,0);
	
	protected float characterControllerHeightCrouched = 0.5f;
	protected Vector3 characterControllerCenterCrouched = new Vector3(0,-0.2f,0);
	protected Vector3 primaryArmPivotCrouched = new Vector3(0,-0.1495f,0);
	
	/// <summary>
	/// When a player gets a hat or banadana, the prefab's local position dictates its
	/// instantiated position. However, as not all character models are equal, we may
	/// need to shift its position.
	/// </summary>
	protected Vector3 accessoryPositionOffset = Vector3.zero;
	
	/// <summary>
	/// The change in position of the player's head when they are crouched
	/// </summary>
	Vector3 crouchHeadDelta = new Vector3(0,-0.3338f,0);
	
	protected float characterHeight = 1.5f;
	
	protected float dyingStartTime = -1;
	protected float dyingSpeed = 2.0f;

	#endregion
	
	#region Enumerations for serialization
	
	enum CharacterWeaponType
	{
		None = 0,
		PlayerPistol = 1,
		PlayerRocketLauncher = 2,
		PlayerShotgun = 3,
		PlayerWinchesta = 4,
		EnemyDynamite,
		EnemyKnife,
		EnemyMachineGun,
		EnemyPistol,
		
	};
	
	enum CharacterHatType
	{
		None = 0,
		CowboyHat = 1,
		EastwoodHat = 2,
		TenGallonHat = 3,
	}
	
	enum CharacterMouthpieceType
	{
		None = 0,
		Bandana = 1,
	}
	
	#endregion
	
	#region Body Appearance
	
	public enum Posture
	{
		Standing,
		WalkingLeft,
		WalkingRight,
		CrouchingLeft,
		CrouchingRight,
		CrawlingLeft,
		CrawlingRight,
		Dying,
	}
	
	public enum FacingDirection
	{
		Left,
		Right,
	}
	
	/// <summary>
	/// Describes how a player can move about the screen
	/// </summary>
	public enum MovementMode
	{
		/// <summary>
		/// In this mode, a player can move left and right, and jump. The up direction
		/// directly corresponds to height. The player is also subject to gravity.
		/// </summary>
		LeftRightJump,
		
		/// <summary>
		/// The player can freely move around the screen in all directions. Gravity
		/// does not apply, and there is no sense of "up."
		/// </summary>
		FullFreedom,
	}
	
	/// <summary>
	/// The character texture animator.
	/// </summary>
	public CharacterTextureAnimator characterTextureAnimator;	
	
	/// <summary>
	/// The movement mode.
	/// </summary>
	public MovementMode movementMode;
	
	/// <summary>
	/// The walking frames.
	/// </summary>
	public Material[] walkingFrames;
	
	/// <summary>
	/// The crouching frames.
	/// </summary>
	public Material[] crouchingFrames;
	
	/// <summary>
	/// The material displayed when the player is standing up
	/// </summary>
	public Material stillMaterial;
	
	/// <summary>
	/// The material displayed when the player is crouching
	/// </summary>
	public Material crouchingMaterial;
	
	/// <summary>
	/// The hat renderer.
	/// </summary>
	public Renderer hatRenderer;
		
	/// <summary>
	/// The mouthpiece renderer.
	/// </summary>
	public Renderer mouthpieceRenderer;
			
	/// <summary>
	/// The primary arm used to hold a weapon
	/// </summary>
	public Transform primaryArm;
		
	/// <summary>
	/// Walking speed
	/// </summary>
	protected float walkingSpeed = 3.0f;
	
	/// <summary>
	/// The vertical walking speed for when a player walks up or down on the screen
	/// </summary>
	protected float verticalWalkingSpeed = 3.0f;
	
	/// <summary>
	/// The hat position when the player is upright
	/// </summary>
	Vector3 normalHatPosition;

	/// <summary>
	/// The mouth gear position when the player is upright
	/// </summary>
	Vector3 normalMouthpiecePosition;
	
	/// <summary>
	/// The facing direction.
	/// </summary>
	protected FacingDirection facingDirection = FacingDirection.Right;

	/// <summary>
	/// Gets or sets the current posture.
	/// </summary>
	/// <value>
	/// The current posture.
	/// </value>
	protected Posture CurrentPosture 
	{
		get { return currentPosture; }	
		set {
			if (value != currentPosture)
			{
				currentPosture = value;
				UpdatePosture();
			}
		}
	}
	Posture currentPosture = Posture.Standing;
	
	/// <summary>
	/// Updates the visible materials based on the current posture
	/// </summary>
	protected void UpdatePosture()
	{		
		switch (currentPosture)
		{
		case Posture.CrouchingLeft:
			characterTextureAnimator.state = CharacterTextureAnimator.State.Still;
			characterTextureAnimator.stillFrame = crouchingMaterial;
			characterTextureAnimator.textureScale = new Vector2(-1,0.99f);
			characterTextureAnimator.stillFrame = crouchingMaterial;
			if (null != myCharacterController)
			{
				myCharacterController.height = characterControllerHeightCrouched;
				myCharacterController.center = characterControllerCenterCrouched;
			}
			primaryArmPivot.localPosition = primaryArmPivotCrouched;
			if (null != hatRenderer) { hatRenderer.transform.localPosition = normalHatPosition + crouchHeadDelta; }
			if (null != mouthpieceRenderer) { mouthpieceRenderer.transform.localPosition = normalMouthpiecePosition + crouchHeadDelta; }
			break;
			
		case Posture.CrawlingLeft:
			characterTextureAnimator.state = CharacterTextureAnimator.State.Moving;
			characterTextureAnimator.movingFrames = crouchingFrames;
			characterTextureAnimator.textureScale = new Vector2(-1,0.99f);
			characterTextureAnimator.stillFrame = crouchingMaterial;
			if (null != myCharacterController)
			{
				myCharacterController.height = characterControllerHeightCrouched;
				myCharacterController.center = characterControllerCenterCrouched;
			}
			primaryArmPivot.localPosition = primaryArmPivotCrouched;
			if (null != hatRenderer) { hatRenderer.transform.localPosition = normalHatPosition + crouchHeadDelta; }
			if (null != mouthpieceRenderer) { mouthpieceRenderer.transform.localPosition = normalMouthpiecePosition + crouchHeadDelta; }
			break;
			
		case Posture.CrouchingRight:
			characterTextureAnimator.state = CharacterTextureAnimator.State.Still;
			characterTextureAnimator.stillFrame = crouchingMaterial;
			characterTextureAnimator.textureScale = new Vector2(1,0.99f);
			characterTextureAnimator.stillFrame = crouchingMaterial;
			if (null != myCharacterController)
			{
				myCharacterController.height = characterControllerHeightCrouched;
				myCharacterController.center = characterControllerCenterCrouched;
			}
			primaryArmPivot.localPosition = primaryArmPivotCrouched;
			if (null != hatRenderer) { hatRenderer.transform.localPosition = normalHatPosition + crouchHeadDelta; }
			if (null != mouthpieceRenderer) { mouthpieceRenderer.transform.localPosition = normalMouthpiecePosition + crouchHeadDelta; }
			break;
			
		case Posture.CrawlingRight:
			characterTextureAnimator.state = CharacterTextureAnimator.State.Moving;
			characterTextureAnimator.movingFrames = crouchingFrames;
			characterTextureAnimator.textureScale = new Vector2(1,0.99f);
			characterTextureAnimator.stillFrame = crouchingMaterial;
			if (null != myCharacterController)
			{
				myCharacterController.height = characterControllerHeightCrouched;
				myCharacterController.center = characterControllerCenterCrouched;
			}
			primaryArmPivot.localPosition = primaryArmPivotCrouched;
			if (null != hatRenderer) { hatRenderer.transform.localPosition = normalHatPosition + crouchHeadDelta; }
			if (null != mouthpieceRenderer) { mouthpieceRenderer.transform.localPosition = normalMouthpiecePosition + crouchHeadDelta; }
			break;
			
		case Posture.Standing:
			characterTextureAnimator.state = CharacterTextureAnimator.State.Still;
			characterTextureAnimator.stillFrame = stillMaterial;
			if (null != myCharacterController)
			{
				myCharacterController.height = characterControllerHeightDefault;
				myCharacterController.center = characterControllerCenterDefault;
			}
			primaryArmPivot.localPosition = primaryArmPivotDefault;
			if (null != hatRenderer) { hatRenderer.transform.localPosition = normalHatPosition; }
			if (null != mouthpieceRenderer) { mouthpieceRenderer.transform.localPosition = normalMouthpiecePosition; }
			break;
		case Posture.WalkingLeft:
			characterTextureAnimator.state = CharacterTextureAnimator.State.Moving;
			characterTextureAnimator.movingFrames = walkingFrames;
			characterTextureAnimator.textureScale = new Vector2(-1,0.99f);
			if (null != myCharacterController)
			{
				myCharacterController.height = characterControllerHeightDefault;
				myCharacterController.center = characterControllerCenterDefault;
			}
			primaryArmPivot.localPosition = primaryArmPivotDefault;
			if (null != hatRenderer) { hatRenderer.transform.localPosition = normalHatPosition; }
			if (null != mouthpieceRenderer) { mouthpieceRenderer.transform.localPosition = normalMouthpiecePosition; }
			break;
		case Posture.WalkingRight:
			characterTextureAnimator.state = CharacterTextureAnimator.State.Moving;
			characterTextureAnimator.movingFrames = walkingFrames;
			characterTextureAnimator.textureScale = new Vector2(1,0.99f);
			if (null != myCharacterController)
			{
				myCharacterController.height = characterControllerHeightDefault;
				myCharacterController.center = characterControllerCenterDefault;
			}
			primaryArmPivot.localPosition = primaryArmPivotDefault;
			if (null != hatRenderer) { hatRenderer.transform.localPosition = normalHatPosition; }
			if (null != mouthpieceRenderer) { mouthpieceRenderer.transform.localPosition = normalMouthpiecePosition; }
			break;
		}
	}
	
	/// <summary>
	/// The mesh flasher for when damage is done.
	/// </summary>
	protected CharacterTextureFlasher characterTextureFlasher;
	
	/// <summary>
	/// Gets or sets the primary arm pivot.
	/// </summary>
	/// <value>
	/// The primary arm pivot.
	/// </value>
	public float PrimaryArmPivot
	{
		get 
		{
			return primaryArmPivot.localEulerAngles.z;
		}
		set 
		{
			// Rotate the primary arm
			if (null != primaryArmPivot)
			{
				primaryArmPivot.localEulerAngles = new Vector3(0,0,value);
			}
			else
			{
				// If we get here, the character is dying
			}
	
			// Now update the weapon appearance
			if (null != activeWeaponObject)
			{
				if (activeWeaponObject.transform.position.x < myTransform.position.x)
				{
					activeWeaponObject.renderer.material.mainTextureScale = new Vector2(1,-1);
					activeWeaponObject.transform.localPosition = new Vector3(0.6356475f, -1.4f, 0);
				}
				else
				{
					activeWeaponObject.renderer.material.mainTextureScale = new Vector2(1,1);
					activeWeaponObject.transform.localPosition = new Vector3(0.6356475f, 1.4f, 0);
				}
			}
			else
			{
				// If we get here, the character is dying
			}
		}
	}
	/// <summary>
	/// The pivot of the primary arm
	/// </summary>
	public Transform primaryArmPivot;
		
	/// <summary>
	/// Gets the name of the active hat.
	/// </summary>
	/// <value>
	/// The name of the active hat.
	/// </value>
	public string ActiveHatName { get { return (null != hatRenderer && hatRenderer.enabled) ? hatRenderer.name : ""; } }
	
	/// <summary>
	/// Gets the name of the active mouthpiece.
	/// </summary>
	/// <value>
	/// The name of the active mouthpiece.
	/// </value>
	public string ActiveMouthpieceName { get { return (null != mouthpieceRenderer && mouthpieceRenderer.enabled) ? mouthpieceRenderer.name : ""; } }		
	
	#endregion
	
	/// <summary>
	/// The audio source.
	/// </summary>
	public AudioSource audioSource;
	
	/// <summary>
	/// Random death groans
	/// </summary>
	public AudioClip[] sndGroans;
	
	/// <summary>
	/// Cached character controller.
	/// </summary>
	protected CharacterController myCharacterController;
	
	/// <summary>
	/// Cached collider
	/// </summary>
	protected Collider myCollider;

	/// <summary>
	/// The character controller collision flags.
	/// </summary>
	protected CollisionFlags collisionFlags;
	
	/// <summary>
	/// The vertical speed of the character. It can be non-zero if the
	/// character is jumping or falling
	/// </summary>
	protected float verticalSpeed;
	
	/// <summary>
	/// True if the character is jumping. If the character is in vertical
	/// motion, we must be able to discern a jump from a free fall.
	/// </summary>
	protected bool isJumping;
	
	/// <summary>
	/// The previous position.
	/// </summary>
	protected Vector3 prevPos;
 	
	/// <summary>
	/// Gets the screen velocity.
	/// </summary>
	/// <value>
	/// The screen velocity.
	/// </value>
	protected Vector3 ScreenVelocity { get { return myTransform.position - prevPos; } }	

	/// <summary>
	/// Gets a value indicating whether the character controller is grounded
	/// </summary>
	/// <value>
	/// <c>true</c> if the character controller is grounded; otherwise, <c>false</c>.
	/// </value>
	protected bool IsCharacterGrounded
	{ 
		// In full freedom mode, there is no jumping or sense of up; therefore the character is always grounded.
		// Otherwise we have to look at the character controller collision flags.
		get { return (MovementMode.FullFreedom == movementMode) ? true : ((collisionFlags & CollisionFlags.CollidedBelow) != 0); }
	}
	
	/// <summary>
	/// Gets a value indicating whether the player is visibly grounded. This is different from
	/// the character being grounded because the character collider is always fighting with the
	/// ground to stay in the same position. If we used IsCharacterGrounded to determine posture
	/// then the character would appear to violently flicker from standing to crouching position
	/// and back.
	/// </summary>
	/// <value>
	/// <c>true</c> if the player is visibly grounded; otherwise, <c>false</c>.
	/// </value>
	protected bool IsVisiblyGrounded
	{ 
		get {
			// TODO: Figure out why the ray length has to be this long!
			return Physics.Raycast(myTransform.position, -Vector3.up, characterHeight * 1.0f, 
				(1 << (int)GameDirector.GameLayers.Default) | (1 << (int)GameDirector.GameLayers.Ledge));
		}
	}

	/// <summary>
	/// Gets a value indicating whether this character is dying. It is visibly dying, but
	/// the master client has to destory it.
	/// </summary>
	/// <value>
	/// <c>true</c> if this character is dying; otherwise, <c>false</c>.
	/// </value>
	public bool IsDying { get { return (dyingStartTime > 0); } }
	
	/// <summary>
	/// Gets the minimum hitpoints before the player dies.
	/// </summary>
	/// <value>
	/// The minimum hitpoints before the player dies.
	/// </value>
	protected float MinHitpointsBeforeFrag { get { return 0.001f; } }
	
	/// <summary>
	/// True if the character is crouching and the character belongs to ourselves (networkView.isMine is true)
	/// </summary>
	protected bool crouchActive;
	
	/// <summary>
	/// True if the player is walking upwards and the character belongs to ourselves (networkView.isMine is true).
	/// </summary>
	protected bool walkUpActive;	

	/// <summary>
	/// True if the player is walking downwards and the character belongs to ourselves (networkView.isMine is true).
	/// </summary>
	protected bool walkDownActive;	
	
	/// <summary>
	/// True if the player is walking left and the character belongs to ourselves (networkView.isMine is true)
	/// </summary>
	protected bool walkLeftActive;

	/// <summary>
	/// True if the player is walking right and the character belongs to ourselves (networkView.isMine is true)
	/// </summary>
	protected bool walkRightActive;
	
	/// <summary>
	/// True if the player is trying to jump and the character belongs to ourselves (networkView.isMine is true)
	/// </summary>
	protected bool jumpActive;
	
	/// <summary>
	/// True if the player is holding a key where, when combined with jumping, will result in jumping onto
	/// or under a ledge.
	/// </summary>
	protected bool ledgeJumpIntentActive;
	
	/// <summary>
	/// The ledge we're trying to get to when jumping
	/// </summary>
	protected Collider targetLedge;
	
	/// <summary>
	/// The ledge we're trying to fall through
	/// </summary>
	protected Collider fallThroughLedge;
	
	#region Properties

	/// <summary>
	/// Gets the name of the active weapon.
	/// </summary>
	/// <value>
	/// The name of the active weapon.
	/// </value>
	protected string ActiveWeaponName { get { return (null == activeWeaponObject) ? "" : activeWeaponObject.name; } }
	
	/// <summary>
	/// The active weapon object.
	/// </summary>
	GameObject activeWeaponObject;
	
	/// <summary>
	/// The active weapon.
	/// </summary>
	protected IWeapon activeWeapon;
		
	/// <summary>
	/// Character hit points.
	/// </summary>
	public float hitPoints = 1.0f;
	
	/// <summary>
	/// The number of points awarded to a player who frags this object.
	/// </summary>
	public int pointValue;
	
	/// <summary>
	/// The default weapon prefab that the character will carry
	/// </summary>
	public string defaultWeaponPrefab;
			
	#endregion
	
	#region Synchronization Variables
	
	/// <summary>
	/// Synchronization: The correct primary arm pivot.
	/// </summary>
	protected Quaternion correctPrimaryArmPivot = Quaternion.identity;
	
	/// <summary>
	/// The color of the character.
	/// </summary>
	protected Color characterColor = Color.black;
	
	#endregion
	
	#region MonoBehavior
	
	protected override void Awake()
	{
		base.Awake();
		
		// Cache values
		characterTextureFlasher = GetComponent<CharacterTextureFlasher>();
		normalHatPosition = accessoryPositionOffset + ((null != hatRenderer) ? hatRenderer.transform.localPosition : Vector3.zero);
		normalMouthpiecePosition = accessoryPositionOffset + ((null != mouthpieceRenderer) ? mouthpieceRenderer.transform.localPosition : Vector3.zero);
		myTransform = transform;
		myCharacterController = GetComponent<CharacterController>();
		audioSource = GetComponent<AudioSource>();
		collisionFlags = CollisionFlags.CollidedBelow;		
		myCollider = collider;
		prevPos = myTransform.position;
		
		// Default serialization values
		correctPrimaryArmPivot = (null == primaryArmPivot) ? Quaternion.identity : primaryArmPivot.localRotation;
		
		// Percolate the layers to all children
		UtilitiesDirector.SetLayerRecurse(myTransform, gameObject.layer);
		
		// Now give the player their default weapon
		if (!string.IsNullOrEmpty(defaultWeaponPrefab))
		{
			SetActiveWeapon(defaultWeaponPrefab);
		}
	}
	
	protected override void Update()
	{
		base.Update();
		
		// If this chracter doesn't belong to us, update the primary arm pivot based on what the owning player says it is
        if (!networkView.isMine)
        {
			PrimaryArmPivot = Quaternion.Lerp(primaryArmPivot.localRotation, correctPrimaryArmPivot, Time.deltaTime * 5).eulerAngles.z;
        }
		// If this character belongs to us and we actually control it, deal with movements here. That includes checking
		// the environment for obstructions ourselves.
		
		// It's critical to note that the actual keyboard/mouse/gamepad commands that assign values like walkLeftActive
		// and movementMode are not polled in this function!
		
		// TODO: Should the server somehow have any say in this?
		else if (null != myCharacterController)
		{
			float horizontalSpeed = 0; // For walking left and right
			
			// Always let a player move left and right even if they're airborne or crouching
			if (walkLeftActive)
			{
				horizontalSpeed = -walkingSpeed;
			}
			else if (walkRightActive)
			{
				horizontalSpeed = walkingSpeed;
			}		
			
			// Handle character movement where the mode involves moving left, right, and jumping
			if (MovementMode.LeftRightJump == movementMode)
			{
				// See if the player pressed the jump button
				if (jumpActive && !isJumping)
				{
					// Ok they pressed the jump button; but what is the intent? Are they trying
					// to get onto a ledge, slip under a ledge, or just jump?
					bool traverseLedge = false;
					if (ledgeJumpIntentActive)
					{
						// If this is true, they're holding down the key that tells the game they
						// want to go to a higher ledge.
						RaycastHit hitInfo;
						if (Physics.Raycast(new Ray(myTransform.position, Vector3.up), out hitInfo, 100, 1 << (int)GameDirector.GameLayers.Ledge))
						{
							// We found a ledge. Lets get on it.
							targetLedge = hitInfo.collider;
							verticalSpeed = 26;
							traverseLedge = true;
						}
					}
					else if (crouchActive)
					{
						// If this is true, they're holding down the key that tells the game they
						// want to go to a lower ledge
						RaycastHit hitInfo;
						if (Physics.Raycast(new Ray(myTransform.position, -Vector3.up), out hitInfo, 100, 1 << (int)GameDirector.GameLayers.Ledge))
						{
							// We found a ledge. Lets go below it.
							fallThroughLedge = hitInfo.collider;
							traverseLedge = true;
						}					
					}
					
					if (!traverseLedge)
					{
						verticalSpeed = 16;
					}
					
					isJumping = true;
				}
				
		    	// Calculate the true vertical character speed based on gravity and jumping.
				if (IsCharacterGrounded)
				{
					verticalSpeed = 0;
					isJumping = false;
				}
				else
				{
					verticalSpeed += -1.0f;
				}				
			}
			// Handle movement where the player can just walk all over the screen and there is no sense of up
			else if (MovementMode.FullFreedom == movementMode)
			{
				if (walkUpActive)
				{
					verticalSpeed = verticalWalkingSpeed;
				}
				else if (walkDownActive)
				{
					verticalSpeed = -verticalWalkingSpeed;
				}
				else
				{
					verticalSpeed = 0;
				}				
			}
	
			// Move the character
	    	collisionFlags = myCharacterController.Move(new Vector3(horizontalSpeed, verticalSpeed, 0) * Time.deltaTime);
			
			// Handle post-movement character processing where the mode involves moving left, right, and jumping
			if (MovementMode.LeftRightJump == movementMode)
			{			
				// If the player is jumping onto a ledge, make sure collision detection works when the 
				// player is going back down to earth
				if (null != targetLedge)
				{
					if (myTransform.position.y - 0.75f > targetLedge.transform.position.y + targetLedge.transform.localRotation.y)
					{
						Physics.IgnoreCollision(myCollider, targetLedge.collider, false);
						targetLedge = null;
					}
					else
					{
						Physics.IgnoreCollision(myCollider, targetLedge.collider, true);
					}
				}
				
				// If the player is falling through a ledge, make sure the collision detection works when
				// the player is fully under it
				if (null != fallThroughLedge)
				{
					if (myTransform.position.y + 0.75f < fallThroughLedge.transform.position.y + fallThroughLedge.transform.localRotation.y)
					{
						Physics.IgnoreCollision(myCollider, fallThroughLedge.collider, false);
						fallThroughLedge = null;
					}
					else
					{
						Physics.IgnoreCollision(myCollider, fallThroughLedge.collider, true);
					}				
				}
			}
		}
		
		// Update dying animation (all players run this code)
		if (IsDying)
		{
			float ttl = 1.0f - (Time.time - dyingStartTime) * dyingSpeed;
			bool tryToUnspawn = false;
			if (ttl < 0) {
				ttl = 0; 
				tryToUnspawn = true;
			}
			Color c;
			if (null != characterTextureAnimator)
			{
				c = new Color(
					characterTextureAnimator.mainColor.r,
					characterTextureAnimator.mainColor.g,
					characterTextureAnimator.mainColor.b,
					ttl);
				characterTextureAnimator.mainColor = c;
			}
			else
			{
				c = new Color(
					renderer.material.color.r,
					renderer.material.color.g,
					renderer.material.color.b,ttl);
				renderer.material.color = c;
			}
			
			// Try to destroy the object if we're the owner
			if (tryToUnspawn && networkView.isMine)
			{
				DestroyThisCharacter();
			}
		}
		
		// Update the material color every frame (we could be flashing)
		if (null != characterTextureAnimator)
		{
			primaryArm.renderer.material.color = new Color(0,0,0,characterTextureAnimator.mainColor.a);
			primaryArm.renderer.material.SetColor("_Emission", characterTextureAnimator.mainColor);
		}
		else
		{
			primaryArm.renderer.material.color = renderer.material.color;
			primaryArm.renderer.material.SetColor("_Emission", renderer.material.GetColor("_Emission"));
		}
	}
	
	protected virtual void FixedUpdate()
	{
		// Restrict the z position
		myTransform.position = new Vector3(myTransform.position.x, myTransform.position.y, 0);

		if (networkView.isMine)
		{			
			// Now update the player posture
			CurrentPosture = CalculatePostureFromPlayerMovement();
		}		
		else
		{
			// We'll get the posture from serialization
		}
		
		// Preserve the previous position
		prevPos = myTransform.position;							
	}
	
	#endregion
	
	#region Character Functions

	/// <summary>
	/// Called by the owning client to calculate the posture from player movement.
	/// </summary>
	/// <returns>
	/// The posture from player movement.
	/// </returns>
	protected virtual Posture CalculatePostureFromPlayerMovement()
	{
		// Assign posture based on movements
		Posture newPosture = CurrentPosture;
		
		if (ScreenVelocity.x < -0.0001f)
		{
			facingDirection = FacingDirection.Left;
		}
		else if (ScreenVelocity.x > 0.0001f)
		{
			facingDirection = FacingDirection.Right;
		}
		
		if (Posture.Dying == newPosture)
		{
			// Just continue visibly dying
		}
		// If the player is in mid-air or crouching, put them in the crouching mode
		else if (ScreenVelocity.y > 0.05f || ScreenVelocity.y < -0.05f || !IsVisiblyGrounded) 
		{
			newPosture = (FacingDirection.Left == facingDirection) ? Posture.CrouchingLeft : Posture.CrouchingRight;
		}
		// If the player is not in mid-air and moving left, they must be walking left
		else if (ScreenVelocity.x < -0.0001f)
		{
			newPosture = crouchActive ? Posture.CrawlingLeft : Posture.WalkingLeft;
		}
		// If the player is not in mid-air and moving right, they must be walking right
		else if (ScreenVelocity.x > 0.0001f)
		{
			newPosture = crouchActive ? Posture.CrawlingRight : Posture.WalkingRight;
		}
		else if (crouchActive)
		{
			newPosture = (FacingDirection.Left == facingDirection) ? Posture.CrouchingLeft : Posture.CrouchingRight;
		}
		// Not moving if no x velocity
		else
		{
			newPosture = Posture.Standing;
		}		
		return newPosture;
	}
	
	/// <summary>
	/// Called for all players to set the active weapon.
	/// </summary>
	/// <param name='prefab'>
	/// The name of the weapon prefab in the Resources folder
	/// </param>
	public virtual void SetActiveWeapon(string prefab)
	{
		if (null != activeWeaponObject)
		{
			// Destroy the active weapon
			activeWeapon.Teardown();
			Destroy(activeWeaponObject);
			activeWeapon = null;
			activeWeaponObject = null;
		}

		// Now give the player the new one
		if (!string.IsNullOrEmpty(prefab))
		{
			GameObject newWeapon = (GameObject)Resources.Load(prefab);
			activeWeaponObject = (GameObject)GameObject.Instantiate(newWeapon
				,primaryArm.position
				,Quaternion.identity);
			activeWeaponObject.name = prefab;
			UtilitiesDirector.SetLayerRecurse(activeWeaponObject.transform, gameObject.layer);
			activeWeaponObject.transform.parent = primaryArm;
			activeWeaponObject.transform.localEulerAngles = Vector3.zero;
			activeWeaponObject.transform.localScale = new Vector3(
				newWeapon.transform.localScale.x / primaryArm.lossyScale.x
				,newWeapon.transform.localScale.y / primaryArm.lossyScale.y
				,newWeapon.transform.localScale.z / primaryArm.lossyScale.z);
			PrimaryArmPivot = 0;
			
			// Set up the new weapon and assign it to the character
			Weapon weapon = activeWeaponObject.GetComponent<Weapon>();
			activeWeapon = (IWeapon)weapon;
			activeWeapon.Setup(this);
		}
	}
	
	/// <summary>
	/// Called for all players to set the active hat.
	/// </summary>
	/// <param name='prefab'>
	/// The name of the hat prefab in the Resources folder
	/// </param>
	protected void SetActiveHat(string prefab)
	{
		if (null != hatRenderer)
		{
			// Get rid of the old hat
			if (hatRenderer.enabled)
			{		
				hatRenderer.enabled = false;
			}
			
			// Now give the player the new one
			if (!string.IsNullOrEmpty(prefab))
			{
				GameObject hatPrefab = (GameObject)Resources.Load(prefab);
				hatRenderer.name = prefab;
				normalHatPosition = hatRenderer.transform.localPosition = hatPrefab.transform.localPosition + accessoryPositionOffset;
				hatRenderer.transform.localRotation = hatPrefab.transform.localRotation;
				hatRenderer.transform.localScale = hatPrefab.transform.localScale;
				hatRenderer.material = hatPrefab.renderer.sharedMaterial;
				hatRenderer.material.SetColor("_Emission", characterColor);
				hatRenderer.enabled = true;
			}
			
			// If the player is crouching then we need to ensure the accessory is in the right place
			UpdatePosture();
		}
		else
		{
			// No hat renderer; probably an NPC
		}
	}
	
	/// <summary>
	/// Called for all players to set the active mouthpiece.
	/// </summary>
	/// <param name='prefab'>
	/// The name of the mouthpiece prefab in the Resources folder
	/// </param>	
	protected void SetActiveMouthpiece(string prefab)
	{
		if (null != mouthpieceRenderer)
			{
			// Get rid of the old mouthpiece
			if (mouthpieceRenderer.enabled)
			{
				mouthpieceRenderer.enabled = false;
				// Destroy any child objects
				foreach (Transform c in mouthpieceRenderer.transform)
				{
					Destroy(c.gameObject);
				}
			}
			
			// Now give the player the new one
			if (!string.IsNullOrEmpty(prefab))
			{
				GameObject mouthpiecePrefab = (GameObject)Resources.Load(prefab);
				mouthpieceRenderer.name = prefab;
				normalMouthpiecePosition = mouthpieceRenderer.transform.localPosition = mouthpiecePrefab.transform.localPosition + accessoryPositionOffset;
				mouthpieceRenderer.transform.localRotation = mouthpiecePrefab.transform.localRotation;
				mouthpieceRenderer.transform.localScale = mouthpiecePrefab.transform.localScale;
				mouthpieceRenderer.material = mouthpiecePrefab.renderer.sharedMaterial;
				mouthpieceRenderer.material.SetColor("_Emission", characterColor);
				mouthpieceRenderer.enabled = true;
				// Add any child objects
				foreach (Transform c in mouthpiecePrefab.transform)
				{
					GameObject o = (GameObject)GameObject.Instantiate(c.gameObject);
					o.transform.parent = mouthpieceRenderer.transform;
					o.transform.localPosition = c.localPosition;
					o.transform.localRotation = c.localRotation;
					o.transform.localScale = c.localScale;
					
				}			
			}
			
			// If the player is crouching then we need to ensure the accessory is in the right place
			UpdatePosture();
		}
		else
		{
			// No mouthpiece; probably an NPC
		}
	}
	
	/// <summary>
	/// Called for all players to begin the visible dying process
	/// </summary>
	protected virtual void BeginDying()
	{
		// Play a groan
		if (sndGroans.Length > 0)
		{
			float pitch;
			if (GetType() == typeof(PlayerCharacter))
			{
				pitch = 0.8f;
			}
			else
			{
				pitch = Random.Range(1.0f, 1.3f);
			}
			PlaySound(sndGroans[Random.Range(0,sndGroans.Length)], 0.5f, pitch);
		}
		// Take away their weapon
		SetActiveWeapon(null);
		// Take away their collider
		Destroy(myCharacterController);
		// Update their posture
		CurrentPosture = Posture.Dying;
		// Reset the walking speed
		walkingSpeed = 0;
		// Set the dying start time
		dyingStartTime = Time.time;		
	}

	/// <summary>
	/// Called by the owning client to destroy this character
	/// </summary>
	protected virtual void DestroyThisCharacter()
	{
		// Just destroy the object
		if (networkView.isMine)
		{
			NetworkDirector.Destroy(gameObject);
		}
		else
		{
			Debug.LogError("Tried to destroy a character that wasn't ours!");
		}
	}	
	
	/// <summary>
	/// Plays a sound.
	/// </summary>
	/// <param name='audioClip'>
	/// Audio clip.
	/// </param>
	public virtual void PlaySound(AudioClip audioClip)
	{
		PlaySound(audioClip, 1.0f, 1.0f);
	}
	
	/// <summary>
	/// Plays a sound.
	/// </summary>
	/// <param name='audioClip'>
	/// Audio clip.
	/// </param>
	/// <param name='volumeMultiplier'>
	/// Volume multiplier.
	/// </param>
	/// <param name='pitch'>
	/// Pitch.
	/// </param>
	public virtual void PlaySound(AudioClip audioClip, float volumeMultiplier, float pitch)
	{
		audioSource.Stop();
		audioSource.volume = AudioDirector.SFXVolume * volumeMultiplier;
		audioSource.pitch = pitch;
		audioSource.PlayOneShot(audioClip);
	}
	
	#endregion
	
	#region Custom Networking
	
    protected override void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
		base.OnSerializeNetworkView(stream, info);
        if (stream.isWriting)
        {
            // We own this player: send the others our data
			Vector3 v = primaryArmPivot.localPosition;
			stream.Serialize(ref v);
			Quaternion q = primaryArmPivot.localRotation;
			stream.Serialize(ref q);
			int data = (int)CurrentPosture;
			stream.Serialize(ref data);
			data = (int)facingDirection;
			stream.Serialize(ref data);
			
			// We can't serialize strings so use enumerations			
			CharacterWeaponType characterWeaponType = string.IsNullOrEmpty(ActiveWeaponName) ? CharacterWeaponType.None : (CharacterWeaponType)System.Enum.Parse(typeof(CharacterWeaponType), ActiveWeaponName);
			int typeInt = (int)characterWeaponType;
			stream.Serialize(ref typeInt);
			
			CharacterHatType characterHatType = string.IsNullOrEmpty(ActiveHatName) ? CharacterHatType.None : (CharacterHatType)System.Enum.Parse(typeof(CharacterHatType), ActiveHatName);
			typeInt = (int)characterHatType;
			stream.Serialize(ref typeInt);

			CharacterMouthpieceType characterMouthpieceType = string.IsNullOrEmpty(ActiveMouthpieceName) ? CharacterMouthpieceType.None : (CharacterMouthpieceType)System.Enum.Parse(typeof(CharacterMouthpieceType), ActiveMouthpieceName);
			typeInt = (int)characterMouthpieceType;
			stream.Serialize(ref typeInt);
			
			// Color
			Color c = characterColor;
			stream.Serialize(ref c.r);
			stream.Serialize(ref c.g);
			stream.Serialize(ref c.b);
			stream.Serialize(ref c.a);
        }
        else
        {
            // Network player, receive data
			Vector3 v = primaryArmPivot.localPosition;
			stream.Serialize(ref v);
			primaryArmPivot.localPosition = v;
			stream.Serialize(ref correctPrimaryArmPivot);
			int data = (int)this.CurrentPosture;
			stream.Serialize(ref data);
			this.CurrentPosture = (Posture)data;
			data = (int)this.facingDirection;
			stream.Serialize(ref data);
			this.facingDirection = (FacingDirection)data;
			
			// We can't serialize strings so use enumerations
			int typeInt = 0;
			
			stream.Serialize(ref typeInt);
			CharacterWeaponType characterWeaponType = (CharacterWeaponType)typeInt;
			if (CharacterWeaponType.None == characterWeaponType)
			{
				if (!string.IsNullOrEmpty(ActiveWeaponName))
				{
					SetActiveWeapon(null);
				}				
			}
			else if (ActiveWeaponName != characterWeaponType.ToString())
			{
				SetActiveWeapon(characterWeaponType.ToString());
			}
			
			stream.Serialize(ref typeInt);
			CharacterHatType characterHatType = (CharacterHatType)typeInt;
			if (CharacterHatType.None == characterHatType)
			{
				if (!string.IsNullOrEmpty(ActiveHatName))
				{
					SetActiveHat(null);
				}
			}
			else if (ActiveHatName != characterHatType.ToString())
			{
				SetActiveHat(characterHatType.ToString());
			}
			
			stream.Serialize(ref typeInt);
			CharacterMouthpieceType characterMouthpieceType = (CharacterMouthpieceType)typeInt;
			if (CharacterMouthpieceType.None == characterMouthpieceType)
			{
				if (!string.IsNullOrEmpty(ActiveMouthpieceName))
				{
					SetActiveMouthpiece(null);
				}
			}
			else if (ActiveMouthpieceName != characterMouthpieceType.ToString())
			{
				SetActiveMouthpiece(characterMouthpieceType.ToString());
			}
	
			// Color
			Color c = Color.black;
			stream.Serialize(ref c.r);
			stream.Serialize(ref c.g);
			stream.Serialize(ref c.b);
			stream.Serialize(ref c.a);			
			if (characterColor != c)
			{
				characterColor = c;
				characterTextureAnimator.mainColor = characterColor;
				primaryArm.renderer.material.SetColor("_Emission", characterColor);				
			}
        }
    }
	
	#endregion
}
