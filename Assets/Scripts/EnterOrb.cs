using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterOrb : MonoBehaviour
{
	GameManager gameManager;
	PlayerController playerController;
	TutorialManager tutorialManager;

    private void Start() {
        gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
		playerController = transform.Find("/Player").GetComponent<PlayerController>();
		tutorialManager = transform.Find("/TutorialManager").GetComponent<TutorialManager>();
    }

    private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHurtbox) {
			if (gameObject.name == "StartOrb") { gameManager.OnRestartConfirm(); }
			else if (gameObject.name == "ReplayTutorialOrb") {
				Initializer.save.versionLatest.tutorialComplete = false;
				Initializer.Save();
                playerController.transform.position = tutorialManager.playerRespawnPoints[0].position;
			}
		}
	}
}
