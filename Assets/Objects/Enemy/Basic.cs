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

	float[] directionWeights = new float[32];

	// NOTE(Roskuski): End of ai state

	public float health;
	public float partialMeter; //how much meter the player gets for just hitting this?
	bool shouldDie = false;
	public float dropChance; //chance to drop a head (0-100)

	KnockbackInfo knockbackInfo;
	float remainingKnockbackTime = 0f;
	[SerializeField] float hitflashTimer = 0f;
	bool isInHeavyStun = false;
	bool hasStartedStunRecovery = false;

	[SerializeField] bool isSandbag = false;
	bool isImmune = false;

	public const float LungeSpeed = 15f;
	// NOTE(Roskuski): copied from the default settings of navMeshAgent
	public const float MoveSpeed = 7f;
	public const float TurnSpeed = 360.0f; // NOTE(Roskuski): in Degrees per second

	public const float TightApprochDistance = 4;
	public const float CloseApprochDistance = 6;
	public const float LooseApprochDistance = 10;
	public const float ApprochDeviance = 2;

	// NOTE(Roskuski): Internal references
	NavMeshAgent navAgent;
	Animator animator;
	BoxCollider swordHitbox;
	EnemyCommunication enemyCommunication;

	SkinnedMeshRenderer model;
	Material[] materials;
	public Material hitflashMat;

	// NOTE(Roskuski): External references
	GameManager gameMan;

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

	bool CanAttemptNavigation() {
		return (navAgent.pathStatus == NavMeshPathStatus.PathComplete || navAgent.pathStatus == NavMeshPathStatus.PathPartial) && navAgent.path.corners.Length >= 2;
	}

	float DistanceToTravel() {
		return navAgent.remainingDistance - approchDistance;
	}

	public enum StunTime {
		ShortStun,
		LongStun,
	}

	public void ChangeDirective_Stunned(StunTime stunTime, KnockbackInfo newKnockbackInfo) {
		if (directive == Directive.Spawn) {
			return; // do not trans from Spawn -> Stunned
		}
		directive = Directive.Stunned;
		hasStartedStunRecovery = false;
		isInHeavyStun = false;
		
		float stunValue = 0;
		switch (stunTime) {
			case StunTime.ShortStun:
				stunValue = 0.5f;
				animator.SetTrigger("wasHurt");
				break;
			case StunTime.LongStun:
				stunValue = 2f;
				animator.SetTrigger("wasHeavyHurt");
				isInHeavyStun = true;
				break;
			default:
				Debug.Assert(false);
				break;
		}

		stunDuration += stunValue;
		hitflashTimer = 0.25f;

		swordHitbox.enabled = false;

		knockbackInfo = newKnockbackInfo;
		remainingKnockbackTime = knockbackInfo.time;
		this.transform.rotation = Quaternion.AngleAxis(180, Vector3.up) * knockbackInfo.direction;
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
				Util.ShowAttackWarning(gameMan, flashSpot.position);
				animationTimer = animationTimes["Enemy_Attack_Slash"];
				break;
			case Attack.Lunge:
				Util.ShowAttackWarning(gameMan, flashSpot.position);
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

	void OnTriggerEnter(Collider other) {
		if (!isImmune) {
			if (other.gameObject.layer == (int)Layers.PlayerHitbox) {
				HeadProjectile head = other.GetComponentInParent<HeadProjectile>();
				PlayerController player = other.GetComponentInParent<PlayerController>();
				ExplosiveTrap explosiveTrap = other.GetComponentInParent<ExplosiveTrap>();

				if (player != null) {
					bool fullAxe = player.meter >= player.meterMax/2;

					KnockbackInfo newKnockbackInfo = other.GetComponent<GetKnockbackInfo>().GetInfo(this.gameObject);
					gameMan.SpawnParticle(0, other.transform.position, 1f);
					switch (gameMan.playerController.currentAttack) {
						case PlayerController.Attacks.LAttack:
							if (fullAxe) {
								health -= 0.5f;
								ChangeDirective_Stunned(StunTime.ShortStun, newKnockbackInfo);
							}
							else {
								health -= 1;
								ChangeDirective_Stunned(StunTime.ShortStun, newKnockbackInfo);
							}
							gameMan.playerController.meter += partialMeter;
							break;

						case PlayerController.Attacks.LAttack2:
							if (fullAxe) {
								health -= 0.5f;
								ChangeDirective_Stunned(StunTime.ShortStun, newKnockbackInfo);
							}
							else {
								health -= 1;
								ChangeDirective_Stunned(StunTime.ShortStun, newKnockbackInfo);
							}
							gameMan.playerController.meter += partialMeter;
							break;

						case PlayerController.Attacks.LAttack3:
							if (fullAxe) {
								health -= 1;
								ChangeDirective_Stunned(StunTime.LongStun, newKnockbackInfo);
							}
							else {
								health -= 2;
								ChangeDirective_Stunned(StunTime.LongStun, newKnockbackInfo);
							}
							gameMan.playerController.meter += partialMeter * 2f;
							break;

						case PlayerController.Attacks.Spin:
							if (fullAxe) {
								health -= 1;
								ChangeDirective_Stunned(StunTime.ShortStun, newKnockbackInfo);
							}
							else {
								health -= 2;
								ChangeDirective_Stunned(StunTime.ShortStun, newKnockbackInfo);
							}
							break;

						case PlayerController.Attacks.LethalDash:
							if (fullAxe) {
								health -= 1;
								ChangeDirective_Stunned(StunTime.ShortStun, newKnockbackInfo);
							}
							else {
								health -= 2;
								ChangeDirective_Stunned(StunTime.ShortStun, newKnockbackInfo);
							}
							break;

						case PlayerController.Attacks.Slam:
							float posDifference = Mathf.Abs((player.transform.position - transform.position).sqrMagnitude);
							Debug.Log(gameObject.name + "'s posDifference after slam: " + posDifference);
							if (posDifference < 40f) {
								shouldDie = true;
							} 
							else if (posDifference < 80f) {
								if (fullAxe) {
									health -= 2;
									ChangeDirective_Stunned(StunTime.LongStun, newKnockbackInfo);
								}
								else {
									health -= 4;
									ChangeDirective_Stunned(StunTime.LongStun, newKnockbackInfo);
								}
							} 
							else {
								ChangeDirective_Stunned(StunTime.LongStun, newKnockbackInfo);
							}
							break;

						case PlayerController.Attacks.Chop:
							shouldDie = true;
							player.ChangeMeter(1);
							break;

						default:
							Debug.Log("I, " + this.name + " was hit by an unhandled attack (" + gameMan.playerController.currentAttack + ")");
							break;
					}
				}
				else if (head != null) {
					// NOTE(Roskuski): Head projectial direct hit
					gameMan.SpawnParticle(0, other.transform.position, 2f);
					shouldDie = true;
				}
				else if (explosiveTrap != null) {
					// NOTE(Roskuski): Knockback trap
					KnockbackInfo newKnockbackInfo = other.GetComponent<GetKnockbackInfo>().GetInfo(this.gameObject);
					ChangeDirective_Stunned(StunTime.LongStun, newKnockbackInfo);
				}
			}
			else if (other.gameObject.layer == (int)Layers.AgnosticHitbox) {
				if (other.GetComponentInParent<Exploding>() != null) {
					// NOTE(Roskuski): Explosive enemy
					KnockbackInfo newKnockbackInfo = other.GetComponent<GetKnockbackInfo>().GetInfo(this.gameObject);
					ChangeDirective_Stunned(StunTime.LongStun, newKnockbackInfo);
					health -= health;
				}
			}

		}
	}

	void Start() {
		navAgent = this.GetComponent<NavMeshAgent>();
		animator = this.GetComponent<Animator>();
		swordHitbox = transform.Find("Weapon_Controller").GetComponent<BoxCollider>();
		enemyCommunication = this.GetComponent<EnemyCommunication>();
		flashSpot = transform.Find("Weapon_Controller");

		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();

		navAgent.updatePosition = false;
		navAgent.updateRotation = false;

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
	}

	void Update() {
		Vector3 playerPosition = gameMan.player.position;
		Quaternion playerRotation = gameMan.player.rotation;
		Vector3 deltaToPlayer = gameMan.player.position - this.transform.position;
		float distanceToPlayer = Vector3.Distance(this.transform.position, gameMan.player.position);

		hitflashTimer -= Time.deltaTime;
		Material[] materialList = model.materials;
		for (int i = 0; i < materialList.Length; i++) {
			if (hitflashTimer > 0) {
				materialList[i] = hitflashMat;
			}
			else {
				materialList[i] = materials[i];
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

		Vector3 movementDelta = Vector3.zero; 
		// Preform knockback regardless of what we want to do
		if (remainingKnockbackTime > 0) {
			remainingKnockbackTime -= Time.deltaTime;
			movementDelta += knockbackInfo.direction * Vector3.forward * knockbackInfo.force * Mathf.Lerp(1, 0, Mathf.Clamp01(Mathf.Pow((remainingKnockbackTime/knockbackInfo.time), 2))) * Time.deltaTime;
		}

		// Directive Changing
		switch (directive) {
			case Directive.Inactive: // using this as a generic start point for enemy AI
				inactiveWait -= Time.deltaTime;
				if (inactiveWait < 0) {
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
						transform.position += this.transform.rotation * Vector3.forward * spawnLateralSpeed * Time.deltaTime;
					}
					
					// @TODO(Roskuski): Blend between up and down?
 
					// Between frames 10, 16, move upward
					if (animationTimerRatio > 0.1785f && animationTimerRatio < 0.2857f) {
						transform.position += this.transform.rotation * Vector3.up * spawnUpwardsSpeed * Time.deltaTime;
					}
					// Between frames 17, 24, move downward, hit the floor
					if (animationTimerRatio > 0.3035f && animationTimerRatio < 0.4285f) {
						transform.position += this.transform.rotation * Vector3.down * spawnDownwardsSpeed * Time.deltaTime;
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
					// @TODO(Roskuski): is this a good way of determining if mInput is not inputting stuff?
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

				movementDelta += moveDirection * Vector3.forward * MoveSpeed * speedModifier * Time.deltaTime;
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
								movementDelta += this.transform.rotation * Vector3.forward * 15f * Time.deltaTime * Mathf.Lerp(0.25f, 1f, (animationTimerRatio - 0.25f) / (0.5f - 0.25f));
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
								movementDelta += (this.transform.rotation * Vector3.forward) * LungeSpeed * Time.deltaTime;
							}

							if (animationTimer < 0.0f) {
								// If we found ourselves off geometry, wait util we finish falling.
								ChangeDirective_Inactive(1.0f);
								navAgent.enabled = true;
							}
							break;
						}
				}
				break;
			default: Debug.Assert(false); break;
		}

		Util.PerformCheckedLateralMovement(this.gameObject, 0.75f, 0.5f, movementDelta, ~0);

		if (directive != Directive.Spawn) {
			Util.PerformCheckedVerticalMovement(this.gameObject, 0.75f, 0.2f, 0.5f, 30.0f);
		}

		animator.SetInteger("Ai Directive", (int)directive);
		animationTimer -= Time.deltaTime * animator.GetCurrentAnimatorStateInfo(0).speed;

		if (health <= 0) {
			shouldDie = true;
		}

		if (transform.position.y <= -50f) {
			shouldDie = true;
		}

		if (shouldDie) {
			Vector3 spawnPos = transform.position + 3 * Vector3.up;
			if (gameMan.playerController.currentAttack != PlayerController.Attacks.Chop) gameMan.DeterminePickups(spawnPos);
			Destroy(this.gameObject); 
		}
	}

	private void OnDestroy() { 
		gameMan.enemiesAlive -= 1;
		gameMan.enemiesKilledInLevel += 1;
		GameManager.enemiesKilledInRun += 1;
		Initializer.save.versionLatest.basicEnemyKills++;
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
