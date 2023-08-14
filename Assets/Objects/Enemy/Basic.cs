using System.Collections;
using System.Collections.Generic;
using static System.Math;
using UnityEngine;
using UnityEngine.AI;

/* NOTE(Roskuski):
 * Enemy AI Directive: What this enemy wants to do.
 * Enemy AI Personality: Determines how this enemy chooses what to do.
 * Enemy AI Wants: actions this AI wants to do when given the oppertunity
 */

public class Basic : MonoBehaviour {
	static bool animationTimesPopulated = false;
	static Dictionary<string,float> animationTimes; // @TODO(Roskuski) Remove this in favor getting this information from the animator
	// NOTE(Roskuski): Enemy ai state

	const float TraitMax = 1000.0f;

	class ChoiceEntry {
		public float baseWeight;
		public float[] traitMod = new float[Util.EnumLength(typeof(TraitKind))];
		public ChoiceEntry(float baseWeight, float[] traitMod) {
			this.baseWeight = baseWeight;
			this.traitMod = traitMod;
		}
	}

	enum TraitKind {
		Aggressive = 0, // NOTE(Roskuski): How aggsive this enemy will behave. Values below TraitMax will act Defensively!
		Sneaky,
		// @TODO(Rosksuki): Pacience trait, dynamic stat: repersents how badly this enemy wants to play the game
	}
	public float[] traits = new float[Util.EnumLength(typeof(TraitKind))];

	// NOTE(Roskuski): This should stay in sync with the animation controller. DO NOT ADD ELEMENTS IN THE MIDDLE OF THE ENUM
	enum Directive {
		// Do nothing, intentionally
		Inactive = 0,

		// Maintain a certain distance from the player, perhaps with a certain offset
		MaintainDistancePlayer,

		// Attack the player!
		PerformAttack,

		// Rise my child!
		Spawn,

		// standing there... menacingly...
		Sandbag,

		// Reeling back from truma
		Stunned,
	}
	[SerializeField] Directive directive;

	// NOTE(Roskuski): This should stay in sync with the animation controller. DO NOT ADD ELEMENTS IN THE MIDDLE OF THE ENUM
	public enum Attack : int {
		None = 0,
		Slash,
		Lunge,
	}
	public Attack currentAttack = Attack.None; // NOTE(Roskuski): Do not set this manually, use the setting function, as that keeps animation state insync

	public bool randomizeStats = true;
	float inactiveWait = 2;
	float approchDistance;
	Vector3 targetOffset;
	bool preferRightStrafe;
	[SerializeField] float flankStrength = 0;

	float stunDuration;

	// NOTE(Roskuski): because data dopes aren't consistant, we need to keep track of this on the script side.
	// I'm going to hope that this stays more or less in sync with the actual animation state
	float animationTimer = 0;
	public float choiceTimer = 3.0f; // @TODO(Roskuski) Fine tune this parameter
	public float attackCooldown;
	bool wantsSlash = false;
	public float enemyCommunicationRange;
	Transform flashSpot; //the place where the red circle will pop up

	float spawnUpwardsSpeed;
	float spawnDownwardsSpeed;
	float spawnLateralSpeed = 5.0f;

	Quaternion moveDirection = Quaternion.identity;
	Vector3 movementDelta = Vector3.zero;

	float[] directionWeights = new float[32];

	// NOTE(Roskuski): End of ai state

	public float health;
	bool wasHitByChop = false;
	public bool shouldDie = false;
	public float dropChance; //chance to drop a head (0-100)
	public bool shouldAddToKillTotal = true;
	public bool isHardMode;

	KnockbackInfo knockbackInfo;
	float remainingKnockbackTime = 0f;
	[SerializeField] float hitflashTimer = 0f;
	public float debugTargetTimer = 0f;
	bool isInHeavyStun = false;
	bool hasStartedStunRecovery = false;

	[SerializeField] bool isSandbag = false;
	bool isImmune = false;

	private float LungeSpeed = 15f;
	// NOTE(Roskuski): copied from the default settings of navMeshAgent
	private float MoveSpeed = 7f;
	private float TurnSpeed = 360.0f; // NOTE(Roskuski): in Degrees per second

	private float TightApprochDistance = 4;
	private float CloseApprochDistance = 6;
	private float LooseApprochDistance = 10;
	public const float ApprochDeviance = 2;

	// NOTE(Roskuski): Internal references
	NavMeshAgent navAgent;
	Animator animator;
	BoxCollider swordHitbox;
	Transform smokePoint;
	EnemyCommunication enemyCommunication;
	MotionAudio_Skel sounds;

	SkinnedMeshRenderer model;
	Material[] materials;
	public Material hitflashMat;
	public Material debugTargetMat;
	public MeshRenderer[] armorMesh;
	public Transform armorPoint;
	public bool isArmored;
	public bool debugHoming;
	float immunityTime = 0f;
	public GameObject stunSphere;

	// NOTE(Roskuski): External references
	GameManager gameMan;
	public bool isCrystallized = false;


	void CheckForHardMode() {
		if (isHardMode) {
			LungeSpeed = 20f;
			MoveSpeed = 10f;
			TightApprochDistance = 1f;
			CloseApprochDistance = 4f;
			LooseApprochDistance = 8f;
			animator.SetFloat("hardSpeed", 1.4f);
        }
    }

	// NOTE(Roskuski): abs(traitMods) array should sum to 1
	// @TODO(Roskuski): Mathematically prove that this true with negtive numbers
	int RollTraitChoice(ChoiceEntry[] choices) {
		int result = -1;
		float[] finalWeight = new float[choices.Length];
		float finalTotal = 0;
		for (int index = 0; index < finalWeight.Length; index += 1) {
			finalWeight[index] += choices[index].baseWeight;
			float modTotal = 0;
			for (int traitIndex = 0; traitIndex < System.Enum.GetNames(typeof(TraitKind)).Length; traitIndex += 1) {
				finalWeight[index] += ((traits[traitIndex])/1000.0f) * choices[index].traitMod[traitIndex] * choices[index].baseWeight;
				modTotal += choices[index].traitMod[traitIndex];
			}
			finalTotal += finalWeight[index];
			Debug.Assert(finalWeight[index] >= 0);
		}
		
		float randomRoll = Random.Range(0, finalTotal);
		float rollingTotal = 0;
		for (int index = 0; index < finalWeight.Length; index += 1) {
			rollingTotal += finalWeight[index];
			if (randomRoll < rollingTotal) {
				result = index;
				break;
			}
		}

		return result;
	}

