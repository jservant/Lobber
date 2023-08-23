using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour {
	GameManager gameMan;
	private Transform player;
	PlayerController playerController;
	public GameObject[] areas;
	public Transform[] playerRespawnPoints;
	public List<GameObject> currentTargets = new List<GameObject>();
	Animator anim;
	Canvas titleCanvas;

	public int areasCompleted = 0;
	public bool targetsExist;
	public bool skipTutorial;
    public static bool firstTimeSinceBoot = true;

    private void Awake() {
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		player = transform.Find("/Player");
		playerController = player.GetComponent<PlayerController>();
		titleCanvas = transform.Find("Intro").GetComponent<Canvas>();
		anim = GetComponent<Animator>();

		if (firstTimeSinceBoot == false) {
			titleCanvas.enabled = false;
			anim.enabled = false;
		}
		UpdateAreas();
	}

	void Start() {
		gameMan.playerController.health = gameMan.playerController.healthMax;
		int spawnChooser = 0;
		if (skipTutorial) { Initializer.save.versionLatest.tutorialComplete = true; }
		if (Initializer.save.versionLatest.tutorialComplete || skipTutorial) {
			if (firstTimeSinceBoot == false) {
				spawnChooser = playerRespawnPoints.Length - 2;
				gameMan.playerController.transform.position = playerRespawnPoints[spawnChooser].position; // spawn in main hub at the end	
			} 
			else {
				spawnChooser = playerRespawnPoints.Length - 1;
                gameMan.playerController.transform.position = playerRespawnPoints[playerRespawnPoints.Length - 1].position; // spawn in hub close to portal
			}
            firstTimeSinceBoot = false;
            areasCompleted = 0;
        }
		Debug.Log("Player is spawning at " + playerRespawnPoints[spawnChooser].gameObject.name);
	}

	void Update() {
		targetsExist = CheckTargets();
		if (!targetsExist && (areasCompleted < 5)) {
			areasCompleted += 1;
			UpdateAreas();
			if (areasCompleted == 5) {
				Initializer.save.versionLatest.tutorialComplete = true;
				Initializer.Save();
			}
		}

		if (areasCompleted > 4) {
			areasCompleted = 6;
			
			UpdateSpawns();
		}

		if (titleCanvas.enabled) {
			playerController.canMove = false;
			if (playerController.pActions.Player.Pause.WasPerformedThisFrame()) {
				playerController.canMove = true;
				anim.enabled = false;
				titleCanvas.enabled = false;
			}
		}
		else playerController.canMove = true;
    }

	void UpdateAreas() {
		for (int i = 0; i < areas.Length; i++) {
			if (i <= areasCompleted) {
				areas[i].SetActive(true);
			}
			else areas[i].SetActive(false);
		}
		currentTargets.Clear();

		foreach (Transform child in areas[areasCompleted].transform) {
			if (child.gameObject.layer == (int)Layers.EnemyHurtbox) currentTargets.Add(child.gameObject);
		}

		UpdateSpawns();
	}


	void UpdateSpawns() { 
		for (int i = 0; i < playerRespawnPoints.Length; i++) {
			playerRespawnPoints[i].gameObject.SetActive(false);
		}
		playerRespawnPoints[areasCompleted].gameObject.SetActive(true);
		gameMan.UpdatePlayerSpawns();
	}

	private bool CheckTargets() {
		for (int n = 0; n < currentTargets.Count; n++) {
			if (currentTargets[n] != null) return true;
		}
		return false;
	}

}
