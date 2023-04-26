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
	Directive directive;

	enum Attack {
		None = 0,
		Projectile,
		Summon,
	}
	Attack currentAttack = Attack.None;

	const float MoveSpeed = 7.5f;
	const float VerticalCorrectSpeed = 1.0f;
	const float MoveTimeMax = 3f;
	const float TurnSpeed = 360f / 1f;

	float flankStrength = 0;
	bool preferRightStrafe = false;
	float comfortableDistance = 15f;

	Vector3 movementDelta;
	Quaternion moveDirection;
	float[] directionWeights = new float[32];
	float moveTime = 0;

	// NOTE(Roskuski): Internal references
	Animator animator;

	// NOTE(Roskuski): External references
	GameManager gameMan;

	void ChangeDirective_Spawn() {
		// Why would we ever need to go back to spawn.
		Debug.Assert(false, "Necro:ChangeDirective_Spawn()");
	}

	void ChangeDirective_Wander() {
		directive = Directive.Wander;
	}

	void ChangeDirective_Attack() {
		directive = Directive.Attack;
	}
	
	void ChangeDirective_Death() {
		directive = Directive.Death;
	}

	void Start() {
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		animator = this.GetComponent<Animator>();

		directive = Directive.Spawn;
	}

	void FixedUpdate() {
		// NOTE(Roskuski): Copying the values from PlayerController, for now.
		Util.PerformCheckedLateralMovement(this.gameObject, 1.0f, 0.5f, movementDelta * Time.fixedDeltaTime, ~Mask.Get(Layers.StickyLedge));
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

		moveTime -= Time.deltaTime;

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

						float strafeWeight = Mathf.Lerp(1, 0, Mathf.Abs(comfortableDistance - distanceToPlayer) / 1);
						float backWeight = Mathf.Lerp(2, 0, distanceToPlayer/comfortableDistance);
						float forwardWeight = Mathf.Lerp(0, 2, ((distanceToPlayer/comfortableDistance) - 1));

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
				break;

			case Directive.Attack:
				break;

			default:
				Debug.Assert(false);
				break;
		}

		animator.SetInteger("directive", (int)directive);
		animator.SetInteger("attack", (int)currentAttack);


	}
}
