using UnityEngine;
using System.Collections.Generic;

public enum Layers : int {
	Default = 0,
	TransparentFX,
	IgnoreRaycast,
	Unused1,
	Water,
	UI,
	EnemyHitbox,
	EnemyHurtbox,
	PlayerHitbox,
	PlayerHurtbox,
	Pickup,
	Unused2,
	Ground,
	AgnosticHitbox,
	AgnosticHurtbox,
	SoundTrigger,
	StickyLedge,
}

public enum Scenes : int {
	MainMenu = 0,
	Level_B,
	Level_K,
	Level_I,
}

public class Mask {
	public static int Get(Layers[] array) {
		int Result = 0;
		foreach (Layers layer in array) {
			Result |= 1 << ((int)layer);
		}
		return Result;
	}
	public static int Get(Layers layer) {
		return Get(new Layers[] { layer });
	}
}

public class Util {
	public static int EnumLength(System.Type EnumType) {
		return System.Enum.GetNames(EnumType).Length;
	}

	public static void ShowAttackWarning(GameManager gameMan, Vector3 position) {
		GameObject.Instantiate(gameMan.FlashPrefab, position, Quaternion.LookRotation(Camera.main.transform.position - position, Vector3.up));
	}

	public static int RollWeightedChoice(float[] weight) {
		int result = -1;
		float total = 0;
		for (int index = 0; index < weight.Length; index += 1) {
			total += weight[index];
			Debug.Assert(weight[index] >= 0);
		}

		Debug.Assert(total > 0);

		float randomRoll = Random.Range(0, total);
		float rollingTotal = 0;
		for (int index = 0; index < weight.Length; index += 1) {
			rollingTotal += weight[index];
			if (randomRoll <= rollingTotal) {
				result = index;
				break;
			}
		}

		return result;
	}

	// NOTE(Roskuski): Returns if we hit a wall.
	public static bool PerformCheckedLateralMovement(GameObject gameObject, float verticalOffset, float spherecastRadius, Vector3 translationDelta, int layerMask, int depthCount = 0, bool hitWall = false) {
		depthCount += 1;

		// NOTE(Roskuski): This avoids a infinite recursion, which would somehow lead to a crash.
		// Sometimes, translationDelta arrives at zero and then infinitely recurses. This early out should pervent infinite loops
		// After adapting basicEnemy to use this code, high recursion was rearing it's head again. 15 cap should be reasonable.
		if ((depthCount > 1 && translationDelta.magnitude < 0.1f) || depthCount >= 15) {
			return hitWall;
		}

		RaycastHit hitInfo;
		float checkDistance = translationDelta.magnitude > 0.1f ? translationDelta.magnitude : 0.1f;
		if (Physics.SphereCast(gameObject.transform.position + Vector3.up * (verticalOffset), spherecastRadius, translationDelta, out hitInfo, checkDistance, layerMask) && (hitInfo.collider.isTrigger == false)) {

			// Move up to the wall, with a safe distance
			Vector3 hitDelta = hitInfo.point - gameObject.transform.position;
			Vector3 hitMove = (new Vector3 (hitDelta.normalized.x, 0, hitDelta.normalized.y) * translationDelta.magnitude) - hitInfo.point.normalized * (spherecastRadius * 1f);
			bool tookParticalMove = false;
			if (Vector3.Dot(hitDelta, translationDelta) >= 0.9f) {
				tookParticalMove = true;
				gameObject.transform.position += hitMove;
			}

			// Account for the distance we have already moved
			Vector3 remainingMove = translationDelta;
			if (tookParticalMove) {
				remainingMove = remainingMove - hitMove;
			}

			// figure out if we want to slide left or right
			float leftScore = Vector3.Dot(Quaternion.AngleAxis(-90, Vector3.up) * hitInfo.normal, remainingMove);
			float rightScore = Vector3.Dot(Quaternion.AngleAxis(90, Vector3.up) * hitInfo.normal, remainingMove);

			float angleToSlide = 90;
			if (leftScore > rightScore) {
				angleToSlide = -90;
			}

			// clip our movement in the direction of the opposite normal of the wall
			float angleToRight = Vector3.SignedAngle(hitInfo.normal, Vector3.right, Vector3.up);
			remainingMove = Quaternion.AngleAxis(angleToRight, Vector3.up) * remainingMove;
			remainingMove.x = 0;
			remainingMove = Quaternion.AngleAxis(-angleToRight, Vector3.up) * remainingMove;

			// calculate the new movement after clipping
			Vector3 remainingDelta = Quaternion.AngleAxis(angleToSlide, Vector3.up) * hitInfo.normal * remainingMove.magnitude;
			
			// attempt to do that move successfully
			return PerformCheckedLateralMovement(gameObject, verticalOffset, spherecastRadius, remainingDelta, layerMask, depthCount, true);
		}
		else {
			gameObject.transform.position += translationDelta;
		}

		return hitWall;
	}

	// NOTE(Roskuski): Returns if we hit the floor.
	public static bool PerformCheckedVerticalMovement(GameObject gameObject, float stepUpHeight, float stepDownHeight, float spherecastRadius, float fallingSpeed) {
		bool result;

		RaycastHit hitInfo;
		if (Physics.SphereCast(gameObject.transform.position + Vector3.up * stepUpHeight, spherecastRadius, Vector3.down, out hitInfo, stepUpHeight + stepDownHeight, Mask.Get(Layers.Ground)) && (hitInfo.collider.isTrigger == false)) {
			float distanceToGround = hitInfo.distance - stepUpHeight + spherecastRadius;
			gameObject.transform.position -= new Vector3(0, distanceToGround, 0);
			result = true;
		}
		else {
			// @TODO(Roskuski): We should probably try to step up after we fall
			gameObject.transform.position -= new Vector3(0, fallingSpeed, 0) * Time.deltaTime;
			result = false;
		}

		return result;
	}

	// Credit for ConeCast code: Copyright (c) 2018 Walter Ellis
	// https://github.com/walterellisfun/ConeCast/blob/master/LICENSE
	public static RaycastHit[] ConeCastAll(Vector3 origin, float maxRadius, Vector3 direction, float maxDistance, float coneAngle) {
		RaycastHit[] sphereCastHits = Physics.SphereCastAll(origin - new Vector3(0, 0, maxRadius), maxRadius, direction, maxDistance);
		List<RaycastHit> coneCastHitList = new List<RaycastHit>();

		if (sphereCastHits.Length > 0) {
			for (int i = 0; i < sphereCastHits.Length; i++) {
				sphereCastHits[i].collider.gameObject.GetComponent<Renderer>().material.color = new Color(1f, 1f, 1f);
				Vector3 hitPoint = sphereCastHits[i].point;
				Vector3 directionToHit = hitPoint - origin;
				float angleToHit = Vector3.Angle(direction, directionToHit);

				if (angleToHit < coneAngle) {
					coneCastHitList.Add(sphereCastHits[i]);
				}
			}
		}

		RaycastHit[] coneCastHits = new RaycastHit[coneCastHitList.Count];
		coneCastHits = coneCastHitList.ToArray();

		return coneCastHits;
	}
}
