using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour {
	GameManager gameMan;
	private Transform player;
	public GameObject[] areas;
	public Transform[] playerRespawnPoints;
	public List<GameObject> currentTargets = new List<GameObject>();
	public int areasCompleted = 0;
	public bool targetsExist;
	public bool skipTutorial;
    public static bool firstTimeSinceBoot = true;

    private void Awake() {
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		player = transform.Find("/Player");
		UpdateAreas();
	}

	void Start() {
		Initializer.Load();
		gameMan.playerController.health = gameMan.playerController.healthMax;
		int spawnChooser = 0;
		if (skipTutorial) { Initializer.save.versionLatest.tutorialComplete = true; }
		if (Initializer.save.versionLatest.tutorialComplete || skipTutorial) {
			if (firstTimeSinceBoot == false) {
				spawnChooser = playerRespawnPoints.Length - 2;
				gameMan.playerController.transform.position = playerRespawnPoints[spawnChooser].position;
			} // spawn in main hub at the end
			else {
				spawnChooser = playerRespawnPoints.Length - 1;
                gameMan.playerController.transform.position = playerRespawnPoints[playerRespawnPoints.Length - 1].position; // spawn in hub close to portal
			}
            firstTimeSinceBoot = false;
		}
		Debug.Log("Is the tutorial completed: " + Initializer.save.versionLatest.tutorialComplete);
		Debug.Log("Player is spawning at " + playerRespawnPoints[spawnChooser].gameObject.name);
	}

	// Update is called once per frame
	void Update() {
		targetsExist = CheckTargets();
		if (!targetsExist && (areasCompleted < 5)) {
			areasCompleted += 1;
			UpdateAreas();
		}

		if (player.position.z > 65f) {
			areasCompleted = 6;
			Initializer.save.versionLatest.tutorialComplete = true;
			UpdateSpawns();
		}
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
