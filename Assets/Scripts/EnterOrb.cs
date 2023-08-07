using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterOrb : MonoBehaviour
{
	GameManager gameManager;

    private void Start() {
        gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
    }

    private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHurtbox) {
			gameManager.OnRestartConfirm();
		}
	}
}
