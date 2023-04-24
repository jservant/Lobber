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
	}
	Directive directive;

	const float MoveSpeed = 7.5f;
	const float MoveTimeMax = 3f;
	const float TurnSpeed = 360f / 1f;

	float flankStrength = 0;
	bool preferRightStrafe = false;
	float comfortableDistance = 15f;

	Vector3 movementDelta;
	Quaternion moveDirection;
	float[] directionWeights = new float[32];
	float moveTime = 0;

	// NOTE(Roskuski): External references
	GameManager gameMan;

	void Start() {
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
	}

	void FixedUpdate() {
		this.transform.position += movementDelta * Time.fixedDeltaTime;
		this.transform.rotation = Quaternion.RotateTowards(this.transform.rotation, Quaternion.LookRotation(gameMan.player.position - this.transform.position, Vector3.up), TurnSpeed * Time.fixedDeltaTime);
	}

	void Update() {
		Vector3 playerPosition = gameMan.player.position;
		Quaternion playerRotation = gameMan.player.rotation;
		Vector3 deltaToPlayer = gameMan.player.position - this.transform.position;
		float distanceToPlayer = Vector3.Distance(this.transform.position, gameMan.player.position);
		movementDelta = Vector3.zero;

		moveTime -= Time.deltaTime;

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

		// Swap strafing angle if our strafe is the same direction as the player
		{
			float strafeAngle = preferRightStrafe ? 90 : -90;

			// NOTE(Roskuski): How similar our strafing angle is to the player's current movement
			float strafeScore = Vector3.Dot(gameMan.playerController.movement.normalized, Quaternion.AngleAxis(strafeAngle, Vector3.up) * deltaToPlayer.normalized) + 1;
			if (strafeScore > 1.9f) {
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

		Vector3 verticalDeltaToPlayer = new Vector3(0, deltaToPlayer.y, 0);
		movementDelta += Vector3.Normalize(moveDirection * Vector3.forward + verticalDeltaToPlayer) * MoveSpeed;
	}
}
