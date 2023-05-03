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
	}
	[SerializeField] Directive directive;

	enum Attack {
		None = 0,
		// Launch a slow moving homing projectile 
		Projectile,
		// Summon a portal and call forth enemies
		Summon,
	}
	[SerializeField] Attack currentAttack = Attack.None;
	[SerializeField] float attackDelay = 0;
	[SerializeField] const float ReferenceAttackDelay = 7f;

	[SerializeField] Vector3 attackTarget;

	[SerializeField] const float MoveSpeed = 7.5f;
	[SerializeField] const float VerticalCorrectSpeed = 1.0f;
	[SerializeField] const float MoveTimeMax = 3f;
	[SerializeField] const float TurnSpeed = 360f / 1f;

	[SerializeField] float flankStrength = 0;
	[SerializeField] bool preferRightStrafe = false;
	const float ReferenceComfortableDistance = 15f;
	[SerializeField] float comfortableDistance = 15f;

	[SerializeField] Vector3 movementDelta;
	[SerializeField] Quaternion moveDirection;
	[SerializeField] float[] directionWeights = new float[32];

	// NOTE(Roskuski): Internal references
	Animator animator;
	Transform ProjectileSpawnPoint;
	GameObject Projectile;

	// NOTE(Roskuski): External references
	GameManager gameMan;

	void ChangeDirective_Spawn() {
		// Why would we ever need to go back to spawn.
		Debug.Assert(false, "Necro:ChangeDirective_Spawn()");
	}

	void ChangeDirective_Wander() {
		directive = Directive.Wander;
		currentAttack = Attack.None;
		this.attackDelay = Random.Range(ReferenceAttackDelay * 0.8f, ReferenceAttackDelay * 1.2f);
	}

	void ChangeDirective_Attack(Attack attack) {
		directive = Directive.Attack;
		currentAttack = attack;

		switch (currentAttack) {
			default: 
				Debug.Assert(false, "Invalid Attack " + currentAttack, this);
				break;

			case Attack.Projectile:
				break;

			case Attack.Summon:
				attackTarget = Vector3.zero;
				for (int count = 0; count < 10; count += 1) {
					Vector3 test = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward * Random.Range(5f, 10f);
					if (Physics.Raycast(test, Vector3.down, 10f, Mask.Get(Layers.Ground))) { // if valid
						attackTarget = test;
						break;
					}
				}

				if (attackTarget == Vector3.zero) {
					ChangeDirective_Wander(); // Failed to find a suitable place to spawn in.
				}
				break;
		}
	}
	
	void ChangeDirective_Death() {
		directive = Directive.Death;
		currentAttack = Attack.None;
	}

	void Start() {
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		ProjectileSpawnPoint = transform.Find("MAIN_JOINT/MidTorso_Joint/Chest_Joint/Neck_Joint/Head_Joint/Projectile Spawnpoint");
		animator = this.GetComponent<Animator>();
		comfortableDistance = Random.Range(ReferenceComfortableDistance - 2, ReferenceComfortableDistance + 2);

		directive = Directive.Spawn;
		gameMan.SpawnParticle(9, transform.position, 1f);
	}

	void FixedUpdate() {
		Vector3 deltaToPlayer = gameMan.player.position - this.transform.position;
		// NOTE(Roskuski): how close the current movement is to going straight towards the player.
		float directScore = Vector3.Dot(movementDelta.normalized, deltaToPlayer.normalized) + 1f;
		float speedModifer = 1f;
		if (directScore > 1.6f) {
			speedModifer = Mathf.Lerp(1.00f, 0.75f, (directScore - 1.6f) / (2.0f - 1.6f));
		}

		// NOTE(Roskuski): Copying the values from PlayerController, for now.
		Util.PerformCheckedLateralMovement(this.gameObject, 1.0f, 0.5f, movementDelta * speedModifer * Time.fixedDeltaTime, ~Mask.Get(Layers.StickyLedge));
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

	void Update() {
		Vector3 playerPosition = gameMan.player.position;
		Quaternion playerRotation = gameMan.player.rotation;
		Vector3 deltaToPlayer = gameMan.player.position - this.transform.position;
		float distanceToPlayer = Vector3.Distance(this.transform.position, gameMan.player.position);
		movementDelta = Vector3.zero;

		switch (directive) {
			case Directive.Spawn:
				moveDirection = this.transform.rotation;
				if (animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) {
					ChangeDirective_Wander();
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
					float[] attackWeights = new float[]{0.50f, 3f, 0f};
					attackWeights[(int)Attack.Summon] = Mathf.Lerp(2f, 0.1f, gameMan.enemiesAlive/GameManager.HighEnemies);
					Attack attackChoice = (Attack)Util.RollWeightedChoice(attackWeights);

					// @TODO(Roskuski): Removeme when we actually have the animations for summoning
					attackWeights[(int)Attack.Summon] = 0;

					switch (attackChoice) {
						case Attack.None:
							attackDelay = Random.Range(ReferenceAttackDelay*0.3f, ReferenceAttackDelay*0.5f);
							break;

						case Attack.Projectile:
							ChangeDirective_Attack(Attack.Projectile);
							break;

						case Attack.Summon:
							ChangeDirective_Attack(Attack.Summon);
							break;

						default:
							Debug.Assert(false, "Unknown Necro.Attack value " + attackChoice, this);
							break;
					}
				}

				break;

			case Directive.Attack:
				AnimatorStateInfo Current = animator.GetCurrentAnimatorStateInfo(0);
				switch(currentAttack) {
					case Attack.None:
					default:
						Debug.Assert(false, "Invalid currentAttack " + currentAttack, this);
						ChangeDirective_Wander();
						break;

					case Attack.Projectile:
						if (Current.IsName("Base Layer.Throw") && Current.normalizedTime >= 1f) {
							ChangeDirective_Wander();
						}
						break;

					case Attack.Summon:
						break;
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
		Debug.Assert(Projectile != null);
		Projectile.transform.parent = null;
		Projectile = null;
	}

	void AnimationClip_OpenPortal() {
	}

	void ANimationClip_ClosePortal() {
	}
}
