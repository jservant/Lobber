using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CrystalDropoff : MonoBehaviour
{
    GameManager gameManager;
    PlayerController playerController;

    public AK.Wwise.Event crystalDepo1;
    public AK.Wwise.Event crystalDepo2;

    void Start()
    {
        gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
        playerController = gameManager.playerController;
    }

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHurtbox && playerController.crystalCount > 0) {
            gameManager.SpawnParticle(10, transform.position, 0.8f);
            gameManager.SpawnParticle(11, transform.position, 1f);
            crystalDepo1.Post(gameObject);
            crystalDepo2.Post(gameObject);
            gameManager.crystalCount += playerController.crystalCount;
            playerController.crystalCount = 0;
            gameManager.crystalCountText.text = "";
            for (var i = playerController.crystalHolster.childCount - 1; i >= 0; i--) {
                Destroy(playerController.crystalHolster.GetChild(i).gameObject);
            }
            gameManager.crystalPickupImage.enabled = false;
            gameManager.waypointMarker.enabled = false;
            gameManager.waypointTracking = false;
		}
	}
}