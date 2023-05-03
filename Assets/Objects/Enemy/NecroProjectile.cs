using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NecroProjectile : MonoBehaviour {
	[SerializeField] Vector3 moveDirection;
	[SerializeField] const float MoveSpeed = 14f;
	[SerializeField] const float TurnSpeed = 180.0f;
	[SerializeField] bool forcedFall = false;

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

			switch ((Layers)other.gameObject.layer) {
				case Layers.PlayerHurtbox: 
					if (other.GetComponent<PlayerController>() != null && gameMan.playerController.currentAttack != PlayerController.Attacks.Dashing && gameMan.playerController.currentAttack != PlayerController.Attacks.LethalDash) {
					GameObject.Destroy(this.gameObject);
					}
					break;
					
				default:
					GameObject.Destroy(this.gameObject);
					break;
			}
		}

	}

	void FixedUpdate() {
		if (this.transform.parent == null) {
			Vector3 deltaToPlayer = gameMan.player.position - this.transform.position + Vector3.up * 1.00f;
			moveDirection = Vector3.RotateTowards(moveDirection.normalized, deltaToPlayer.normalized, Mathf.PI*2f * (TurnSpeed/360) * Time.fixedDeltaTime, 0);

			if (deltaToPlayer.magnitude > 0.50f) {
				forcedFall = true; 
			}
			if (forcedFall) {
				this.transform.position += Vector3.down * 2.0f * Time.fixedDeltaTime;
				moveDirection.y = 0;
			}
			this.transform.position += moveDirection * MoveSpeed * Time.fixedDeltaTime;
			this.transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);
		}
	}

	void Update() {
		if (this.transform.parent == null) {
			this.transform.localScale = new Vector3(1, 1, 1);
		}
	}

    private void OnDestroy() {
		gameMan.SpawnParticle(9, transform.position, 1f);
    }
}
