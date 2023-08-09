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
			if (gameObject.name == "StartOrb") {
				playerController.Win();
				StartCoroutine(StartDelay());
			}
			else if (gameObject.name == "ReplayTutorialOrb") {
				Initializer.save.versionLatest.tutorialComplete = false;
				Initializer.Save();
                playerController.transform.position = tutorialManager.playerRespawnPoints[0].position;
			}
		}
	}

	private IEnumerator StartDelay() {
		OrbSpawn orb = GetComponent<OrbSpawn>();
		yield return new WaitForSeconds(0.6f);
		orb.anim.SetBool("DeSpawn", true);
		yield return new WaitForSeconds(0.4f);
		gameManager.SpawnParticle(12, transform.position, 1.5f);
		Util.SpawnFlash(gameManager, 11, transform.position, true);
		gameManager.OnRestartConfirm();
	}
}