	public void RollArmorChance() {
		float armorChance = GameManager.armoredEnemyChance;
		float randomChance = Random.Range(0, 99);
		if (randomChance < armorChance) isArmored = true;
    }

	bool CanAttemptNavigation() {
		return (navAgent.pathStatus == NavMeshPathStatus.PathComplete || navAgent.pathStatus == NavMeshPathStatus.PathPartial) && navAgent.path.corners.Length >= 2;
	}

	float DistanceToTravel() {
		return navAgent.remainingDistance - approchDistance;
	}


	public void ChangeDirective_Stunned(StunTime stunTime, KnockbackInfo newKnockbackInfo, bool extraStun) {
		if (directive != Directive.Spawn && stunTime != StunTime.None) {
			directive = Directive.Stunned;
			hasStartedStunRecovery = false;
			isInHeavyStun = false;
			
			float stunValue = 0;
			switch (stunTime) {
				case StunTime.Short:
					stunValue = 0.5f;
					animator.SetTrigger("wasHurt");
					break;

				case StunTime.Long:
					stunValue = Random.Range(2f, 2.4f);
					if (extraStun) stunValue += 2f;
					animator.SetTrigger("wasHeavyHurt");
					isInHeavyStun = true;
					break;

				case StunTime.None:
				default:
					Debug.Assert(false);
					break;
			}

			stunDuration = stunValue;
			hitflashTimer = 0.25f;

			swordHitbox.enabled = false;

			knockbackInfo = newKnockbackInfo;
			remainingKnockbackTime = knockbackInfo.time;
			this.transform.rotation = Quaternion.AngleAxis(180, Vector3.up) * knockbackInfo.direction;
		}
	}

	void ChangeDirective_Inactive(float inactiveWait) {
		directive = Directive.Inactive;
		this.inactiveWait = inactiveWait;
		currentAttack = Attack.None;
		isInHeavyStun = false;
		hasStartedStunRecovery = false;

		animator.SetInteger("CurrentAttack", (int)Attack.None);
		swordHitbox.enabled = false;
	}

	void ChangeDirective_MaintainDistancePlayer(float stoppingDistance, Vector3 targetOffset = default(Vector3)) {
		directive = Directive.MaintainDistancePlayer;
		approchDistance = stoppingDistance + Random.Range(0, ApprochDeviance);
		this.targetOffset = targetOffset;
		swordHitbox.enabled = false;
	}

	// Probably incorrect to try and go to this state manually
	void ChangeDirective_Spawn() {
		Debug.Assert(false);
	}

	void ChangeDirective_PerformAttack(Attack attack) {
		directive = Directive.PerformAttack;
		currentAttack = attack;
		animator.SetBool("wantsSlash", false);
		animator.SetInteger("CurrentAttack", (int)currentAttack);
		switch (attack) {
			default:
			case Attack.None:
				Debug.Assert(false);
				break;

			case Attack.Slash:
				Util.SpawnFlash(gameMan, 0, flashSpot.position, true);
				animationTimer = animationTimes["Enemy_Attack_Slash"];
				break;

			case Attack.Lunge:
				Util.SpawnFlash(gameMan, 0, flashSpot.position, true);
				animationTimer = animationTimes["Enemy_Attack_Dash"];
				break;
		}
		Collider[] nearEnemies = Physics.OverlapSphere(transform.position, enemyCommunicationRange, Mask.Get(Layers.EnemyHurtbox));
		foreach (Collider enemyCol in nearEnemies) {
			enemyCol.GetComponent<EnemyCommunication>().nearbyAttacker += 1;
		}
	}

	// helper: logic for deteriming whigh following range is being used.
	bool UsingApprochRange(float distance) {
		return (approchDistance >= distance) && (approchDistance <= distance + ApprochDeviance);
	}

	static readonly StunTime[] AttackStunTimeTable = {
		StunTime.None, // None
		StunTime.Short, // LAttack
		StunTime.Short, // LAttack2
		StunTime.Long, // LAttack3
		StunTime.Long, // Chop
		StunTime.Long, // Slam
		StunTime.Short, // Spin
		StunTime.None, // HeadThrow (Handled by head projectile)
		StunTime.None, // Dashing
		StunTime.Short, // LethalDashing
		StunTime.None, // ShotgunThrow (Handled by head projectile)
	};

	static readonly float[] AttackMeterGainOnHitTable = {
		0.0f, // None
		0.1f, // LAttack
		0.1f, // LAttack2
		0.2f, // LAttack3
		1.0f, // Chop (Should be enough to kill a basic in one hit)
		0.0f, // Slam (Special case, Slam does different damages at different radii)
		0.0f, // Spin
		0.0f, // HeadThrow (Hit + Damage is handled by the projectile, we shouldn't even get a hit while in this attack)
		0.0f, // Dashing
		0.0f, // LethalDash
		0.0f, // ShotgunThrow (Hit + Damage is handled by the projectile, we shouldn't even get a hit while in this attack)
	};

