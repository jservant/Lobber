using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Movement Patterns
 * - Prioty, maintain LoS while also being at the same elevation of the player
 * Create "floor" volumes That dictate it's minium height, pair that with trying to go to the same height as the player?
 */

public class Necro : MonoBehaviour {
	enum Directive {
		Spawn = 0, 
		Attack,
		Wander,
		Death,
		Stunned,
	}
	[SerializeField] Directive directive;

	enum Attack {
		None = 0,
		// Launch a slow moving homing projectile 
		Projectile,
	}
	Attack currentAttack = Attack.None;
	float attackDelay = 0;
	const float ReferenceAttackDelay = 7f;

	Vector3 attackTarget;

	const float MoveSpeed = 7.5f;
	const float VerticalCorrectSpeed = 1.0f;
	const float MoveTimeMax = 3f;
	const float TurnSpeed = 360f / 1f;

	float flankStrength = 0;
	bool preferRightStrafe = false;
	const float ReferenceComfortableDistance = 15f;
	float comfortableDistance = 15f;

	Vector3 movementDelta;
	Quaternion moveDirection;
	float[] directionWeights = new float[32];

	[SerializeField] Material hitflashMat;
	Material[] materials;
	float hitflashTimer;
	SkinnedMeshRenderer model;

	[SerializeField] float health;
	bool shouldDie = false;
	bool isImmune = false;
	bool wasHitByChop = false;

	float stunDuration = 0f;
	KnockbackInfo knockbackInfo;
	float remainingKnockbackTime;

	// NOTE(Roskuski): Internal references
	Animator animator;
	Transform ProjectileSpawnPoint;
	GameObject Projectile;

	// NOTE(Roskuski): External references
	GameManager gameMan;
	private MotionAudio_Necro sounds;

	void ChangeDirective_Spawn() {
		// Why would we ever need to go back to spawn.
		Debug.Assert(false, "Necro:ChangeDirective_Spawn()");
	}

	void ChangeDirective_Wander() {
		if (directive != Directive.Death) {
			directive = Directive.Wander;
			currentAttack = Attack.None;
			this.attackDelay = Random.Range(ReferenceAttackDelay * 0.8f, ReferenceAttackDelay * 1.2f);
		}
	}

	void ChangeDirective_Attack(Attack attack) {
		if (directive != Directive.Death) {
			directive = Directive.Attack;
			currentAttack = attack;

			switch (currentAttack) {
				default: 
					Debug.Assert(false, "Invalid Attack " + currentAttack, this);
					break;

				case Attack.Projectile:
					break;
			}
		}
	}

	public void ChangeDirective_Stunned(StunTime stunTime, KnockbackInfo newKnockbackInfo) {
		if (directive != Directive.Death && directive != Directive.Spawn && stunTime != StunTime.None) {
			directive = Directive.Stunned;
			
			float stunValue = 0;
			switch (stunTime) {
				case StunTime.Short:
					stunValue = 0.5f;
					break;

				case StunTime.Long:
					stunValue = Random.Range(2f, 2.2f);
					break;

				case StunTime.None:
				default:
					Debug.Assert(false);
					break;
			}

			stunDuration = stunValue;
			hitflashTimer = 0.25f;

			knockbackInfo = newKnockbackInfo;
			remainingKnockbackTime = knockbackInfo.time;
			this.transform.rotation = Quaternion.AngleAxis(180, Vector3.up) * knockbackInfo.direction;

			if (Projectile != null) {
				Destroy(Projectile);
				Projectile = null;
			}
		}
	}
	
