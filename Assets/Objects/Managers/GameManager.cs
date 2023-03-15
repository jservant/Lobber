using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using TMPro;

public class GameManager : MonoBehaviour {
	public Transform player;
	public PlayerController playerController;
	public Camera camera;

	public TMP_Text ammoUI;
	public Canvas pauseUI;
	public Transform healthBar;

	public GameObject PlayerPrefab;
	public GameObject SkullPrefab;
	public GameObject EnemyPrefab;
	public GameObject HeadPickupPrefab;

	public string[] scenes;
	static int sceneValue = 0;

	public bool updateTimeScale = true;


	DebugActions dActions;
	float frozenTime = 0;

	void Awake() {
		player = transform.Find("/Player");
		playerController = player.GetComponent<PlayerController>();
		if (player != null) {
			Debug.Log("Object Named Player found");
		}
		else Debug.LogWarning("Object Named Player Not found");

		dActions = new DebugActions();
		camera = transform.Find("/CameraPoint/Main Camera").GetComponent<Camera>();
		ammoUI = transform.Find("MainUI/AmmoUI").GetComponent<TMP_Text>();
		pauseUI = transform.Find("PauseUI").GetComponent<Canvas>();
		ammoUI.text = "SKULLS: 0";
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

		if (dActions.DebugTools.SpawnEnemy.WasPerformedThisFrame()) {
			Vector2 MouseLocation2D = dActions.DebugTools.MouseLocation.ReadValue<Vector2>();
			Vector3 MouseLocation = new Vector3(MouseLocation2D.x, MouseLocation2D.y, 0);
			Ray ray = camera.ScreenPointToRay(MouseLocation);
			RaycastHit hit;

			if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000.0f)) {
				Debug.Log(EnemyPrefab.name + " spawned at " + hit.point);
				Instantiate(EnemyPrefab, hit.point, Quaternion.identity);
			}
		}

		if (dActions.DebugTools.SwitchScene.WasPerformedThisFrame()) {
			sceneValue++;
			if (sceneValue >= scenes.Length) {
				sceneValue = 0;
			}
			SceneManager.LoadScene(scenes[sceneValue]);
		}

		if (playerController.pActions.Player.Pause.WasPerformedThisFrame()) {
			if (updateTimeScale) {
				updateTimeScale = false;
				Time.timeScale = 0;
				pauseUI.enabled = true;
			} else {
				updateTimeScale = true;
				Time.timeScale = 1;
				pauseUI.enabled = false;
			}
		}

		UpdateHealthBar();
	}

	// NOTE(Ryan): Can be called to freeze the game for the time specified.
	// Frames60 is the amount of time, based on a 16.66ms long frame
	public void FreezeFrames(int Frames60) {
		frozenTime += (float)(Frames60) / 60.0f;
	}

	public void UpdateHealthBar()
    {
		float healthMax = playerController.healthMax;
		float health = playerController.health;
		healthBar.localScale = new Vector3 ((health / healthMax) * 7.26f, 3f, 1f);
    }

	void OnEnable() { dActions.Enable(); }
	void OnDisable() { dActions.Disable(); }
}