	void OnTriggerEnter(Collider other) {
		if (!isImmune && immunityTime <= 0) {
			KnockbackInfo newKnockbackInfo = new KnockbackInfo(Quaternion.identity, 0, 0);
			StunTime stunTime = StunTime.None;
			float damage = 0f;
			float meterGain = 0f;
			bool playHitSound = false;
			bool _extraStun = false;
			bool triggerStunSphere = true;
			bool interrupted = false;
			if (directive == Directive.PerformAttack) interrupted = true;

			if (other.gameObject.layer == (int)Layers.PlayerHitbox) {
				HeadProjectile head = other.GetComponentInParent<HeadProjectile>();
				PlayerController player = other.GetComponentInParent<PlayerController>();
				ExplosiveTrap explosiveTrap = other.GetComponentInParent<ExplosiveTrap>();
				StunSphere stunSphere = other.GetComponent<StunSphere>();

				if (player != null) {
					if (!isArmored) gameMan.SpawnParticle(0, other.transform.position, 1f);
					else gameMan.SpawnParticle(13, armorPoint.position, 0.75f);
					gameMan.SpawnParticle(12, other.transform.position, 1f);

					newKnockbackInfo = other.GetComponent<GetKnockbackInfo>().GetInfo(this.gameObject);
					stunTime = AttackStunTimeTable[(int)player.currentAttack];
					damage = PlayerController.AttackDamageTable[(int)player.currentAttack];
					meterGain = AttackMeterGainOnHitTable[(int)player.currentAttack];
					if (isArmored) meterGain = 0f;
					wasHitByChop = false;
					playHitSound = true;

					// Attack specific code
					switch (gameMan.playerController.currentAttack) {
						case PlayerController.Attacks.Slam:
							float posDifference = Mathf.Abs((player.transform.position - transform.position).sqrMagnitude);
							Debug.Log(gameObject.name + "'s posDifference after slam: " + posDifference);
							if (posDifference < 40f) {
								damage = 6f;
								if (isArmored) damage = 8f;
							} 
							else if (posDifference < 80f) {
								damage = 3f;
								if (isArmored) damage = 5f;
							} 
							break;

						case PlayerController.Attacks.Chop:
							// @TODO(Roskuski): Different System to prevent headpickup spawns from chop. this current system will not work well if we implment enemies with healthpools that can surrive a chop
							var chopThreshold = 5f;
							if (interrupted) chopThreshold = 10f;
							if (!isArmored && health <= chopThreshold) {
								wasHitByChop = true;
								gameMan.SpawnParticle(12, other.transform.position, 1.5f);
								sounds.Sound_EnemyLob();
								playHitSound = false;
							}
							else {
								playHitSound = true;
								meterGain = 0f;
							}
							gameMan.ShakeCamera(5f, 0.1f);
							if (GameObject.Find("HapticManager") != null) HapticManager.PlayEffect(player.hapticEffects[2], this.transform.position);
							break;

						case PlayerController.Attacks.LethalDash:
							if (!isArmored) sounds.Sound_EnemySliced();
							gameMan.ShakeCamera(3f, 0.1f);
							if (GameObject.Find("HapticManager") != null) HapticManager.PlayEffect(player.hapticEffects[2], this.transform.position);
							break;

						default:
							Debug.Log("I, " + this.name + " was hit by an unhandled attack (" + gameMan.playerController.currentAttack + ")");
							break;
					}

				}
				else if (head != null) {
					// NOTE(Roskuski): Head projectial direct hit
					gameMan.SpawnParticle(0, other.transform.position, 2f);
					damage = 5f;
					stunTime = StunTime.Long;
					_extraStun = true;
					playHitSound = true;
				}
				else if (explosiveTrap != null) {
					// NOTE(Roskuski): Knockback trap
					newKnockbackInfo = other.GetComponent<GetKnockbackInfo>().GetInfo(this.gameObject);
					stunTime = StunTime.Long;
					_extraStun = true;
					triggerStunSphere = false;
					playHitSound = true;
					if (isArmored) damage = 5f;
				}
				else if (stunSphere != null) {
					newKnockbackInfo = other.GetComponent<GetKnockbackInfo>().GetInfo(this.gameObject);
					if (isArmored == false) {
						stunTime = StunTime.Long;
						triggerStunSphere = false;
						playHitSound = true;
					}
					damage = stunSphere.damage;
					if (damage > 0) {
						playHitSound = true;
						stunTime = StunTime.Long;
					}
				}
			}
			else if (other.gameObject.layer == (int)Layers.AgnosticHitbox) {
				if (other.GetComponentInParent<Exploding>() != null) {
					// NOTE(Roskuski): Explosive enemy
					newKnockbackInfo = other.GetComponent<GetKnockbackInfo>().GetInfo(this.gameObject);
					stunTime = StunTime.Long;
					_extraStun = true;
					//newKnockbackInfo.force *= 2f;
					damage = 6f;
					playHitSound = true;
					triggerStunSphere = false;
				}
			}

			//Multiply damage if interrupted out of an attack
			if (interrupted) {
				damage *= 2f;
            }

			if (!isArmored) {
				if (playHitSound) sounds.CharacterGetHit();
				health -= damage;
				gameMan.playerController.ChangeMeter(meterGain);
				if (stunTime == StunTime.Long) sounds.Sound_EnemyStun();
				ChangeDirective_Stunned(stunTime, newKnockbackInfo, _extraStun);
			}
            else {
				if (playHitSound) sounds.Sound_ArmorHit();
				if (damage >= 3) ChangeDirective_Stunned(StunTime.Short, newKnockbackInfo, false);
				if (damage > 0) damage -= 1;
				health -= damage;
				if (health <= 4) ArmorBreak(stunTime, newKnockbackInfo, _extraStun, triggerStunSphere);
			}
		}
	}

	public void ArmorBreak(StunTime _stunTime, KnockbackInfo _newKnockbackInfo, bool extraStun, bool _stunSphere) {
		ChangeDirective_Stunned(_stunTime, _newKnockbackInfo, extraStun);
		sounds.Sound_ArmorBreak();
		sounds.Sound_EnemyCrystalShatter();
		gameMan.SpawnParticle(13, armorPoint.position, 1f);
		gameMan.SpawnParticle(14, armorPoint.position, 0.5f);
		gameMan.ShakeCamera(5f, 0.15f);
		if (GameObject.Find("HapticManager") != null) HapticManager.PlayEffect(gameMan.player.GetComponent<PlayerController>().hapticEffects[2], this.transform.position);
		if (_stunSphere) Instantiate(stunSphere, armorPoint.position, Quaternion.identity);
		Util.SpawnFlash(gameMan, 1, armorPoint.position, true);
		health = 4;
		immunityTime = 0.3f;
		for (int i = 0; i < armorMesh.Length; i++) armorMesh[i].enabled = false;
		isArmored = false;
	}

