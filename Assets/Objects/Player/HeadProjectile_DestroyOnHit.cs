using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadProjectile_DestroyOnHit : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != (int)Layers.SoundTrigger) {
			Destroy(this.transform.Find("../").gameObject);
		}
	}
}
