using UnityEngine;
using System.Collections;

/// <summary>
/// The level 1 boss
/// </summary>
public sealed class L1Boss : EnemyBoss, IDamageTaker
{
	/// <summary>
	/// The player being pursued.
	/// </summary>	
	PlayerCharacter playerBeingPursued;
		
	/// <summary>
	/// The sound effect played when this object fires
	/// </summary>
	public AudioClip sndFire;
	
	/// <summary>
	/// This function is called in a timer by all clients to try to make this enemy play
	/// firing sounds.
	/// </summary>
	void PlayFireSound()
	{
		PlaySound(sndFire, 0.7f, 1f);
	}
		
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
		
	#region MonoBehavior
	
	protected override void Start() 
	{
		base.Start();
		// Assign boss color
		Color bossColor = new Color(0.25f,0.25f,0.25f,1);
		characterTextureAnimator.mainColor = bossColor;
		primaryArm.renderer.material.SetColor("_Emission", bossColor);
		// Aim to the left
		PrimaryArmPivot = 180.0f;
		// Pursue the player
		InvokeRepeating("PlayFireSound", 1.0f, 2);
		InvokeRepeating("FireGun", 1.0f, 2);
		InvokeRepeating("FireGun", 1.1f, 2);
		InvokeRepeating("FireGun", 1.2f, 2);
		InvokeRepeating("FireGun", 1.3f, 2);
		InvokeRepeating("FireGun", 1.4f, 2);		
	}
	
	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		
		// If we're the master client, we must update which player we're pursuing
		if (PhotonNetwork.isMasterClient && !IsDying)
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
	public void RPCBoss1TakeDamage(float damage, PhotonPlayer owningPlayer)
	{
		// Flash the thug white
		characterTextureFlasher.Flash(0.04f);

		// Deduct the hitpoint count
		hitPoints -= damage;
		
		// Handling for the owner (the master client)
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
				
				// Tell everybody that this character is dying
				photonView.RPC("RPCBoss1BeginDying", PhotonTargets.All);
				
				// Tell everybody that the boss has been defeated. Do it buffered in case someone
				// jumps in during the victory sequence.
				gameDirector.photonView.RPC("RPCBossDefeated", PhotonTargets.AllBuffered);
			}
		}
	}
	
	/// <summary>
	/// Invoked for everybody but the master client (who already knows this character is dying) to
	/// inform them the character is dying.
	/// </summary>
	[RPC]
	public void RPCBoss1BeginDying()
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
			photonView.RPC("RPCBoss1TakeDamage", PhotonTargets.All, damage, (damageDealer.IsOwnedByPlayer() ? damageDealer.GetOwningPlayer() : null) );
		}
		else
		{
			// Ignore if the character is already dying
		}
	}
	
	#endregion		
}
