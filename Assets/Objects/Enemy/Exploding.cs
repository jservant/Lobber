using System.Collections;
using System.Collections.Generic;
using static System.Math;
using UnityEngine;
using UnityEngine.AI;

public class Exploding : MonoBehaviour {
	enum Directive {
		Spawn = 0,
		WaitForFuse,
		LaunchSelf,
		Explosion,
		Death,
	}
	Directive directive;

	public bool randomizeStats = true;

	float spawnDuration = 2.0f;

	[SerializeField] float fuseDuration = 0;
	public float lowFuseTime; //when to start flashing red
	float bombFlashWait = 1.0f; //how long to wait between flashes
	float bombFlashWaitTime = 0f;
	float waitDuration = 0;
	float movementBurstDuration = 0;
	Vector3 movementDelta;
	bool reevaluateMovement = false;

	float spawnLateralSpeed = 5f;
	float spawnUpwardsSpeed = 5f;
	float spawnDownwardsSpeed;

	KnockbackInfo knockbackInfo;
	float remainingKnockbackTime;

	float launchDuration = 0;
	Vector3 launchTarget;
	Vector3 launchInitalPosition;
	bool launchHasStarted = false;

	float explosionTimer = 0.1f;
	float explosionDelay = 0;

	[SerializeField] float hitflashTimer = 0f;
	SkinnedMeshRenderer handModel;
	MeshRenderer bombModel;
	Material[] bombMaterials;
	Material handMaterial;
	public Material hitflashMat;
	public Material bombFlashMat;

	Quaternion moveDirection;
	float[] directionWeights = new float[32];

	public float FollowingRadius;
	public float MoveSpeed;
	public float MovementBustLength;
	public float WaitLength;
	public float waitMultiplier; //how much faster it shortens its wait time as the player gets closer (2f = twice as fast)

	public bool shouldAddToKillTotal = true;
	public bool shouldDealDamage = true;

	readonly float LaunchHeight = 6.0f;
	readonly float LaunchLength = 1.0f;

	public float fuseMin; //minimum fuse duration
	public float fuseMax; //maximum fuse duration

	// Internal References
	NavMeshAgent navAgent;
	Animator animator;
	public Animator bombAnimator;
	EnemyCommunication enemyCommunication;
	CapsuleCollider explosionHitbox;
	CapsuleCollider selfHurtbox;
	Transform attackWarningTransform;
	Transform smokePoint;
	MotionAudio_Pest sounds;
	public GameObject groundIndicator;
	private GameObject groundIndicatorInstance;

	// External References
	GameManager gameMan;

	void ChangeDirective_Spawn() {
		Debug.Assert(false);
	}

	void ChangeDirective_WaitForFuse() {
		if (directive != Directive.Explosion) {
			directive = Directive.WaitForFuse;
		}
	}

	void ChangeDirective_LaunchSelf(Vector3 target) {
		if (directive != Directive.Explosion) {
			directive = Directive.LaunchSelf;
			launchDuration = LaunchLength;
			launchTarget = target;
			launchInitalPosition = this.transform.position;
			animator.SetTrigger("StartAttack");
			Util.SpawnFlash(gameMan, 0, attackWarningTransform.position, true);
			groundIndicatorInstance = Instantiate(groundIndicator, target, groundIndicator.transform.rotation);
		}
	}

	void ChangeDirective_Death() {
		if (directive != Directive.Death && directive != Directive.Explosion) {
			animator.SetTrigger("Dead");
			directive = Directive.Death;
			selfHurtbox.isTrigger = true;
			if (groundIndicatorInstance != null) Destroy(groundIndicatorInstance);
		}
	}

	public void ChangeDirective_Explosion(float delay = 0) {
		if (directive != Directive.Explosion) {
			directive = Directive.Explosion;
			selfHurtbox.isTrigger = true;
			explosionDelay = delay;
			if (groundIndicatorInstance != null) Destroy(groundIndicatorInstance);
		}
	}

