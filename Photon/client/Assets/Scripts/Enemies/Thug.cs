using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// A thug is the most basic enemy character, and is always owned by the master client.
/// </summary>
public sealed class Thug : EnemyCharacter, IDamageTaker
{
	public enum ThugType
	{
		Standing,
		Crouching,
		Walking,
		Crawling,
		Knifing,
	};
	
	/// <summary>
	/// The type of the thug.
	/// </summary>
	public ThugType thugType;
	
	/// <summary>
	/// The player being pursued.
	/// </summary>
	PlayerCharacter playerBeingPursued;
	
	#region MonoBehavior
	
	protected override void Start()
	{
		base.Start();
		
		// Always face the left initially
		facingDirection = Character.FacingDirection.Left;
		
		// Set up the character
		switch (thugType)
		{
		case ThugType.Crouching:
			crouchActive = true;
			break;
		case ThugType.Standing:
			break;
		case ThugType.Walking:
			UpdateWalkingDirection();
			break;
		case ThugType.Crawling:
			crouchActive = true;
			UpdateWalkingDirection();
			break;
		case ThugType.Knifing:
			crouchActive = true;
			UpdateWalkingDirection();
			break;
		}
		
		// Aim to the left by default
		PrimaryArmPivot = 180.0f;
		
		// Fire the gun in random intervals
		InvokeRepeating("FireGun", 1.0f, 1.5f + Random.value * 2.0f);
	}
	
	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		
		// If we're the master client, we must update which player we're pursuing
		if (PhotonNetwork.isMasterClient && !IsDying)
		{
			
			List<PlayerCharacter> playerCharacters = PlayerCharacter.AllPlayerCharacters;
			float? dNearest = null;
			playerBeingPursued = null;
			
			foreach (PlayerCharacter pc in playerCharacters)
			{
				float d = Vector3.SqrMagnitude( myTransform.position - pc.transform.position );
				if (!dNearest.HasValue || d < dNearest.Value)
				{
					dNearest = d;
					playerBeingPursued = pc;
				}
			}
			
			if (null != playerBeingPursued)
			{
				float angle = Vector3.Angle(Vector3.right, playerBeingPursued.transform.position - myTransform.position);
				if (playerBeingPursued.transform.position.y < myTransform.position.y) { angle = 360.0f - angle; }
				PrimaryArmPivot = Mathf.Lerp(PrimaryArmPivot, angle, Time.deltaTime * 5);
			}
		}
	} 
		
	#endregion
	
	/// <summary>
	/// This function is called in a timer by all clients to try to make this enemy fire.
	/// It must be run for all players because the master client can change.
	/// </summary>
	void FireGun()
	{
		if (PhotonNetwork.isMasterClient && !IsDying)
		{
			// The active weapon is a simple pistol, so just pull the trigger
			activeWeapon.BeginFiringPrimary(WeaponFiringDirection.LocalAxis);
			activeWeapon.EndFiringPrimary();
		}
	}	
	
	/// <summary>
	/// This function is called in regular intervals by all clients to make the thug
	/// decide where to walk around if it can.
	/// </summary>
	void UpdateWalkingDirection()
	{
		if (PhotonNetwork.isMasterClient && !IsDying)
		{
			PlayerCharacter pc = GetClosestPlayer();
			if (null != pc)
			{
				if (pc.transform.position.x < myTransform.position.x) {
					walkLeftActive = true;
					walkRightActive = false;
				}
				else {
					walkLeftActive = false;
					walkRightActive = true;
				}
			}
		}
		Invoke("UpdateWalkingDirection", 1.0f);
	}
	
	#region Character functions (RPCs)
		
	/// <summary>
	/// Sent from the master client observing something damaging this object to all clients
	/// to inform them that the object is being damaged.
	/// </summary>
	/// <param name='damage'>
	/// Damage.
	/// </param>
	/// <param name='owningPlayer'>
	/// The player who owns the object that dealt the damage, or null if it was an enemy.
	/// </param>
	[RPC]
	public void RPCThugTakeDamage(float damage, PhotonPlayer owningPlayer)
	{
		// Flash the thug white
		characterTextureFlasher.Flash(0.04f);
	
		// Deduct the hitpoint count
		hitPoints -= damage;
	
		// Handling for the master client
		if (PhotonNetwork.isMasterClient)
		{	
			// If the character is doomed, then begin its VISUAL death sequence. Other players
			// will discover this with the next serialization; and you, as the master client, 
			// will eventually destroy the character entirely.
			if (hitPoints - damage < MinHitpointsBeforeFrag)
			{
				GameDirector gameDirector = (GameDirector)Object.FindObjectOfType(typeof(GameDirector));
				// Do stuff for the owning player
				if (null != owningPlayer)
				{
					// Increase their score
					int currentScore = (int)owningPlayer.customProperties["Player.ActiveSession.Score"];		
					gameDirector.photonView.RPC("RPCSetScore", owningPlayer, currentScore + pointValue);
				}
				// Give everyone a 1 in 3 chance for a weapon drop
				if (Random.Range(0.0f, 1.0f) < 0.33f)
				{
					// TODO: Decide which players get which drops
					gameDirector.photonView.RPC("RPCGiveDrop", PhotonTargets.All, myTransform.position);					
				}
				
				// Tell everybody that this character is dying
				photonView.RPC("RPCThugBeginDying", PhotonTargets.All);
			}
		}
	}
	
	/// <summary>
	/// Invoked for everybody but the master client (who already knows this character is dying) to
	/// inform them the character is dying.
	/// </summary>
	[RPC]
	public void RPCThugBeginDying()
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
			float damage = damageDealer.GetDamage();
			// Tell the master client that this character is taking damage
			photonView.RPC("RPCThugTakeDamage", PhotonTargets.All, damage, (damageDealer.IsOwnedByPlayer() ? damageDealer.GetOwningPlayer() : null) );
		}
		else
		{
			// Ignore if the character is already dying
		}
	}
	
	#endregion	
}
