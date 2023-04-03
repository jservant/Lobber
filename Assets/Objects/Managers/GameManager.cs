using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class GameManager : MonoBehaviour {
	public Transform player;
	public PlayerController playerController;
	[Header("UI")]
	EventSystem eSystem;
	public Canvas mainUI;
	public Canvas pauseBG;
	public Button resumeButton;
	public Canvas pauseUI;
	public Canvas optionsUI;
	public TMP_Text statusTextboxText;
	public Transform healthBar;
	public Transform meterBar;
	[Header("Prefabs:")]
	public GameObject PlayerPrefab;
	public HeadProjectile SkullPrefab;
	public GameObject EnemyPrefab;
	public GameObject HeadPickupPrefab;

	public GameObject eSpawnParent;
	public OrbSpawn[] eSpawns;
	public int enemiesAlive = 0;
	public int enemiesKilled = 0;

	bool transitioningLevel = false;
	[Header("Bools:")]
	public bool updateTimeScale = true;
	public bool canSpawn = true;
	DebugActions dActions;
	float frozenTime = 0;

	[Header("Particle System:")]
	public ParticleSystem[] particles;

	void Awake() {
		player = transform.Find("/Player");
		playerController = player.GetComponent<PlayerController>();
		if (player != null) {
			Debug.Log("Object Named Player found");
		}
		else Debug.LogWarning("Object Named Player Not found");
		eSpawnParent = GameObject.Find("EnemySpawns");
		eSpawns = eSpawnParent.GetComponentsInChildren<OrbSpawn>();
		dActions = new DebugActions();
		eSystem = GetComponent<EventSystem>();
		mainUI = transform.Find("MainUI").GetComponent<Canvas>();
		pauseBG = transform.Find("PauseBG").GetComponent<Canvas>();
		resumeButton = transform.Find("PauseUI/ResumeButton").GetComponent<Button>();
		pauseUI = transform.Find("PauseUI").GetComponent<Canvas>();
		optionsUI = transform.Find("OptionsUI").GetComponent<Canvas>();
		statusTextboxText = transform.Find("StatusTextbox/StatusTextboxText").GetComponent<TMP_Text>();
		statusTextboxText.text = "";

		if (canSpawn) {
			int randomIndex = Random.Range(0, eSpawns.Length);
			eSpawns[randomIndex].StartCoroutine(eSpawns[randomIndex].Spawning(5));
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
			if (false) {
				Vector2 MouseLocation2D = dActions.DebugTools.MouseLocation.ReadValue<Vector2>();
				Vector3 MouseLocation = new Vector3(MouseLocation2D.x, MouseLocation2D.y, 0);
				Ray ray = Camera.main.ScreenPointToRay(MouseLocation);
				RaycastHit hit;

				if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000.0f)) {
					Debug.Log(EnemyPrefab.name + " spawned at " + hit.point);
					Instantiate(EnemyPrefab, hit.point, Quaternion.identity);
					enemiesAlive += 1;
				}
			}
			else {
				eSpawns[0].StartCoroutine(eSpawns[0].Spawning(5));
			}
		}

		if (playerController.pActions.Player.Pause.WasPerformedThisFrame()) {
			if (optionsUI.enabled == true) {
				resumeButton.Select();
				pauseUI.enabled = true;
				optionsUI.enabled = false;
			}
			else if (pauseUI.enabled == false) {
				resumeButton.Select();
				updateTimeScale = false;
				Time.timeScale = 0;
				pauseBG.enabled = true;
				pauseUI.enabled = true;
			}
			else {
				eSystem.SetSelectedGameObject(null);
				updateTimeScale = true;
				Time.timeScale = 1;
				pauseUI.enabled = false;
				pauseBG.enabled = false;
			}
		}

		UpdateHealthBar();
		UpdateMeter();

		if (canSpawn && enemiesAlive <= 5 && enemiesKilled < 50) {
			int randomIndex = Random.Range(0, eSpawns.Length);
			eSpawns[randomIndex].StartCoroutine(eSpawns[randomIndex].Spawning(5));
		}


		if (enemiesKilled >= 50 && enemiesAlive <= 0 && transitioningLevel == false) {
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
	public void SpawnParticle(int particleID, Vector3 position, float scale) {
		ParticleSystem particle = particles[particleID];
		var TempParticle = Instantiate(particle, position, particle.gameObject.transform.rotation);
		TempParticle.gameObject.transform.localScale *= scale;
		Transform[] particleScales = TempParticle.transform.GetComponentsInChildren<Transform>();
		foreach (Transform t in particleScales) {
			t.localScale *= scale;
        }
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
		eSystem.SetSelectedGameObject(null);
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
