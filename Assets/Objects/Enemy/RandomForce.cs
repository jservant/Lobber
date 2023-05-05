using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomForce : MonoBehaviour {
	public float force;
	public float torque;
	public Rigidbody[] rb;

	// Start is called before the first frame update
	void Start() {
		for (int i = 0; i < rb.Length; i++) {
			//force
			var rf1 = Random.Range(-force, force);
			var rf2 = Random.Range(1f, force);
			var rf3 = Random.Range(-force, force);

			rb[i].AddForce(rf1, rf2, rf3, ForceMode.Impulse);

			//torque
			var tf1 = Random.Range(-torque, torque);
			var tf2 = Random.Range(-torque, torque);
			var tf3 = Random.Range(-torque, torque);

			rb[i].AddTorque(tf1, tf2, tf3, ForceMode.Impulse);
		}
	}

	// Update is called once per frame
	void Update() {
		
	}
}