	void Start() {
		navAgent = this.GetComponent<NavMeshAgent>();
		animator = this.GetComponent<Animator>();
		swordHitbox = transform.Find("Weapon_Controller").GetComponent<BoxCollider>();
		enemyCommunication = this.GetComponent<EnemyCommunication>();
		sounds = this.GetComponent<MotionAudio_Skel>();
		flashSpot = transform.Find("Weapon_Controller");
		smokePoint = transform.Find("SmokePoint");

		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();

		navAgent.updatePosition = false;
		navAgent.updateRotation = false;

		CheckForHardMode();

		if (randomizeStats) for (int index = 0; index < System.Enum.GetNames(typeof(TraitKind)).Length; index += 1) {
			traits[index] = Random.Range(TraitMax * -1 + 1, TraitMax); // Getting -TraitMax in all traits breaks the current (2-24-2023) RollTraitChoice
		}

		if (!animationTimesPopulated) {
			animationTimesPopulated = true;
			animationTimes = new Dictionary<string, float>(animator.runtimeAnimatorController.animationClips.Length);
			foreach (AnimationClip clip in animator.runtimeAnimatorController.animationClips) {
				animationTimes[clip.name] = clip.length;
			}
		}

		// Setup Directive.Spawn
		directive = Directive.Spawn;
		if (isSandbag) { directive = Directive.Sandbag; }
		animationTimer = animationTimes["Enemy_Spawn"];
		
		{
			// Between frames 10, 16, move upward
			// Between frames 17, 24, move downward, hit the floor
			float upwardsTime   = (0.2857f - 0.1758f) * animationTimes["Enemy_Spawn"];
			float downwardsTime = (0.4285f - 0.3035f) * animationTimes["Enemy_Spawn"];
			float lateralTime   = (0.0000f - 0.4285f) * animationTimes["Enemy_Spawn"];
			spawnUpwardsSpeed = 5f;

			RaycastHit hitInfo;
			// NOTE(Roskuski): I'm not certain why we need to rotate Vector3.back to test in the correct direction here. Vector3.forward results in testing in the direction behind the skeleton.
			if (!Physics.Raycast(this.transform.position + this.transform.rotation * Vector3.back * spawnLateralSpeed * lateralTime, Vector3.down, out hitInfo, Mathf.Infinity, Mask.Get(Layers.Ground))) {
				Physics.Raycast(this.transform.position, Vector3.down, out hitInfo, Mathf.Infinity, Mask.Get(Layers.Ground));
				spawnLateralSpeed = 0;
			}
			spawnDownwardsSpeed = (spawnUpwardsSpeed * upwardsTime + hitInfo.distance) / downwardsTime;
		}

		model = transform.Find("Skeleton_Base_Model").GetComponent<SkinnedMeshRenderer>();
		materials = model.materials;
		if (gameMan.armorEnabled) RollArmorChance();
		if (isArmored) {
			health += 3;
			for (int i = 0; i < armorMesh.Length; i++) armorMesh[i].enabled = true;
		}
	}

	void FixedUpdate() {
		int layerMask = ~Mask.Get(new Layers[] {Layers.EnemyHitbox, Layers.Corpses});

		if (remainingKnockbackTime > 0 || directive == Directive.Spawn) {
			layerMask &= ~Mask.Get(Layers.StickyLedge);
		}

		if (directive != Directive.Spawn) {
			Util.PerformCheckedLateralMovement(this.gameObject, 0.75f, 0.5f, movementDelta * Time.fixedDeltaTime, layerMask);
			Util.PerformCheckedVerticalMovement(this.gameObject, 0.75f, 0.2f, 0.5f, 30.0f);
		}
		else if (directive == Directive.Spawn) {
			transform.position += movementDelta * Time.fixedDeltaTime;
		}
	}

