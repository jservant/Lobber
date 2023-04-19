using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadProjectile : MonoBehaviour {
	public float speed = 25f;
	public int damage = 8;
	public float lifetime = 3f;
	public float stunSphereRadius = 3f;
	public bool canStun = true;
	public bool canPierce = false;
	int enemiesKilled = 0;

	Transform head;
	Rigidbody rb;
	GetKnockbackInfo getKnockbackInfo;

	public AK.Wwise.Event HeadImpactSound;

	private void Start() {
		head = transform.Find("Model");
		rb = GetComponent<Rigidbody>();
		getKnockbackInfo = GetComponent<GetKnockbackInfo>();
		Destroy(gameObject, lifetime);
	}

	void Update() {
		transform.Translate(Vector3.forward * speed * Time.deltaTime);
		head.Rotate(Vector3.forward, 1000 * Time.deltaTime);
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.EnemyHurtbox || other.gameObject.layer == (int)Layers.AgnosticHurtbox) {
			if (canStun) {
				Collider[] eColliders = Physics.OverlapSphere(transform.position, stunSphereRadius, Mask.Get(Layers.EnemyHurtbox));
				for (int index = 0; index < eColliders.Length; index += 1) {
					Basic basicEnemy = eColliders[index].gameObject.GetComponent<Basic>();
					if (basicEnemy != null) {
						KnockbackInfo knockbackInfo = getKnockbackInfo.GetInfo(basicEnemy.gameObject);
						basicEnemy.ChangeDirective_Stunned(Basic.StunTime.LongStun, knockbackInfo);
					}
				}
			}
			Sound_HeadImpact();
			Destroy(gameObject);
			if (canPierce && enemiesKilled < 2) { 
				enemiesKilled++;
				Debug.Log("Enemies killed on this skull: " + enemiesKilled);
			}
			else {
				Destroy(gameObject); 
			}
		}
	}

	public void Sound_HeadImpact() {
		HeadImpactSound.Post(gameObject);
	}

    void OnDrawGizmos() {
		Gizmos.color = Color.white;
		Gizmos.DrawWireSphere(transform.position, stunSphereRadius);
	}
}
