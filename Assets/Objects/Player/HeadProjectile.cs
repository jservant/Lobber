using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadProjectile : MonoBehaviour {
	public float speed = 25f;
	public int damage = 8;
	public float lifetime = 3f;
	public float stunSphereRadius = 3f;
	public bool canStun = true;

	Transform head;
	Rigidbody rb;
	GetKnockbackInfo getKnockbackInfo;

	private void Start() {
		head = transform.Find("Model");
		rb = GetComponent<Rigidbody>();
		getKnockbackInfo = GetComponent<GetKnockbackInfo>();
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
			if (canStun) {
				Collider[] eColliders = Physics.OverlapSphere(transform.position, stunSphereRadius, Mask.Get(Layers.EnemyHurtbox));
				for (int index = 0; index < eColliders.Length; index += 1) {
					Basic basicEnemy = eColliders[index].gameObject.GetComponent<Basic>();
					KnockbackInfo knockbackInfo = getKnockbackInfo.GetInfo(basicEnemy.gameObject);
					basicEnemy.ChangeDirective_Stunned(2.0f, knockbackInfo);
				}
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
	void OnDrawGizmos() {
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position, stunSphereRadius);
	}
}