	void ChangeDirective_Death() {
		if (directive != Directive.Death) {
			directive = Directive.Death;
			currentAttack = Attack.None;
			gameObject.layer = (int)Layers.Corpses;

			if (Projectile != null) {
				Destroy(Projectile);
				Projectile = null;
			}
		}
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
		if (!isImmune && directive != Directive.Death) {
			KnockbackInfo newKnockbackInfo = new KnockbackInfo(Quaternion.identity, 0, 0);
			StunTime stunTime = StunTime.None;
			float damage = 0f;
			float meterGain = 0f;

			if (other.gameObject.layer == (int)Layers.PlayerHitbox) {
				HeadProjectile head = other.GetComponentInParent<HeadProjectile>();
				PlayerController player = other.GetComponentInParent<PlayerController>();
				ExplosiveTrap explosiveTrap = other.GetComponentInParent<ExplosiveTrap>();

				if (player != null) {
					gameMan.SpawnParticle(0, other.transform.position, 1f);
					sounds.Necro_GetHit();
					
					newKnockbackInfo = other.GetComponent<GetKnockbackInfo>().GetInfo(this.gameObject);
					stunTime = AttackStunTimeTable[(int)player.currentAttack];
					damage = PlayerController.AttackDamageTable[(int)player.currentAttack];
					meterGain = AttackMeterGainOnHitTable[(int)player.currentAttack];

					// Attack specific code
					switch (gameMan.playerController.currentAttack) {
						case PlayerController.Attacks.Slam:
							float posDifference = Mathf.Abs((player.transform.position - transform.position).sqrMagnitude);
							Debug.Log(gameObject.name + "'s posDifference after slam: " + posDifference);
							if (posDifference < 40f) {
								damage = 8f;
							} 
							else if (posDifference < 80f) {
								damage = 4f;
							} 
							break;

						case PlayerController.Attacks.Chop:
							// @TODO(Roskuski): Different System to prevent headpickup spawns from chop. this current system will not work well if we implment enemies with healthpools that can surrive a chop
							wasHitByChop = true;
							gameMan.ShakeCamera(5f, 0.1f);
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
					damage = 8f;
				}
				else if (explosiveTrap != null) {
					// NOTE(Roskuski): Knockback trap
					newKnockbackInfo = other.GetComponent<GetKnockbackInfo>().GetInfo(this.gameObject);
					stunTime = StunTime.Long;
				}
			}
			else if (other.gameObject.layer == (int)Layers.AgnosticHitbox) {
				if (other.GetComponentInParent<Exploding>() != null) {
					// NOTE(Roskuski): Explosive enemy
					newKnockbackInfo = other.GetComponent<GetKnockbackInfo>().GetInfo(this.gameObject);
					stunTime = StunTime.Long;
					newKnockbackInfo.force *= 2f;
					damage = 8f;
				}
			}

			health -= damage;
			gameMan.playerController.ChangeMeter(meterGain);
			ChangeDirective_Stunned(stunTime, newKnockbackInfo);

		}
	}

	void Start() {
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		ProjectileSpawnPoint = transform.Find("MAIN_JOINT/MidTorso_Joint/Chest_Joint/Neck_Joint/Head_Joint/Projectile Spawnpoint");
		animator = this.GetComponent<Animator>();
		sounds = GetComponent<MotionAudio_Necro>();
		model = transform.Find("Lil_Necromancer").GetComponent<SkinnedMeshRenderer>();
		materials = model.materials;

		comfortableDistance = Random.Range(ReferenceComfortableDistance - 2, ReferenceComfortableDistance + 2);
		directive = Directive.Spawn;
		gameMan.SpawnParticle(9, transform.position, 1f);
		isImmune = true;
	}

	void FixedUpdate() {
		if (directive != Directive.Spawn && directive != Directive.Death && directive != Directive.Stunned) {
			Vector3 deltaToPlayer = gameMan.player.position - this.transform.position;
			// NOTE(Roskuski): how close the current movement is to going straight towards the player.
			float directScore = Vector3.Dot(movementDelta.normalized, deltaToPlayer.normalized) + 1f;
			float speedModifer = 1f;
			if (directScore > 1.6f) {
				speedModifer = Mathf.Lerp(1.00f, 0.75f, (directScore - 1.6f) / (2.0f - 1.6f));
			}

			// NOTE(Roskuski): Copying the values from PlayerController, for now.
			Util.PerformCheckedLateralMovement(this.gameObject, 1.0f, 0.5f, movementDelta * speedModifer * Time.fixedDeltaTime, ~Mask.Get(new Layers[] {Layers.StickyLedge, Layers.Corpses}));
			this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(gameMan.player.position - this.transform.position, Vector3.up), TurnSpeed * Time.fixedDeltaTime);

			// NOTE(Roskuski): float to the same height as the player
			float verticalDeltaToPlayer = gameMan.player.position.y - this.transform.position.y;
			if (Mathf.Abs(verticalDeltaToPlayer) > 0.05f) {
				RaycastHit hitInfo;
				this.transform.position += new Vector3(0, Mathf.Sign(verticalDeltaToPlayer) * VerticalCorrectSpeed * Time.fixedDeltaTime, 0); 
				if (Physics.SphereCast(gameObject.transform.position + Vector3.up * 1.0f, 0.5f, Vector3.down, out hitInfo, 0.5f, Mask.Get(Layers.Ground)) && (hitInfo.collider.isTrigger == false)) {
					float distanceToGround = hitInfo.distance - 1.0f + 0.5f;
					gameObject.transform.position -= new Vector3(0, distanceToGround, 0);
				}
			}
		}
	}

