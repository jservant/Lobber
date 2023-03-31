using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonePile : MonoBehaviour
{
	PlayerController player;

	private void Start() {
		player = transform.Find("/Player").GetComponent<PlayerController>();
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHitbox) { // player is hitting enemy
			player.ChangeMeter(1);
			Destroy(gameObject);
		}
	}
}
