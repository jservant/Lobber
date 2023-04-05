using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveTrap : MonoBehaviour {
	public GameObject hitbox;
	private Collider capsule;
	private bool isArmed;
	public float armTime; //time it takes for the trap to rearm itself
	private float currentArmTime;
	public float triggerTime; //active frames of the trap hitbox
	public float currentTriggerTime;
	public Animator anim;
	public MeshRenderer barrel;

	private bool check = true;

	void Start() {
		capsule = this.GetComponent<CapsuleCollider>();
		hitbox.SetActive(false);
		isArmed = true;
		anim.Play("BombBarrel");
		currentArmTime = armTime;
		currentTriggerTime = 0f;
	}

	void Update() {
		if (isArmed == false) {
			currentArmTime -= Time.deltaTime;
			barrel.enabled = false;
			capsule.enabled = false;
			check = true;
			if (currentArmTime <= 0) isArmed = true;
		}
		else {
			barrel.enabled = true;
			capsule.enabled = true;
			if (check) {
				anim.Play("BombBarrel", -1, 0f);
				check = false;
			}
		}

		if (currentTriggerTime <= 0) {
			hitbox.SetActive(false);
		}
		else currentTriggerTime -= Time.deltaTime;

	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHitbox || other.gameObject.layer == (int)Layers.AgnosticHitbox) {
			if (isArmed) SpringTrap();
		}
	}

	void SpringTrap() {
		isArmed = false;
		currentArmTime = armTime;
		currentTriggerTime = triggerTime;
		hitbox.SetActive(true);
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, 10f);
	}
}
