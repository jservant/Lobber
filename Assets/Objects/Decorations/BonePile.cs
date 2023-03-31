using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonePile : MonoBehaviour
{
	PlayerController player;
	public GameObject headPop;

	private void Start() {
		player = transform.Find("/Player").GetComponent<PlayerController>();
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHitbox) { // player is hitting enemy
			//player.ChangeMeter(1);
			GameObject headInstance = Instantiate(headPop, transform.position, transform.rotation);
			HeadPickup hpop = headInstance.transform.Find("Head").GetComponent<HeadPickup>();
			hpop.randomForce = 5f;
			Destroy(gameObject);
		}
	}
}
