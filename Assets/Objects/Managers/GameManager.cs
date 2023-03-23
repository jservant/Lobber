using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour {
	public Transform player;
	public PlayerController playerController;

	public Canvas mainUI;
	public Canvas pauseBG;
	public Canvas pauseUI;
	public Canvas optionsUI;
	public TMP_Text statusTextboxText;
	public Transform healthBar;
	public Transform meterBar;

	public GameObject PlayerPrefab;
	public GameObject SkullPrefab;
	public GameObject EnemyPrefab;
	public GameObject HeadPickupPrefab;
	public GameObject eSpawn;
	public OrbSpawn[] eSpawns;
	public List<GameObject> enemies;
	public int enemiesKilled = 0;

	bool transitioningLevel = false;

	public bool updateTimeScale = true;
	public bool canSpawn = true;
	DebugActions dActions;
	float frozenTime = 0;

	void Awake() {
		player = transform.Find("/Player");
		playerController = player.GetComponent<PlayerController>();
		if (player != null) {
			Debug.Log("Object Named Player found");
		}
		else Debug.LogWarning("Object Named Player Not found");
		eSpawn = GameObject.Find("EnemySpawns");
		eSpawns = eSpawn.GetComponentsInChildren<OrbSpawn>();
		dActions = new DebugActions();
		pauseBG = transform.Find("MainUI").GetComponent<Canvas>();
		pauseBG = transform.Find("PauseBG").GetComponent<Canvas>();
		pauseUI = transform.Find("PauseUI").GetComponent<Canvas>();
		optionsUI = transform.Find("OptionsUI").GetComponent<Canvas>();
		statusTextboxText = transform.Find("StatusTextbox/StatusTextboxText").GetComponent<TMP_Text>();
		statusTextboxText.text = "";

		enemies = new List<GameObject>();

		if (canSpawn) for (int i = 0; i < eSpawns.Length; i++) {
			eSpawns[i].spawnNow = true;
		}
	}

	private void Update() {
		if (updateTimeScale) {
			if (frozenTime > 0) {
				Time.timeScale = 0.0f;
				frozenTime -= Time.unscaledDeltaTime;
			}
			else {
				Time.timeScale = 1.0f;
			}
		}

		bool isMenuOpen = pauseUI.enabled || optionsUI.enabled;

		if (!isMenuOpen && dActions.DebugTools.SpawnEnemy.WasPerformedThisFrame()) {
			Vector2 MouseLocation2D = dActions.DebugTools.MouseLocation.ReadValue<Vector2>();
			Vector3 MouseLocation = new Vector3(MouseLocation2D.x, MouseLocation2D.y, 0);
			Ray ray = Camera.main.ScreenPointToRay(MouseLocation);
			RaycastHit hit;

			if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000.0f)) {
				Debug.Log(EnemyPrefab.name + " spawned at " + hit.point);
				Instantiate(EnemyPrefab, hit.point, Quaternion.identity);
			}
		}

		if (playerController.pActions.Player.Pause.WasPerformedThisFrame()) {
			if (optionsUI.enabled == true) {
				pauseUI.enabled = true;
				optionsUI.enabled = false;
			}
			else if (pauseUI.enabled == false) {
				updateTimeScale = false;
				Time.timeScale = 0;
				pauseBG.enabled = true;
				pauseUI.enabled = true;
			}
			else {
				updateTimeScale = true;
				Time.timeScale = 1;
				pauseUI.enabled = false;
				pauseBG.enabled = false;
			}
		}

		UpdateHealthBar();
		UpdateMeter();

		if (canSpawn && enemies.Count <= 5 && enemiesKilled < 20) {
			int randSpawn = Random.Range(0, eSpawns.Length);
			eSpawns[randSpawn].spawnNow = true;
		}
		if (enemiesKilled >= 20 && enemies.Count <= 0 && transitioningLevel == false) {
			StartCoroutine(Win());
		}
	}

	IEnumerator Win() {
		transitioningLevel = true;
		if (SceneManager.GetActiveScene().buildIndex == 3) {
			statusTextboxText.text = "YOU WIN!!!";
			Debug.Log("YOUR THE BUIGESS FUCKIN WINNER:; DAMMM");
			yield return new WaitForSeconds(5);
			SceneManager.LoadScene(0);
		}
		else {
			Debug.Log("YOU WIN!! Next stage starting shortly...");
			statusTextboxText.text = "Stage Clear!";
			yield return new WaitForSeconds(5);
			SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
		}
	}

	// NOTE(Ryan): Can be called to freeze the game for the time specified.
	// Frames60 is the amount of time, based on a 16.66ms long frame
	public void FreezeFrames(int Frames60) {
		frozenTime += (float)(Frames60) / 60.0f;
	}
	public void UpdateHealthBar() {
		float healthMax = playerController.healthMax;
		float health = playerController.health;
		healthBar.localScale = new Vector3((health / healthMax), 1f, 1f);
	}

	public void UpdateMeter() {
		meterBar.localScale = new Vector3((playerController.meter / playerController.meterMax), 1f, 1f);
	}

	public void OnResume() {
		updateTimeScale = true;
		Time.timeScale = 1;
		pauseUI.enabled = false;
		pauseBG.enabled = false;
	}

	public void OnOptions() {
		pauseUI.enabled = false;
		optionsUI.enabled = true;
	}

	public void OnOptionsBack() {
		pauseUI.enabled = true;
		optionsUI.enabled = false;
	}

	public void OnQuit() {
		SceneManager.LoadScene(0);
		//Application.Quit();
	}

	void OnEnable() { dActions.Enable(); }
	void OnDisable() { dActions.Disable(); }
}
