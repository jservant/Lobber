using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class KnockbackInfo {
	public Quaternion direction;
	public float force;
	public float time;

	public KnockbackInfo(Quaternion direction, float force, float time) {
		this.direction = direction;
		this.force = force;
		this.time = time;
	}
	public KnockbackInfo(KnockbackInfo info) {
		this.direction = info.direction;
		this.force = info.force;
		this.time = info.time;
	}
}

public class GetKnockbackInfo : MonoBehaviour {
	public enum InfoSource {
		Constant,
		DirectionLinkedObject,
		DirectionLaterallyAwayFromLinkedObject,
	};
	public InfoSource infoSource = InfoSource.Constant;

	public KnockbackInfo constantInfo;
	public GameObject linkedObject;

	public KnockbackInfo GetInfo(GameObject referenceObject) {
		KnockbackInfo knockbackInfo = new KnockbackInfo(constantInfo);
		switch (infoSource) {
			default:
				Debug.Assert(false);
				break;

			case InfoSource.Constant:
				// Set from prior
				break;

			case InfoSource.DirectionLinkedObject:
				knockbackInfo.direction = linkedObject.transform.rotation;
				break;

			case InfoSource.DirectionLaterallyAwayFromLinkedObject:
				Vector3 referencePosition = referenceObject.transform.position;
				referencePosition.y = 0;
				Vector3 linkedPosition = linkedObject.transform.position;
				linkedPosition.y = 0;

				knockbackInfo.direction = Quaternion.LookRotation(referencePosition - linkedPosition, Vector3.up);
				break;
		}

		return knockbackInfo;
	}
}
