using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadProjectile : MonoBehaviour {
	[SerializeField] float speed = 25f;
	[SerializeField] int damage = 8;
	[SerializeField] float lifetime = 3f;
	[SerializeField] float stunSphereRadius = 3f;
	Transform head;
	Rigidbody rb;

	private void Start() {
		head = transform.Find("Model");
		rb = GetComponent<Rigidbody>();
		Destroy(gameObject, lifetime);
	}

	void Update() {
		transform.Translate(Vector3.forward * speed * Time.deltaTime);
		head.Rotate(Vector3.forward, 1000 * Time.deltaTime);
		//head.eulerAngles = new Vector3(0, 5, 0);
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.EnemyHurtbox) {
			Debug.Log("proj should die lol");
			Collider[] eColliders = Physics.OverlapSphere(transform.position, stunSphereRadius, Mask.Get(Layers.EnemyHurtbox));
			for (int index = 0; index < eColliders.Length; index += 1) {
				// add the enemy stun command here
			}
					Destroy(gameObject);
		}
	}

	/*private void OnCollisionEnter(Collision collision) {
		if (collision.gameObject.layer == (int)Layers.EnemyHurtbox) {
			Debug.Log("proj should die lol");
			Destroy(gameObject);
		}
	}*/
}