	void Update() {
		Vector3 playerPosition = gameMan.player.position;
		Quaternion playerRotation = gameMan.player.rotation;
		Vector3 deltaToPlayer = gameMan.player.position - this.transform.position;
		float distanceToPlayer = Vector3.Distance(this.transform.position, gameMan.player.position);
		movementDelta = Vector3.zero;

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

		if (health <= 0) {
			ChangeDirective_Death();
		}

		switch (directive) {
			case Directive.Spawn:
				moveDirection = this.transform.rotation;
				if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) {
					ChangeDirective_Wander();
					isImmune = false;
				} 
				break;

			case Directive.Stunned:
				stunDuration -= Time.deltaTime;
				if (stunDuration < 0) {
					ChangeDirective_Wander();
					stunDuration = 0;
				}
				break;

			case Directive.Wander:
				// Choose Move direction
				Quaternion angleStep = Quaternion.AngleAxis(360.0f / directionWeights.Length, Vector3.up);
				bool isStrafing = false;

				{
					Vector3 consideredDelta = Vector3.forward;
					Vector3 desiredFlank = Quaternion.AngleAxis(flankStrength, Vector3.up) * deltaToPlayer.normalized;

					for (int index = 0; index < directionWeights.Length; index += 1) {
						float strafeAngle = preferRightStrafe ? 90 : -90;
						float strafeScore = Vector3.Dot(Quaternion.AngleAxis(strafeAngle, Vector3.up) * deltaToPlayer.normalized, consideredDelta) + 1;
						float backScore = Vector3.Dot(Quaternion.AngleAxis(180, Vector3.up) * deltaToPlayer.normalized, consideredDelta) + 1;
						float forwardScore = Vector3.Dot(deltaToPlayer.normalized, consideredDelta) + 1;

						float strafeWeight = Mathf.Lerp(2, 0, Mathf.Abs(comfortableDistance - distanceToPlayer) / 1);
						float backWeight = Mathf.Lerp(3, 0, distanceToPlayer/comfortableDistance);
						float forwardWeight = Mathf.Lerp(0, 1, ((distanceToPlayer/comfortableDistance) - 1));

						directionWeights[index] = (strafeScore * strafeWeight + backScore * backWeight + forwardScore * forwardWeight) / (strafeWeight + backWeight + forwardWeight);

						isStrafing = (strafeWeight / (strafeWeight + backWeight + forwardWeight)) >= 0.90f;

						// NOTE(Roskuski): Advance the angle to the next index.
						consideredDelta = angleStep * consideredDelta;
					}
				}

				// Consider Walls
				{
					int strafeIndex = -1;
					if (isStrafing) {
						float bestWeightTemp = -2;
						for (int index = 0; index < directionWeights.Length; index += 1) {
							if (directionWeights[index] > bestWeightTemp) {
								bestWeightTemp = directionWeights[index];
								strafeIndex = index;
							}
						}
					}

					Vector3 consideredDelta = Vector3.forward;
					for (int index = 0; index < directionWeights.Length; index += 1) {
						RaycastHit hitInfo;
						if (Physics.SphereCast(this.transform.position + Vector3.up * 0.75f, 0.5f, consideredDelta, out hitInfo, 1.5f, ~Mask.Get(new Layers[]{Layers.PlayerHitbox, Layers.EnemyHitbox, Layers.EnemyHurtbox, Layers.AgnosticHitbox, Layers.StickyLedge}))) {
							directionWeights[index] *= 0.25f;
							if (index == strafeIndex) {
								preferRightStrafe = !preferRightStrafe;
							}
						}

						// NOTE(Roskuski): Advance the angle to the next index.
						consideredDelta = angleStep * consideredDelta;
					}
				}

				// Swap strafing angle if our strafe is the same direction as the player
				{
					float strafeAngle = preferRightStrafe ? 90 : -90;

					// NOTE(Roskuski): How similar our strafing angle is to the player's current movement
					float strafeScore = Vector3.Dot(gameMan.playerController.movement.normalized, Quaternion.AngleAxis(strafeAngle, Vector3.up) * deltaToPlayer.normalized) + 1;
					if (strafeScore > 1.9f && gameMan.playerController.trueInput.magnitude > 0.1f) {
						preferRightStrafe = !preferRightStrafe;
					}
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

				moveDirection = Quaternion.RotateTowards(moveDirection, chosenAngle, TurnSpeed * Time.deltaTime);
				movementDelta += moveDirection * Vector3.forward * MoveSpeed;

				// Attack consideration
				attackDelay -= Time.deltaTime;
				if (attackDelay < 0 && distanceToPlayer > comfortableDistance - 5f && distanceToPlayer < comfortableDistance + 5f) {
					float[] attackWeights = new float[]{0.50f, 3f};
					Attack attackChoice = (Attack)Util.RollWeightedChoice(attackWeights);

					// @TODO(Roskuski): Removeme when we actually have the animations for summoning

					switch (attackChoice) {
						case Attack.None:
							attackDelay = Random.Range(ReferenceAttackDelay*0.3f, ReferenceAttackDelay*0.5f);
							break;

						case Attack.Projectile:
							ChangeDirective_Attack(Attack.Projectile);
							break;

						default:
							Debug.Assert(false, "Unknown Necro.Attack value " + attackChoice, this);
							break;
					}
				}

				break;

			case Directive.Attack:
				{
					AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
					switch(currentAttack) {
						case Attack.None:
						default:
							Debug.Assert(false, "Invalid currentAttack " + currentAttack, this);
							ChangeDirective_Wander();
							break;

						case Attack.Projectile:
							if (current.IsName("Base Layer.Throw") && current.normalizedTime >= 1f) {
								ChangeDirective_Wander();
							}
							break;
					}
				}
				break;

			case Directive.Death:
				{
					AnimatorStateInfo current = animator.GetCurrentAnimatorStateInfo(0);
					if (current.IsName("Base Layer.Death") && current.normalizedTime >= 1f) {
						GameObject.Destroy(this.gameObject);
					}
				}
				break;

			default:
				Debug.Assert(false);
				break;
		}

		animator.SetInteger("directive", (int)directive);
		animator.SetInteger("currentAttack", (int)currentAttack);
	}

	void AnimationClip_ReadyProjectile() {
		Projectile = Object.Instantiate(gameMan.NecroProjectilePrefab, ProjectileSpawnPoint);
	}

	void AnimationClip_LaunchProjectile() {
		Debug.Assert(ProjectileSpawnPoint != null);
		if (Projectile != null) {
			Projectile.transform.parent = null;
			Projectile = null;
		}
	}

    private void OnDestroy() {
		gameMan.enemiesAlive -= 1;
		gameMan.enemiesKilledInLevel += 1;
		GameManager.enemiesKilledInRun += 1;
		Initializer.save.versionLatest.necroEnemyKills++;
	}
}
