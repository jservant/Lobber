using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomForce : MonoBehaviour {
	public float force;
	public float torque;
	public Rigidbody[] rb;

	public bool hasHead;
	public float lifeTime; //how long the corpse stays in the level
	public float shrinkTime; //how quick the pieces shrink when lifetime is up
	private float shrinkTimer;

	// Start is called before the first frame update
	void Start() {
		if (!hasHead) rb[0].gameObject.SetActive(false);

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

		lifeTime = Random.Range(lifeTime - 0.5f, lifeTime + 0.5f);
		shrinkTimer = shrinkTime;
	}

	// Update is called once per frame
	void Update() {
		lifeTime -= Time.deltaTime;
		if (lifeTime <= 0) {
			for (int i = 0; i < rb.Length; i++) {
				rb[i].transform.localScale *= Mathf.Lerp(0, 1, shrinkTime / shrinkTimer);
			}
			shrinkTime -= Time.deltaTime;
		}

		if (shrinkTime <= 0) Destroy(gameObject);
	}
}