	bool CanAttemptNavigation() {
		return (navAgent.pathStatus == NavMeshPathStatus.PathComplete || navAgent.pathStatus == NavMeshPathStatus.PathPartial) && navAgent.path.corners.Length >= 2;
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHitbox) {
			PlayerController playerController = other.gameObject.GetComponentInParent<PlayerController>();
			if (playerController) {
				ChangeDirective_Death();
				sounds.PestBombHit();
				if (playerController.currentAttack == PlayerController.Attacks.Chop) {
					ChangeDirective_Explosion();
				}

				GetKnockbackInfo getKnockbackInfo = other.gameObject.GetComponent<GetKnockbackInfo>();
				if (getKnockbackInfo != null) {
					knockbackInfo = getKnockbackInfo.GetInfo(this.gameObject);
					remainingKnockbackTime = knockbackInfo.time;
				}
			}
			else {
				if (other.CompareTag("Stun") == false) ChangeDirective_Explosion();
			}
			if (other.CompareTag("Stun") == false) {
				fuseDuration = 0;
				hitflashTimer = 0.25f;
				Vector3 spawnPoint = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
				gameMan.SpawnParticle(13, spawnPoint, 1f);
			}
			else {
				if (other.GetComponent<StunSphere>().damage > 0f) ChangeDirective_Explosion();
            }
		}
		else if (other.gameObject.layer == (int)Layers.AgnosticHitbox) {
				fuseDuration = 0f;
				hitflashTimer = 0.25f;
				float randomDelay = Random.Range(0.1f, 0.3f);
				ChangeDirective_Explosion(randomDelay);
		}
	}

	void Start() {
		navAgent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();
		enemyCommunication = GetComponent<EnemyCommunication>();
		explosionHitbox = transform.Find("Hitbox").GetComponent<CapsuleCollider>();
		selfHurtbox = GetComponent<CapsuleCollider>();
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		attackWarningTransform = transform.Find("Main/MrBomb");
		sounds = GetComponent<MotionAudio_Pest>();
		smokePoint = transform.Find("SmokePoint");

		navAgent.updatePosition = false;
		navAgent.updateRotation = false;

		directive = Directive.Spawn;

		if (randomizeStats) {
			fuseDuration = Random.Range(fuseMin, fuseMax);
		}

		handModel = transform.Find("Skeleton_Bombois").GetComponent<SkinnedMeshRenderer>();
		bombModel = transform.Find("Main/MrBomb").GetComponent<MeshRenderer>();
		handMaterial = handModel.material;
		bombMaterials = bombModel.materials;

		sounds.PestBombFuse();
	}

	void FixedUpdate() {
		if (directive == Directive.Spawn) {
			this.transform.position += movementDelta * Time.fixedDeltaTime;
		}

		if (directive == Directive.WaitForFuse || directive == Directive.Death || directive == Directive.Explosion) {
			int layerMask = ~Mask.Get(new Layers[] {Layers.EnemyHitbox, Layers.Corpses});

			if (remainingKnockbackTime > 0) {
				layerMask &= ~Mask.Get(Layers.StickyLedge);
			}

			reevaluateMovement = Util.PerformCheckedLateralMovement(this.gameObject, 0.75f, 0.5f, movementDelta * Time.fixedDeltaTime, layerMask);
			Util.PerformCheckedVerticalMovement(this.gameObject, 0.75f, 0.2f, 0.5f, 30.0f);
		}

		{
			// Between frames 10, 16, move upward
			// Between frames 17, 24, move downward, hit the floor
			float lateralTime = (2.0f - 1.2f);
			float upwardsTime = (2.0f - 1.6f);
			float downwardsTime = (1.6f - 1.2f);

			RaycastHit hitInfo;
			// NOTE(Roskuski): I'm not certain why we need to rotate Vector3.back to test in the correct direction here. Vector3.forward results in testing in the direction behind the skeleton.
			if (!Physics.Raycast(this.transform.position + this.transform.rotation * Vector3.back * spawnLateralSpeed * lateralTime, Vector3.down, out hitInfo, Mathf.Infinity, Mask.Get(Layers.Ground))) {
				Physics.Raycast(this.transform.position, Vector3.down, out hitInfo, Mathf.Infinity, Mask.Get(Layers.Ground));
				spawnLateralSpeed = 0;
			}
			spawnDownwardsSpeed = (spawnUpwardsSpeed * upwardsTime + hitInfo.distance) / downwardsTime;
		}
	} 

	void Update() {
		Vector3 playerPosition = gameMan.player.position;
		Quaternion playerRotation = gameMan.player.rotation;
		Vector3 deltaToPlayer = gameMan.player.position - this.transform.position;
		float distanceToPlayer = Vector3.Distance(this.transform.position, gameMan.player.position);
		movementDelta = Vector3.zero;

		animator.SetBool("IsMoving", false);

		//Bombflash
		if (fuseDuration <= lowFuseTime) {
			if (fuseDuration <= lowFuseTime / 2f) bombFlashWait = 0.6f;
			if (fuseDuration <= lowFuseTime / 4f) bombFlashWait = 0.4f;

			if (bombFlashWaitTime <= 0) {
				bombAnimator.Play("Mr_Bomb_Pulse");
				bombFlashWaitTime = bombFlashWait;
			}
			bombFlashWaitTime -= Time.deltaTime;
		}
        
		//Hitflash
		hitflashTimer -= Time.deltaTime;
		Material[] bombMaterialList = bombModel.materials;
		for (int i = 0; i < bombMaterialList.Length; i++) {
			if (hitflashTimer > 0) {
				bombMaterialList[i] = hitflashMat;
				handModel.material = hitflashMat;
			}
			else {
				bombMaterialList[i] = bombMaterials[i];
				handModel.material = handMaterial;
			}
		}
		bombModel.materials = bombMaterialList;

		movementDelta += Util.ProcessKnockback(ref remainingKnockbackTime, knockbackInfo);

		// Processing information from other enemies
		switch (directive) {
			case Directive.Spawn: 
				spawnDuration -= Time.deltaTime;
				if (spawnDuration < 0) {
					ChangeDirective_WaitForFuse();
					RaycastHit hitInfo;
					if (Physics.Raycast(this.transform.position + Vector3.up * 2.5f, Vector3.down, out hitInfo, 2.6f, Mask.Get(Layers.Ground))) {
						this.transform.position += Vector3.down * (hitInfo.distance - 2.6f); // NOTE(Roskuski): should dislodge skeletons from the ground if frame lag causes them to sink inwards.
					}
				}

				if (spawnDuration < 2f && spawnDuration > 1.2f) {
					movementDelta += this.transform.rotation * Vector3.forward * spawnLateralSpeed;
				}

				if (spawnDuration < 1.4f) {
					animator.SetTrigger("SpawnMoveEnd");
				}

				if (spawnDuration < 2f && spawnDuration > 1.6f) {
					movementDelta += Vector3.up * spawnUpwardsSpeed;
					animator.SetBool("IsRising", true);
				}

				if (spawnDuration < 1.6f && spawnDuration > 1.2f) {
					movementDelta += Vector3.down * spawnDownwardsSpeed;
					animator.SetBool("IsRising", false);
				}

				break;

			case Directive.WaitForFuse:
				fuseDuration -= Time.deltaTime;
				if (fuseDuration < 0 && movementBurstDuration < 0.0f) {
					ChangeDirective_LaunchSelf(playerPosition);
					break;
				}

				navAgent.nextPosition = this.transform.position;
				navAgent.SetDestination(playerPosition);

				if (movementBurstDuration > 0.0f) {
					animator.SetBool("IsMoving", true);
					movementBurstDuration -= Time.deltaTime;

					NavMeshHit hit;
					Vector3 testDelta = moveDirection * Vector3.forward * MoveSpeed * Time.deltaTime;
					if (!NavMesh.SamplePosition(this.transform.position + testDelta, out hit, 0.35f, NavMesh.AllAreas)) {
						reevaluateMovement = true;
					}

					if (!reevaluateMovement) {
						movementDelta += moveDirection * Vector3.forward * MoveSpeed;
						this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, moveDirection, 360 * 2 * Time.deltaTime);
					}

					if (movementBurstDuration <= 0.0f) {
						waitDuration = WaitLength + Random.Range(-0.2f, 0.4f);
					}
				}

				// Choose Move direction
				if (CanAttemptNavigation() && ((reevaluateMovement) || ((movementBurstDuration <= 0.0f) && (waitDuration <= 0.0f)))) {
					if (!reevaluateMovement) {
						movementBurstDuration = MovementBustLength;
					}

					reevaluateMovement = false;
					bool withinTargetRange = navAgent.remainingDistance <= FollowingRadius;
					Vector3 targetDelta = Vector3.zero;

					targetDelta = Vector3.Normalize(navAgent.path.corners[1] - navAgent.path.corners[0]);
					Quaternion angleStep = Quaternion.AngleAxis(360.0f / directionWeights.Length, Vector3.up);

					// @TODO(Roskuski): Make Expoding willing to take low choices
					{
						Vector3 consideredDelta = Vector3.forward;

						for (int index = 0; index < directionWeights.Length; index += 1) {
							if (withinTargetRange) {
								float rightScore = Vector3.Dot(Quaternion.AngleAxis(90, Vector3.up) * targetDelta, consideredDelta) + 1.0f;
								float leftScore = Vector3.Dot(Quaternion.AngleAxis(-90, Vector3.up) * targetDelta, consideredDelta) + 1.0f;
								float maxSidewaysScore = Mathf.Max(rightScore, leftScore);
								float backScore = Vector3.Dot(Quaternion.AngleAxis(180, Vector3.up) * targetDelta, consideredDelta) + 1.0f;
								directionWeights[index] =
									(maxSidewaysScore * Mathf.Lerp(0.25f, 0.90f, navAgent.remainingDistance/FollowingRadius) +
									backScore * Mathf.Lerp(0.75f, 0.10f, navAgent.remainingDistance/FollowingRadius));
							}
							else {
								directionWeights[index] = Vector3.Dot(targetDelta, consideredDelta) + 1.0f;
							}

							// NOTE(Roskuski): Advance the angle to the next index.
							consideredDelta = angleStep * consideredDelta;
						}
					}

					{
						Vector3 consideredDelta = Vector3.forward;
						for (int index = 0; index < directionWeights.Length; index += 1) {
							NavMeshHit hit;
							if (!NavMesh.SamplePosition(this.transform.position + consideredDelta, out hit, 0.35f, NavMesh.AllAreas)) {
								directionWeights[index] = 0;
							}

							// NOTE(Roskuski): Advance the angle to the next index.
							consideredDelta = angleStep * consideredDelta;
						}
					}

					float maxWeight = 0;
					for (int index = 0; index < directionWeights.Length; index += 1) {
						if (maxWeight < directionWeights[index]) {
							maxWeight = directionWeights[index];
						}
					}

					float totalWeight = 0;
					float minimumConsideredWeight = maxWeight - 0.30f;
					if (maxWeight >= 0.30f) {
						minimumConsideredWeight = 0.001f;
					}
					for (int index = 0; index < directionWeights.Length; index += 1) {
						if (directionWeights[index] <= maxWeight - 0.30f ) { // discard the bottom 85 percent
							directionWeights[index] = 0;
						}
						totalWeight += directionWeights[index];
					}

					float directionRoll = Random.Range(0.0f, totalWeight);
					float rollingTotal = 0;
					Quaternion resultAngle = Quaternion.identity;
					for (int index = 0; index < directionWeights.Length; index += 1) {
						rollingTotal += directionWeights[index];
						if (directionRoll < rollingTotal) {
							break;
						}

						resultAngle *= angleStep;
					}

					moveDirection = resultAngle;
				}

				float waitModifier = Mathf.Lerp(waitMultiplier, 1, distanceToPlayer/FollowingRadius);
				if (waitDuration > 0.0f) {
					waitDuration -= Time.deltaTime * waitModifier;
				}

				break;

			case Directive.LaunchSelf:
				// wait for starting animation to end
				if (animator.GetCurrentAnimatorStateInfo(0).IsName("AttackStart")) {
					launchHasStarted = true;
				}

				this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(launchTarget - launchInitalPosition, Vector3.up), 360 * 2 * Time.deltaTime);

				if (launchHasStarted && !animator.GetCurrentAnimatorStateInfo(0).IsName("AttackStart")) {
					// Follow a parbola
					float arcOffset = Mathf.Lerp(0, LaunchHeight, -(Mathf.Pow((LaunchLength - launchDuration)/LaunchLength - 0.5f, 2) * 4) + 1);
					Vector3 targetOffset = (launchTarget - launchInitalPosition) * Mathf.Lerp(0, 1, (LaunchLength - launchDuration)/LaunchLength);

					animator.SetBool("IsRising", launchDuration > LaunchLength/2f);

					transform.position = launchInitalPosition + new Vector3(0, arcOffset, 0) + targetOffset;

					launchDuration -= Time.deltaTime;
				}
				if (launchDuration < 0) {
					ChangeDirective_Explosion();
				}
				break;

			case Directive.Death:
				if (animator.GetCurrentAnimatorStateInfo(0).IsName("Death") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) {
					ChangeDirective_Explosion();
				}
				break;

			case Directive.Explosion: 
				if (explosionDelay > 0) {
					explosionDelay -= Time.deltaTime;
				}
				else {
					if (shouldDealDamage) explosionHitbox.gameObject.SetActive(true);
					if (explosionTimer >= 0) {
						explosionTimer -= Time.deltaTime;
					}
					else {
						sounds.PestBombExplode();
						gameMan.SpawnParticle(3, transform.position, 1f);
						Vector3 flashSpot = new Vector3(transform.position.x, transform.position.y + 0.5f, transform.position.z);
						Util.SpawnFlash(gameMan, 3, flashSpot, false);
						gameMan.ShakeCamera(5f, 0.25f);
						Destroy(this.gameObject);
					}
				}
				break;

			default:
				Debug.Assert(false);
				break;
		}

		if (transform.position.y <= -50f) {
			Destroy(gameObject);
		}

	}

	public void SmokeParticleMedium() {
		gameMan.SpawnParticle(18, smokePoint.position, 1f);
    }

	private void OnDestroy() {
		if (shouldAddToKillTotal) {
			gameMan.enemiesAlive -= 1;
			gameMan.enemiesKilledInLevel += 1;
			gameMan.AddToKillStreak(1, 2f);
			GameManager.enemiesKilledInRun += 1;
			Initializer.save.versionLatest.explosiveEnemyKills++;
			if (groundIndicatorInstance != null) Destroy(groundIndicatorInstance);
			sounds.PestBombExplode();
		}
	}

	private void OnDrawGizmos() {
		Quaternion angleStep = Quaternion.AngleAxis(360.0f / directionWeights.Length, Vector3.up);
		Vector3 consideredDelta = Vector3.forward;

		for (int index = 0; index < directionWeights.Length; index += 1) {
			Gizmos.color = Color.Lerp(Color.red, Color.green, directionWeights[index]/2.0f);
			Gizmos.DrawRay(this.transform.position, consideredDelta);

			// NOTE(Roskuski): Advance the angle to the next index.
			consideredDelta = angleStep * consideredDelta;
		}
	}
}