	void Update() {
		Vector3 playerPosition = gameMan.player.position;
		Quaternion playerRotation = gameMan.player.rotation;
		Vector3 deltaToPlayer = gameMan.player.position - this.transform.position;
		float distanceToPlayer = Vector3.Distance(this.transform.position, gameMan.player.position);
		movementDelta = Vector3.zero; 

		hitflashTimer -= Time.deltaTime;
		debugTargetTimer -= Time.deltaTime;
		Material[] materialList = model.materials;
		for (int i = 0; i < materialList.Length; i++) {
			if (hitflashTimer > 0) {
				materialList[i] = hitflashMat;
			}
			else {
				materialList[i] = materials[i];
			}

			if (debugTargetTimer > 0) {
				materialList[i] = debugTargetMat;
			}
			else {
				if (debugHoming) materialList[i] = materials[i];
			}
		}
		model.materials = materialList;

		// Processing information from other enemies
		for (int index = 0; index < enemyCommunication.nearbyAttacker; index += 1) {
			attackCooldown += 1.0f + Random.Range(0.0f, 1.0f);
		}
		enemyCommunication.nearbyAttacker = 0;

		// Ensure that the sword hitbox is disabled when we're not attacking
		if (directive != Directive.PerformAttack) {
			swordHitbox.enabled = false;
		}

		// Preform knockback regardless of what we want to do
		movementDelta += Util.ProcessKnockback(ref remainingKnockbackTime, knockbackInfo);

		// Directive Changing
		switch (directive) {
			case Directive.Inactive: // using this as a generic start point for enemy AI
				inactiveWait -= Time.deltaTime;
				if (inactiveWait <= 0) {
					if (isSandbag) { directive = Directive.Sandbag; }
					else {
						preferRightStrafe = Random.Range(0, 2) == 1 ? true : false;
						int choice = RollTraitChoice( new ChoiceEntry[]{
								new ChoiceEntry(1, new float[]{0.5f, 0.5f}),
								new ChoiceEntry(1, new float[]{0.5f, 0.5f}),
								new ChoiceEntry(1, new float[]{0.5f, 0.5f}),
								new ChoiceEntry(1, new float[]{0.5f, 0.5f}),
								});
						choiceTimer = new float[] { 3.0f, 2.0f, 1.0f, 0.5f }[choice];

						choice = RollTraitChoice( new ChoiceEntry[]{
								new ChoiceEntry(1, new float[]{-0.5f, 0.5f}),
								new ChoiceEntry(1, new float[]{0.75f, 0.25f}),
								new ChoiceEntry(1, new float[]{0.75f, 0.25f})
								});
						switch (choice) {
							default: Debug.Assert(false, choice); break;
							case 0:
								ChangeDirective_MaintainDistancePlayer(LooseApprochDistance);
								break;
							case 1:
								ChangeDirective_MaintainDistancePlayer(CloseApprochDistance);
								break;
							case 2:
								ChangeDirective_MaintainDistancePlayer(TightApprochDistance);
								break;
						}
					}
				}
				break;

			case Directive.Sandbag:
				// Doing nothing, with style...
				break;

			case Directive.Stunned:
				stunDuration -= Time.deltaTime;

				if (stunDuration < 0) {
					ChangeDirective_Inactive(0);
					stunDuration = 0;
				}
				if (isInHeavyStun && !hasStartedStunRecovery && stunDuration <= 0.620f) {
					hasStartedStunRecovery = true;
					animator.SetTrigger("HeavyHurtStartRecovery");
				}
				break;

			case Directive.Spawn:
				isImmune = true;
				if (animationTimer < 0.0f) {
					isImmune = false;
					ChangeDirective_Inactive(0);

					RaycastHit hitInfo;
					if (Physics.Raycast(this.transform.position + Vector3.up * 2.5f, Vector3.down, out hitInfo, 2.6f, Mask.Get(Layers.Ground))) {
						this.transform.position += Vector3.down * (hitInfo.distance - 2.6f); // NOTE(Roskuski): should dislodge skeletons from the ground if frame lag causes them to sink inwards.
					}
				}

				{ // NOTE(Roskuski): Needs to be in a deeper scope because we use a variable of the same name elsewhere in this switch statement
					float animationTimerRatio = 1.0f - animationTimer / animationTimes["Enemy_Spawn"];
					// Between frames 0, 24 move forward
					if (animationTimerRatio > 0 && animationTimerRatio < 0.4285f) {
						movementDelta += this.transform.rotation * Vector3.forward * spawnLateralSpeed;
					}
					
					// @TODO(Roskuski): Blend between up and down?
 
					// Between frames 10, 16, move upward
					if (animationTimerRatio > 0.1785f && animationTimerRatio < 0.2857f) {
						movementDelta += this.transform.rotation * Vector3.up * spawnUpwardsSpeed;
					}
					// Between frames 17, 24, move downward, hit the floor
					if (animationTimerRatio > 0.3035f && animationTimerRatio < 0.4285f) {
						movementDelta += this.transform.rotation * Vector3.down * spawnDownwardsSpeed;
					}
					
					if (animationTimerRatio > 0.4285f) {
						isImmune = false;
					}
				}
				break;

			case Directive.MaintainDistancePlayer:
				navAgent.nextPosition = this.transform.position;

				navAgent.SetDestination(playerPosition + targetOffset);

				bool isBackpedaling = false;
				bool isStrafing = false;
				bool isCloseToLedge = false;

				if (CanAttemptNavigation() && navAgent.path.corners.Length >= 2) {
					const float NearRadius = 2;

					Vector3 nextNodeDelta = navAgent.path.corners[1] - this.transform.position;

					Collider[] nearEnemies = Physics.OverlapSphere(this.transform.position, NearRadius, Mask.Get(Layers.EnemyHitbox));
					Vector3[] nearEnemyDeltas;
					if (nearEnemies.Length != 0) {
						nearEnemyDeltas = new Vector3[nearEnemies.Length - 1];
					}
					else {
						nearEnemyDeltas = new Vector3[0];
					}

					for (int index = 0; index < nearEnemyDeltas.Length; index += 1) {
						if (nearEnemies[index].gameObject != this.gameObject) {
							nearEnemyDeltas[index] = nearEnemies[index].transform.position - this.transform.position;
						}
					}

					Quaternion angleStep = Quaternion.AngleAxis(360.0f / directionWeights.Length, Vector3.up);

					// Pathfinding phase 
					{
						Vector3 consideredDelta = Vector3.forward;
						Vector3 desiredFlank = Quaternion.AngleAxis(flankStrength, Vector3.up) * nextNodeDelta.normalized;

						for (int index = 0; index < directionWeights.Length; index += 1) {
							if (flankStrength != float.PositiveInfinity) {
								directionWeights[index] = Vector3.Dot(nextNodeDelta.normalized, consideredDelta) * 0.5f + Vector3.Dot(desiredFlank, consideredDelta) * 0.5f + 1.0f;
							}
							else {
								directionWeights[index] = Vector3.Dot(nextNodeDelta.normalized, consideredDelta) + 1.0f;
							}

							// NOTE(Roskuski): Advance the angle to the next index.
							consideredDelta = angleStep * consideredDelta;
						}
					}

					// Consider if the player is moving straight for me!
					if (DistanceToTravel() < 1.5f) {
						Vector3 mInput3d = new Vector3(gameMan.playerController.mInput.x, 0, gameMan.playerController.mInput.y);
						if (gameMan.playerController.mInput != Vector2.zero &&
								(Vector3.Dot(mInput3d, this.transform.rotation * Vector3.forward) < -0.7)) {
							isBackpedaling = true;

							Vector3 consideredDelta = Vector3.forward;
							for (int index = 0; index < directionWeights.Length; index += 1) {
								float maxDot = Vector3.Dot(Quaternion.AngleAxis(180, Vector3.up) * nextNodeDelta.normalized, consideredDelta) + 1.0f;
								directionWeights[index] = maxDot;

								// NOTE(Roskuski): Advance the angle to the next index.
								consideredDelta = angleStep * consideredDelta;
							}
						}
						else { // Lets strafe around the player
							isStrafing = true;
							Vector3 consideredDelta = Vector3.forward;
							for (int index = 0; index < directionWeights.Length; index += 1) {
								float rightScore = Vector3.Dot(Quaternion.AngleAxis(90, Vector3.up) * nextNodeDelta.normalized, consideredDelta) + 1.0f;
								float leftScore = Vector3.Dot(Quaternion.AngleAxis(-90, Vector3.up) * nextNodeDelta.normalized, consideredDelta) + 1.0f;
								if (preferRightStrafe) {
									leftScore *= 0.75f;
								}
								else if (!preferRightStrafe) {
									rightScore *= 0.75f;
								}

								directionWeights[index] = Mathf.Max(leftScore, rightScore);

								// NOTE(Roskuski): Advance the angle to the next index.
								consideredDelta = angleStep * consideredDelta;
							}

						}
					}

					// Consider enemies
					{
						Vector3 consideredDelta = Vector3.forward;
						for (int index = 0; index < directionWeights.Length; index += 1) {
							// NOTE(Roskuski): [0, 1] 1 is no enemy contention, 0 is maximum enemy contention
							float enemyMod = 1;
							if (nearEnemyDeltas.Length > 0) {
								float totalEnemyWeight = 0;
								for (int enemyIndex = 0; enemyIndex < nearEnemyDeltas.Length; enemyIndex += 1) {
									// NOTE(Roskuski): According to the docs, Mathf.Lerp clamps it's thrid param
									float enemyWeight = Mathf.Lerp(0, 1, nearEnemyDeltas[enemyIndex].magnitude / (NearRadius * 0.5f));
									totalEnemyWeight += (Vector3.Dot(consideredDelta.normalized, nearEnemyDeltas[enemyIndex].normalized) + 1) * enemyWeight;
								}
								totalEnemyWeight /= nearEnemyDeltas.Length;
								enemyMod = 1 - (totalEnemyWeight / 2);
							}
							// makes enemyMod more impactful the more enemies are around
							directionWeights[index] *= (enemyMod / (1 + (nearEnemyDeltas.Length)));

							// NOTE(Roskuski): Advance the angle to the next index.
							consideredDelta = angleStep * consideredDelta;
						}
					}

					// Consider Ledges
					{
						Vector3 consideredDelta = Vector3.forward;
						for (int index = 0; index < directionWeights.Length; index += 1) {
							float ledgeMod = 1;
							float hitDistance = 0;
							const int MaxQuantums = 4;

							for (int quantums = 1; quantums <= MaxQuantums; quantums += 1) {
								NavMeshHit hit;
								Vector3 testDelta = consideredDelta * MoveSpeed * Time.deltaTime * (float)quantums;
								if (NavMesh.SamplePosition(this.transform.position + testDelta, out hit, 0.2f, NavMesh.AllAreas)) {
									hitDistance = Vector3.Distance(hit.position, this.transform.position);
								}
								else {
									break;
								}
							}

							if (hitDistance > MoveSpeed * Time.deltaTime * MaxQuantums * 0.20) {
								ledgeMod = Mathf.Clamp01(hitDistance / (MoveSpeed * Time.deltaTime * MaxQuantums)); // NOTE(Roskuski): the closer that hitDistance is to the max distance we are considering, the more okay we are with taking this path.
							}
							else {
								isCloseToLedge = true;
								ledgeMod = 0;
							}

							directionWeights[index] *= ledgeMod;

							// NOTE(Roskuski): Advance the angle to the next index.
							consideredDelta = angleStep * consideredDelta;
						}
					}

					// Consider Walls
					{
						Vector3 consideredDelta = Vector3.forward;
						for (int index = 0; index < directionWeights.Length; index += 1) {
							if (Physics.Raycast(this.transform.position + Vector3.up * 0.75f, consideredDelta, 1.5f, ~Mask.Get(new Layers[]{Layers.PlayerHitbox, Layers.EnemyHitbox, Layers.AgnosticHitbox}))) {
								directionWeights[index] *= 0.25f;
							}

							// NOTE(Roskuski): Advance the angle to the next index.
							consideredDelta = angleStep * consideredDelta;
						}

					}

					// Bias towards current direction
					{
						Vector3 consideredDelta = Vector3.forward;
						int currentDirectionIndex = -1;
						float bestFit = -1;
						for (int index = 0; index < directionWeights.Length; index += 1) {
							float score = Vector3.Dot(consideredDelta.normalized, this.moveDirection.normalized * Vector3.forward);
							if (score > bestFit) {
								currentDirectionIndex = index;
								bestFit = score;
							}

							// NOTE(Roskuski): Advance the angle to the next index.
							consideredDelta = angleStep * consideredDelta;
						}
						directionWeights[currentDirectionIndex] *= 1.1f;
					}

					Quaternion chosenAngle = Quaternion.identity;
					Quaternion consideredAngle = Quaternion.identity;
					float bestWeight = -2;
					for (int index = 0; index < directionWeights.Length; index += 1) {
						if (directionWeights[index] > bestWeight) {
							bestWeight = directionWeights[index];
							chosenAngle = consideredAngle;
						}
						// NOTE(Roskuski): Advance the angle to the next index.
						consideredAngle *= angleStep;
					}

					if (isCloseToLedge) {
						moveDirection = Quaternion.RotateTowards(moveDirection, chosenAngle, TurnSpeed * Time.deltaTime * 10); // NOTE(Roskuski): Turn 10 times as fast if we're near a ledge, not sure how much of a difference this has
					}
					else {
						moveDirection = Quaternion.RotateTowards(moveDirection, chosenAngle, TurnSpeed * Time.deltaTime);
					}

					// Swap strafing direction if we're obstructed in our current one
					if (isStrafing) {
						int bestRightIndex = -1;
						float bestRightScore = -1;
						int bestLeftIndex = -1;
						float bestLeftScore = -1;

						Vector3 consideredDelta = Vector3.forward;
						for (int index = 0; index < directionWeights.Length; index += 1) {
							float rightScore = Vector3.Dot(Quaternion.AngleAxis(90, Vector3.up) * nextNodeDelta.normalized, consideredDelta) + 1.0f;
							float leftScore = Vector3.Dot(Quaternion.AngleAxis(-90, Vector3.up) * nextNodeDelta.normalized, consideredDelta) + 1.0f;
							if (rightScore > bestRightScore) {
								bestRightScore = rightScore;
								bestRightIndex = index;
							}
							else if (!preferRightStrafe) {
								bestLeftScore = leftScore;
								bestLeftIndex = index;
							}

							// NOTE(Roskuski): Advance the angle to the next index.
							consideredDelta = angleStep * consideredDelta;
						}

						if (preferRightStrafe) {
							if (directionWeights[bestRightIndex] < 0.25f) {
								preferRightStrafe = false;
							}
						}
						else {
							if (directionWeights[bestLeftIndex] < 0.25f) {
								preferRightStrafe = true;
							}
						}
					}
				}

				float speedModifier = 1.0f;
				if (isBackpedaling) {
					speedModifier = 0.65f;
				}
				else if (isStrafing) {
					speedModifier = 0.6f;
				}
				else {
					speedModifier = Mathf.Lerp(0.5f, 1, (Vector3.Distance(this.transform.position, playerPosition + targetOffset) - approchDistance) + 1);
				}

				movementDelta += moveDirection * Vector3.forward * MoveSpeed * speedModifier;
				{
					Quaternion rotationDelta = moveDirection * Quaternion.Inverse(this.transform.rotation);
					Vector3 animatorMove = rotationDelta * Vector3.back;
					animator.SetFloat("moveX", animatorMove.x);
					animator.SetFloat("moveY", animatorMove.z);
				}

				// Rotate the visual seperately
				if (distanceToPlayer < 6.25 || speedModifier < 0.75f) {
					Vector3 deltaToPlayerNoY = deltaToPlayer;
					deltaToPlayerNoY.y = 0;
					Quaternion rotationDelta = Quaternion.LookRotation(deltaToPlayerNoY, Vector3.up);
					this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, rotationDelta, TurnSpeed * Time.deltaTime);
				}
				else if (Quaternion.Angle(this.transform.rotation, this.moveDirection) > 2) {
					this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, this.moveDirection, TurnSpeed * Time.deltaTime);
				}

