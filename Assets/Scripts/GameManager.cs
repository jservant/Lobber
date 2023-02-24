using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour {
	public Transform player;
	public PlayerController playerController;
	public Camera camera; 

	public GameObject PlayerPrefab;
	public GameObject SkullPrefab;
	public GameObject EnemyPrefab;


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
	}


	private void Update() {
		if (frozenTime > 0) {
			Time.timeScale = 0.0f;
			frozenTime -= Time.unscaledDeltaTime;
		}
		else {
			Time.timeScale = 1.0f;
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
	}

	// NOTE(Ryan): Can be called to freeze the game for the time specified.
	// Frames60 is the amount of time, based on a 16.66ms long frame
	public void FreezeFrames(int Frames60) {
		frozenTime += (float)(Frames60) / 60.0f;
	}

	void OnEnable() { dActions.Enable(); }
	void OnDisable() { dActions.Disable(); }
}
