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
            gameManager.SpawnParticle(10, transform.position, 0.8f);
            gameManager.SpawnParticle(11, transform.position, 1f);
            gameManager.crystalCount++;
            playerController.hasCrystal = false;
            for (var i = playerController.crystalHolster.childCount - 1; i >= 0; i--) {
                Destroy(playerController.crystalHolster.GetChild(i).gameObject);
            }
            gameManager.crystalPickupImage.enabled = false;
            gameManager.waypointMarker.enabled = false;
            gameManager.waypointTracking = false;
		}
	}
}