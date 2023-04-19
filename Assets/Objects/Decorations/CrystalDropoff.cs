using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalDropoff : MonoBehaviour
{
    public int crystalCount;
    public int crystalGoal;
    GameManager gameManager;
    PlayerController playerController;

    void Start()
    {
        gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
        playerController = gameManager.playerController;
    }

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHitbox && playerController.hasCrystal) {
            crystalCount++;
            playerController.hasCrystal = false;
            if (crystalCount >= crystalGoal) {
                StartCoroutine(gameManager.Win());
			}
		}
	}
}