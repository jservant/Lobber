using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadProjectile_DestroyOnHit : MonoBehaviour {
	void OnTriggerEnter(Collider other) {
		Destroy(this.transform.Find("../").gameObject);
	}
}
