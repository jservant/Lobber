using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NecroProjectile : MonoBehaviour {
	public Vector3 moveDirection;
	const float MoveSpeed = 10f;

	// NOTE(Roskuski): External references
	GameManager gameMan;

	void Start() {
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();

		if (this.transform.parent != null) {
			moveDirection = this.transform.parent.rotation * Vector3.forward;
		}
	}

	void OnTriggerEnter(Collider other) {
		if (this.transform.parent == null) {
			GameObject.Destroy(this.gameObject);
		}
	}

	void FixedUpdate() {
		if (this.transform.parent == null) {
			Vector3 deltaToPlayer = gameMan.player.position - this.transform.position + Vector3.up * 0.6f;
			moveDirection = Vector3.RotateTowards(moveDirection.normalized, deltaToPlayer.normalized, Mathf.PI*2f * (120f/360f) * Time.fixedDeltaTime, 0);
			this.transform.position += moveDirection * MoveSpeed * Time.fixedDeltaTime;
		}
	}

	void Update() {
		if (this.transform.parent == null) {
			this.transform.localScale = new Vector3(1, 1, 1);
		}
	}
}
