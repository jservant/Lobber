using UnityEngine;

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
	TrapHitbox,
	Ground,
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

	public static void PreformCheckedLateralMovement(GameObject gameObject, float verticalOffset, float spherecastRadius, Vector3 translationDelta, int depthCount = 0) {
		depthCount += 1;

		// @NOTE(Roskuski): This avoids a infinite recursion, which would somehow lead to a crash.
		// @TODO(Roskuski): This leads me to believe that when this function does get into a state where it would infinitly recurse that it's values get out of control, leading to the crash, as it seems that unity can handle the infinite recursion "fine".
		// The crash happens on a assertion falure in SphereCast
		// It appears that translationDelta arrives at zero and then infinitely recurses. This early out should pervent infinite loops
		if (translationDelta == Vector3.zero || depthCount >= 15) {
			return;
		}

		RaycastHit hitInfo;
		float checkDistance = translationDelta.magnitude > 0.1f ? translationDelta.magnitude : 0.1f;
		if (Physics.SphereCast(gameObject.transform.position + Vector3.up * (verticalOffset), spherecastRadius, translationDelta, out hitInfo, checkDistance) && (hitInfo.collider.isTrigger == false)) {

			// Move up to the wall, with a safe distance
			Vector3 hitDelta = hitInfo.point - gameObject.transform.position;
			Vector3 hitMove = (new Vector3 (hitDelta.x, 0, hitDelta.y) * translationDelta.magnitude) - translationDelta.normalized * spherecastRadius;
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
			PreformCheckedLateralMovement(gameObject, verticalOffset, spherecastRadius, remainingDelta, depthCount);
		}
		else {
			gameObject.transform.position += translationDelta;
		}
	}

	public static void PreformCheckedVerticalMovement(GameObject gameObject, float stepUpHeight, float stepDownHeight, float spherecastRadius, float fallingSpeed) {
		RaycastHit hitInfo;
		if (Physics.SphereCast(gameObject.transform.position + Vector3.up * stepUpHeight, spherecastRadius, Vector3.down, out hitInfo, stepUpHeight + stepDownHeight, Mask.Get(Layers.Ground)) && (hitInfo.collider.isTrigger == false)) {
			float distanceToGround = hitInfo.distance - stepUpHeight + spherecastRadius;
			gameObject.transform.position -= new Vector3(0, distanceToGround, 0);
		}
		else {
			// @TODO(Roskuski): We should probably try to step up after we fall
			gameObject.transform.position -= new Vector3(0, fallingSpeed, 0) * Time.deltaTime;
		}
	}
}