				// @TODO(Roskuski): This might look weird if the feet don't line up when we attempt to make an attack.
				// might want to only choose an attack when it would blend sensiably in the animation.
				// adding a data keyframe will make this a snap to impl.

				if (wantsSlash && DistanceToTravel() < 1.5f) {
					ChangeDirective_PerformAttack(Attack.Slash);
					wantsSlash = false;
					animator.SetBool("wantsSlash", wantsSlash);
				}
				else if (DistanceToTravel() < 1.5f && wantsSlash == false) {
					flankStrength = float.PositiveInfinity;
					choiceTimer -= Time.deltaTime;
					attackCooldown -= Time.deltaTime;

					if (choiceTimer <= 0) {
						ChoiceEntry[] choices = new ChoiceEntry[] {
							new ChoiceEntry(5, new float[]{-0.5f, 0.5f}),
							new ChoiceEntry(3, new float[]{0.75f, 0.25f}),
							new ChoiceEntry(3, new float[]{0.75f, 0.25f}),
						};

						// @TODO(Roskuski): Previously we blocked doing attacks when the player was hit
						if (attackCooldown > 0) {
							choices[1].baseWeight = 0;
							choices[2].baseWeight = 0;
						}

						int choice = RollTraitChoice(choices);
						switch (choice) {
							default:
								Debug.Assert(false);
								break;
							case 0: // Change ApprochDistance
								choiceTimer = 3.0f;
								choice = RollTraitChoice( new ChoiceEntry[] {
										new ChoiceEntry(1, new float[]{-0.5f, 0.5f}),
										new ChoiceEntry(1, new float[]{0.5f, 0.5f}),
										}); 

								if (choice == 0 && UsingApprochRange(LooseApprochDistance)) {
									choice = 1;
								}
								if (choice == 1 && UsingApprochRange(TightApprochDistance)) {
									choice = 0;
								}
								switch(choice) {
									default:
										Debug.Assert(false);
										break;
									case 0: // Move Outwards
										if (UsingApprochRange(TightApprochDistance)) {
											ChangeDirective_MaintainDistancePlayer(CloseApprochDistance);
										}
										else if (UsingApprochRange(CloseApprochDistance)) {
											ChangeDirective_MaintainDistancePlayer(LooseApprochDistance);
										}
										break;
									case 1: // Move Inwards
										if (UsingApprochRange(LooseApprochDistance)) {
											ChangeDirective_MaintainDistancePlayer(CloseApprochDistance);
										}
										else if (UsingApprochRange(CloseApprochDistance)) {
											ChangeDirective_MaintainDistancePlayer(TightApprochDistance);
										}
										break;
								}
								break;
							case 1: // Walkup and slash
								wantsSlash = true;
								animator.SetBool("wantsSlash", true);
								ChangeDirective_MaintainDistancePlayer(0);
								break;
							case 2: // Lunge
								ChangeDirective_PerformAttack(Attack.Lunge);
								break;
						}
					}
				}
				else if (DistanceToTravel() > 2.0f && flankStrength == float.PositiveInfinity) {
					int choice = RollTraitChoice( new ChoiceEntry[] {
							new ChoiceEntry(3, new float[]{0.0f, -1f}),
							new ChoiceEntry(2, new float[]{0.0f, 0.5f}),
							new ChoiceEntry(2, new float[]{0.0f, 1f}),
							});
					switch(choice) {
						default:
							Debug.Assert(false);
							break;
						case 0: // Don't flank
							flankStrength = 0;
							break;
						case 1: // Flank weak
							flankStrength = Random.Range(30.0f, 50.0f);
							flankStrength *= preferRightStrafe ? 1 : -1;
							break;
						case 2: // Flank strong
							flankStrength = Random.Range(50.0f, 70.0f);
							flankStrength *= preferRightStrafe ? 1 : -1;
							break;
					}
				}
				break;
			case Directive.PerformAttack:
				switch (currentAttack) {
					case Attack.None:
						Debug.Assert(false);
						break;
					case Attack.Slash:
						{
							// @TODO(Roskuski): These hitbox activations were keyed to the animations before, make it so again
							float animationTimerRatio = 1.0f - animationTimer / animationTimes["Enemy_Attack_Slash"];
							if (animationTimerRatio <= 0.50f) {
								if (distanceToPlayer < 6.0f) {
									Vector3 deltaToPlayerNoY = deltaToPlayer;
									deltaToPlayerNoY.y = 0;

									this.transform.rotation = Quaternion.LookRotation(deltaToPlayerNoY, Vector3.up);
								}
							}

							if (animationTimerRatio >= 0.25f && animationTimerRatio <= 0.5f) {
								movementDelta += this.transform.rotation * Vector3.forward * 15f * Mathf.Lerp(0.25f, 1f, (animationTimerRatio - 0.25f) / (0.5f - 0.25f));
							}

							if (animationTimer < 0.0f) {
								ChangeDirective_Inactive(0);
								navAgent.enabled = true;
							}
							break;
						}
					case Attack.Lunge:
						{
							swordHitbox.enabled = true;
							float animationTimerRatio = 1.0f - animationTimer / animationTimes["Enemy_Attack_Dash"];
							
							if (animationTimerRatio <= 0.56) {
								if (distanceToPlayer < 15.0f) {
									Vector3 deltaToPlayerNoY = deltaToPlayer;
									deltaToPlayerNoY.y = 0;

									this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(deltaToPlayerNoY, Vector3.up), 360 * Time.deltaTime);
								}
							}

							if (animationTimerRatio >= 0.45f && animationTimerRatio <= 0.8f) {
								movementDelta += (this.transform.rotation * Vector3.forward) * LungeSpeed;
							}

							if (animationTimer < 0.0f) {
								// If we found ourselves off geometry, wait util we finish falling.
								ChangeDirective_Inactive(0);
								navAgent.enabled = true;
							}
							break;
						}
				}
				break;
			default: Debug.Assert(false); break;
		}

		animator.SetInteger("Ai Directive", (int)directive);
		animationTimer -= Time.deltaTime * animator.GetCurrentAnimatorStateInfo(0).speed;
		if (immunityTime > 0) {
			immunityTime -= Time.deltaTime;
		}

		if (health <= 0) {
			shouldDie = true;
		}

		if (transform.position.y <= -50f) {
			shouldDie = true;
		}

		if (shouldDie) {
			Vector3 spawnPos = transform.position + 3 * Vector3.up;
			bool redSkull;
			if (isHardMode) redSkull = true;
			else redSkull = false;

			if (transform.position.y > -49f) {
				if (isCrystallized) {
					gameMan.DeterminePickups(spawnPos, true, false);
					gameMan.isCrystalEnemyAlive = false;
				}
				else if (!wasHitByChop) {
					gameMan.DeterminePickups(spawnPos, isCrystallized, redSkull);
				}
			}

			float corpseForce = 0;
			int corpse = 0;
			if (redSkull) corpse = 1;
			if (remainingKnockbackTime > 0) corpseForce = knockbackInfo.force / 10f;
			if (!wasHitByChop) {
				if (isCrystallized) {
					gameMan.SpawnParticle(11, transform.position, 0.8f);
					Util.SpawnFlash(gameMan, 5, armorPoint.position, true);
				}
				else {
					gameMan.SpawnCorpse(corpse, transform.position, transform.rotation, corpseForce, true);
				}
			}
			else {
				if (isCrystallized) {
					gameMan.SpawnParticle(11, transform.position, 0.8f);
					Util.SpawnFlash(gameMan, 5, armorPoint.position, true);
				}
				else {
					gameMan.SpawnCorpse(corpse, transform.position, transform.rotation, corpseForce, false);
				}
			}

			float voiceChance = Random.Range(1, 10);
			if (voiceChance <= 4f) sounds.Sound_EnemyVO();

			Destroy(this.gameObject); 
		}
	}

    public void SmokeParticleSmall() {
		gameMan.SpawnParticle(17, smokePoint.position, 1f);
    }

	public void SmokeParticleMedium() {
		gameMan.SpawnParticle(18, smokePoint.position, 1f);
	}

	private void OnDestroy() {
		if (shouldAddToKillTotal) {
			gameMan.enemiesAlive -= 1;
			gameMan.enemiesKilledInLevel += 1;
			gameMan.AddToKillStreak(1, 1f);
			GameManager.enemiesKilledInRun += 1;
			if (isCrystallized) {
				gameMan.isCrystalEnemyAlive = false;
				sounds.Sound_EnemyCrystalShatter();
			}
			Initializer.save.versionLatest.basicEnemyKills++;
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.blue;
		Gizmos.DrawWireSphere(transform.position, enemyCommunicationRange);

		Gizmos.DrawRay(transform.position + Vector3.up * 2.2f, moveDirection * Vector3.forward);

		float maxWeight = 0;
		foreach (float datum in directionWeights) {
			maxWeight = Mathf.Max(maxWeight, datum);
			Debug.Assert(datum >= 0);
		}

		Quaternion angleStep = Quaternion.AngleAxis(360.0f / directionWeights.Length, Vector3.up);
		Vector3 consideredDelta = Vector3.forward;
		for (int index = 0; index < directionWeights.Length; index += 1) {
			Gizmos.color = Color.Lerp(Color.red, Color.green, directionWeights[index] / maxWeight);
			Gizmos.DrawRay(transform.position + Vector3.up * 2, consideredDelta);

			// NOTE(Roskuski): Advance the angle to the next index.
			consideredDelta = angleStep * consideredDelta;
		}
	}
}
