using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnterOrb : MonoBehaviour
{
	GameManager gameManager;
	PlayerController playerController;
	TutorialManager tutorialManager;
	Transform lobberPoint;
	public GameObject orb;

    private void Start() {
        gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
		playerController = transform.Find("/Player").GetComponent<PlayerController>();
		tutorialManager = transform.Find("/TutorialManager").GetComponent<TutorialManager>();
		lobberPoint = transform.Find("LobberPoint");

		if (Initializer.save.versionLatest.hardModeUnlocked == true) orb.SetActive(true);
    }

    private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHurtbox && orb.activeInHierarchy) {
			if (gameObject.name == "StartOrb") {
				playerController.portalPoint = lobberPoint;
				playerController.Win();
				StartCoroutine(StartDelay());
				GameManager._hardModeActive = false;
			}
			else if (gameObject.name == "ReplayTutorialOrb") {
				Initializer.save.versionLatest.tutorialComplete = false;
				Initializer.Save();
				playerController.portalPoint = lobberPoint;
				playerController.Win();
				StartCoroutine(StartDelay());
			}
			else if (gameObject.name == "HardModeOrb") {
				playerController.portalPoint = lobberPoint;
				playerController.Win();
				StartCoroutine(StartDelay());
				GameManager._hardModeActive = true;
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
		if (gameObject.name == "ReplayTutorialOrb") SceneManager.LoadScene((int)Scenes.Tutorial);
		else gameManager.OnRestartConfirm();
	}
}
