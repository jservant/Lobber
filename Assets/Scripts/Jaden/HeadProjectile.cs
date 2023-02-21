using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadProjectile : MonoBehaviour {
	[SerializeField] float speed = 25f;
	[SerializeField] int damage = 8;
	[SerializeField] float lifetime = 3f;
	Transform head;

	private void Start() {
		head = transform.Find("Model");
		Destroy(gameObject, lifetime);
	}

	void Update() {
		transform.Translate(Vector3.forward * speed * Time.deltaTime);
		head.Rotate(Vector3.forward, 1000 * Time.deltaTime);
		//head.eulerAngles = new Vector3(0, 5, 0);
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.EnemyHurtbox) {
			Enemy eInstance = other.GetComponent<Enemy>();
			//eInstance.ReceiveDamage(damage);
			Destroy(gameObject);
		}
	}
}
