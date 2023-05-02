using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalDropoff : MonoBehaviour
{
    GameManager gameManager;
    PlayerController playerController;

    void Start()
    {
        gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
        playerController = gameManager.playerController;
    }

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHurtbox && playerController.hasCrystal) {
            gameManager.SpawnParticle(3, transform.position, 0.5f);
            gameManager.crystalCount++;
            playerController.hasCrystal = false;
            gameManager.crystalPickupImage.enabled = false;
		}
	}
}