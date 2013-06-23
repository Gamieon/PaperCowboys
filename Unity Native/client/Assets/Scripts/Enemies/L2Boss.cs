using UnityEngine;
using System.Collections;

public sealed class L2Boss : EnemyBoss, IDamageTaker
{
	/// <summary>
	/// The player being pursued.
	/// </summary>		
	PlayerCharacter playerBeingPursued;
			
	/// <summary>
	/// This function is called in a timer by all clients to try to make this enemy throw
	/// dynamite. It must be run for all players because the master client can change.
	/// </summary>		
	void ThrowDynamite()
	{
		if (NetworkDirector.isMasterClient && !IsDying)
		{
			// The active weapon is a simple pistol, so just pull the trigger
			activeWeapon.BeginFiringPrimary(WeaponFiringDirection.LocalAxis);
			activeWeapon.EndFiringPrimary();
		}
	}	
		
	#region MonoBehavior
	
	protected override void Start() 
	{
		base.Start();
		// Assign boss color
		Color bossColor = new Color(0.25f,0.2f,0.2f,1);
		characterTextureAnimator.mainColor = bossColor;
		primaryArm.renderer.material.SetColor("_Emission", bossColor);
		// Give them a hat
		SetActiveHat("CowboyHat");
		hatRenderer.material.SetColor("_Emission", bossColor);
		// Aim to the left
		PrimaryArmPivot = 180.0f;
		facingDirection = FacingDirection.Left;
		// Pursue the player
		InvokeRepeating("ThrowDynamite", 1.0f, 2);
	}
	
	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		
		// If we're the master client, we must update which player we're pursuing
		if (NetworkDirector.isMasterClient && !IsDying)
		{
			// Update which player we're pursuing
			playerBeingPursued = GetClosestPlayer();			
			if (null != playerBeingPursued)
			{
				float angle = Vector3.Angle(Vector3.right, playerBeingPursued.transform.position - myTransform.position);
				if (playerBeingPursued.transform.position.y < myTransform.position.y) { angle = 360.0f - angle; }
				PrimaryArmPivot = Mathf.Lerp(PrimaryArmPivot, angle, Time.deltaTime * 5);
			}
		}
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
	/// <param name='owningPlayer'>
	/// The player who owns the object that dealt the damage, or null if it was an enemy.
	/// </param>
	[RPC]
	public void RPCBoss2TakeDamage(float damage, NetworkPlayer owningPlayer)
	{
		// Flash the thug white
		characterTextureFlasher.Flash(0.04f);
		
		// Deduct the hitpoint count
		hitPoints -= damage;
		
		// Handling for the owner
		if (NetworkDirector.isMasterClient)
		{	
			// If the character is doomed, then begin its VISUAL death sequence. Other players
			// will discover this with the next serialization; and you, as the master client, 
			// will eventually destroy the character entirely.
			if (hitPoints - damage < MinHitpointsBeforeFrag)
			{
				GameDirector gameDirector = (GameDirector)Object.FindObjectOfType(typeof(GameDirector));
				// Increase the owning player's score
				int currentScore = (int)NetworkDirector.GetCustomPlayerProperty(owningPlayer, "Player.ActiveSession.Score");
				if (Network.player == owningPlayer)
				{
					// Don't send RPC's to ourselves. Just call the function outright.
					gameDirector.RPCSetScore(currentScore + pointValue);
				}
				else
				{
					gameDirector.networkView.RPC("RPCSetScore", owningPlayer, currentScore + pointValue);
				}
				
				// Tell everybody that this character is dying
				networkView.RPC("RPCBoss2BeginDying", RPCMode.All);
				
				// Tell everybody that the boss has been defeated. Do it buffered in case someone
				// jumps in during the victory sequence.
				gameDirector.networkView.RPC("RPCBossDefeated", RPCMode.AllBuffered);		
			}
		}
	}
	
	/// <summary>
	/// Invoked for everybody but the master client (who already knows this character is dying) to
	/// inform them the character is dying.
	/// </summary>
	[RPC]
	public void RPCBoss2BeginDying()
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
			// Tell all players that this character is taking damage. Since we only support players damaging bosses, GetOwningPlayer should never be null.
			networkView.RPC("RPCBoss2TakeDamage", RPCMode.All, damage, damageDealer.GetOwningPlayer() );
			// If we clearly doomed the character, begin its VISIBLE death sequence here. The master
			// client will eventually destroy it from the game.
			if (hitPoints - damage < MinHitpointsBeforeFrag)
			{
				BeginDying(); // This will flag the character as dying
			}
		}
		else
		{
			// Ignore if the character is already dying
		}
	}
	
	#endregion		
}
