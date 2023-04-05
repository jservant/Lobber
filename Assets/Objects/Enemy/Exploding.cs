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
		Death,
	}
	[SerializeField] Directive directive;

	public bool randomizeStats = true;

	[SerializeField] float fuseDuration = 0;
	[SerializeField] float waitDuration = 0;
	[SerializeField] float movementBurstDuration = 0;
	[SerializeField] float spawnDuration = 2.0f;

	[SerializeField] float launchDuration = 0;
	[SerializeField] Vector3 launchTarget;
	[SerializeField] Vector3 launchInitalPosition;
	bool launchHasStarted = false;

	bool keepExplosionActive = false;

	Quaternion moveDirection;
	float[] directionWeights = new float[32];

	readonly float FollowingRadius = 17.0f;
	readonly float MoveSpeed = 12.0f * 2;
	readonly float MovementBustLength = 0.40f;
	readonly float WaitLength = 1.0f;

	readonly float LaunchHeight = 6.0f;
	readonly float LaunchLength = 1.0f;

	// Internal References
	NavMeshAgent navAgent;
	Animator animator;
	EnemyCommunication enemyCommunication;
	CapsuleCollider explosionHitbox;
	Transform attackWarningTransform;

	// External References
	GameManager gameMan;

	void ChangeDirective_Spawn() {
		Debug.Assert(false);
	}

	void ChangeDirective_WaitForFuse() {
		directive = Directive.WaitForFuse;
	}

	void ChangeDirective_LaunchSelf(Vector3 target) {
		directive = Directive.LaunchSelf;
		launchDuration = LaunchLength;
		launchTarget = target;
		launchInitalPosition = this.transform.position;
		animator.SetTrigger("StartAttack");
		Util.ShowAttackWarning(gameMan, attackWarningTransform.position);
	}

	void ChangeDirective_Death() {
		if (directive != Directive.Death) {
			animator.SetTrigger("Dead");
			directive = Directive.Death;
			explosionHitbox.gameObject.SetActive(true);
			keepExplosionActive = true;
		}
	}

	bool CanAttemptNavigation() {
		return (navAgent.pathStatus == NavMeshPathStatus.PathComplete || navAgent.pathStatus == NavMeshPathStatus.PathPartial) && navAgent.path.corners.Length >= 2;
	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHitbox) {
			ChangeDirective_Death();
		}
		else if (other.gameObject.layer == (int)Layers.AgnosticHitbox) {
			ChangeDirective_Death();
		}
	}

	void Start() {
		navAgent = GetComponent<NavMeshAgent>();
		animator = GetComponent<Animator>();
		enemyCommunication = GetComponent<EnemyCommunication>();
		explosionHitbox = transform.Find("Hitbox").GetComponent<CapsuleCollider>();
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		attackWarningTransform = transform.Find("Main/MrBomb (1)");

		navAgent.updatePosition = false;
		navAgent.updateRotation = false;

		directive = Directive.Spawn;

		if (randomizeStats) {
			fuseDuration = Random.Range(25.0f, 30.0f);
		}
	}

	void Update() {
		Vector3 playerPosition = gameMan.player.position;
		Quaternion playerRotation = gameMan.player.rotation;
		Vector3 deltaToPlayer = gameMan.player.position - this.transform.position;
		float distanceToPlayer = Vector3.Distance(this.transform.position, gameMan.player.position);

		animator.SetBool("IsMoving", false);

		// Processing information from other enemies
		switch (directive) {
			case Directive.Spawn: 
				spawnDuration -= Time.deltaTime;
				if (spawnDuration < 0) {
					ChangeDirective_WaitForFuse();
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

				bool reevaluateMovement = false;

				if (movementBurstDuration > 0.0f) {
					animator.SetBool("IsMoving", true);
					movementBurstDuration -= Time.deltaTime;
					// @TODO(Roskuski): Include ledge detection so we don't run off edges
					float hitDistance = 0;
					const int MaxQuantums = 4;

					for (int quantums = 1; quantums <= MaxQuantums; quantums += 1) {
						NavMeshHit hit;
						Vector3 testDelta = moveDirection * Vector3.forward * MoveSpeed * Time.deltaTime * (float)quantums;
						if (!NavMesh.SamplePosition(this.transform.position + testDelta, out hit, 0.2f, NavMesh.AllAreas)) {
							reevaluateMovement = true;
							break;
						}
					}

					if (!reevaluateMovement) {
						this.transform.position += moveDirection * Vector3.forward * MoveSpeed * Time.deltaTime;
					}

					if (movementBurstDuration <= 0.0f) {
						waitDuration = WaitLength + Random.Range(0.0f, 0.5f);
					}
				}

				// Choose Move direction
				if ((reevaluateMovement) || ((movementBurstDuration <= 0.0f) && (waitDuration <= 0.0f) && CanAttemptNavigation())) {
					bool withinTargetRange = navAgent.remainingDistance <= FollowingRadius;
					Vector3 targetDelta = Vector3.zero;

					targetDelta = Vector3.Normalize(navAgent.path.corners[1] - navAgent.path.corners[0]);
					Quaternion angleStep = Quaternion.AngleAxis(360.0f / directionWeights.Length, Vector3.up);

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

					float totalWeight = 0;
					for (int index = 0; index < directionWeights.Length; index += 1) {
						if (directionWeights[index] <= 1.7f) {
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
					if (!reevaluateMovement) { 
						movementBurstDuration = MovementBustLength;
					}
				}

				if (waitDuration > 0.0f) {
					waitDuration -= Time.deltaTime;
				}

				break;

			case Directive.LaunchSelf:
				// wait for starting animation to end
				if (animator.GetCurrentAnimatorStateInfo(0).IsName("AttackStart")) {
					launchHasStarted = true;
				}

				if (launchHasStarted && !animator.GetCurrentAnimatorStateInfo(0).IsName("AttackStart")){
					// Follow a parbola
					float arcOffset = Mathf.Lerp(0, LaunchHeight, -(Mathf.Pow((LaunchLength - launchDuration)/LaunchLength - 0.5f, 2) * 4) + 1);
					Vector3 targetOffset = (launchTarget - launchInitalPosition) * Mathf.Lerp(0, 1, (LaunchLength - launchDuration)/LaunchLength);

					animator.SetBool("IsRising", launchDuration > LaunchLength/2f);

					transform.position = launchInitalPosition + new Vector3(0, arcOffset, 0) + targetOffset;

					launchDuration -= Time.deltaTime;
				}
				if (launchDuration < 0) {
					ChangeDirective_Death();
				}
				break;

			case Directive.Death:
				if (!keepExplosionActive) {
					explosionHitbox.gameObject.SetActive(false);
				}
				else {
					keepExplosionActive = false;
				}

				if (animator.GetCurrentAnimatorStateInfo(0).IsName("Death") && animator.GetCurrentAnimatorStateInfo(0).normalizedTime >= 1f) {
					Destroy(this.gameObject);
				}
				break;

			default:
				Debug.Assert(false);
				break;
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
