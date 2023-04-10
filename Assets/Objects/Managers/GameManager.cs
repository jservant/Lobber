using System.IO;
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
	public static int storedPlayerHealth = 0;
	public static float storedPlayerMeter = 0;

	[Header("UI")]
	EventSystem eSystem;
	public Canvas mainUI;
	public Canvas pauseBG;
	public Button resumeButton;
	public Canvas pauseUI;
	public Canvas optionsUI;
	public Button statsButton;
	public Canvas statsUI;
	public Button statsBackButton;
	public TMP_Text statsText;
	public TMP_Text statusTextboxText;
	public Transform healthBar;
	public Transform meterBar;

	[Header("Prefabs:")]
	public GameObject PlayerPrefab;
	public HeadProjectile SkullPrefab;
	public GameObject BasicPrefab;
	public GameObject ExplodingPrefab;
	public GameObject HeadPickupPrefab;
	public GameObject FlashPrefab;
	public GameObject OrbSpawnPrefab; 

	[Header("Spawning:")]
	public Transform[] eSpawns;
	public int enemiesAlive = 0;
	public int enemiesKilledInLevel = 0;
	public static int enemiesKilledInRun = 0;
	public static int enemyKillingGoal = 30;
	bool transitioningLevel = false;
	
	[SerializeField] float spawnTokens;
	float spawnDelay;

	readonly static float TokenCost_SmallSpawn = 30;
	readonly static float TokenCost_MediumSpawn = 60;
	readonly static float TokenCost_BigSpawn = 80;
	readonly static float TokensPerSecond = 3.0f;
	readonly static int HighEnemies = 18;
	readonly static int TargetEnemies = 12;
	readonly static int LowEnemies = 4;

	[Header("Bools:")]
	public bool updateTimeScale = true;
	public bool canSpawn = true;
	public bool debugTools = true;
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

		{
			GameObject eSpawnParent = GameObject.Find("EnemySpawns");
			eSpawns = eSpawnParent.GetComponentsInChildren<Transform>();
			Transform[] TempArray =  new Transform[eSpawns.Length - 1];
			for (int index = 0; index < eSpawns.Length; index += 1) {
				if (index - 1 >= 0) {
					TempArray[index - 1] = eSpawns[index];
				}
			}
			eSpawns = TempArray;
		}

		dActions = new DebugActions();
		eSystem = GetComponent<EventSystem>();
		mainUI = transform.Find("MainUI").GetComponent<Canvas>();
		pauseBG = transform.Find("PauseBG").GetComponent<Canvas>();
		resumeButton = transform.Find("PauseUI/ResumeButton").GetComponent<Button>();
		pauseUI = transform.Find("PauseUI").GetComponent<Canvas>();
		optionsUI = transform.Find("OptionsUI").GetComponent<Canvas>();
		statsUI = transform.Find("StatsUI").GetComponent<Canvas>();
		statusTextboxText = transform.Find("StatusTextbox/StatusTextboxText").GetComponent<TMP_Text>();
		statusTextboxText.text = "";
		Time.timeScale = 1;
		
		spawnTokens = 100;
	}

	private void Update() {
		if (updateTimeScale) {
			if (frozenTime > 0) {
				Time.timeScale = 0.0f;
				frozenTime -= Time.unscaledDeltaTime;
			}
			else {
				//Time.timeScale = 1.0f;
			}
		}

		bool isMenuOpen = pauseUI.enabled || optionsUI.enabled;

		if (debugTools) {
			if (!isMenuOpen) {
				{ // Spawn at point
					GameObject toSpawn = null;
					Vector3 offset = Vector3.zero;

					if (dActions.DebugTools.SpawnBasic.WasPerformedThisFrame()) {
						toSpawn = BasicPrefab;
					}
					if (dActions.DebugTools.SpawnExploding.WasPerformedThisFrame()) { 
						toSpawn = ExplodingPrefab;
					}
					if (dActions.DebugTools.SummonSpawnPortal.WasPerformedThisFrame()) {
						OrbSpawnPrefab.GetComponent<OrbSpawn>().basicAmount = 5;
						OrbSpawnPrefab.GetComponent<OrbSpawn>().explodingAmount = 5;
						toSpawn = OrbSpawnPrefab;
						offset = Vector3.up * 5f;
					}

					if (toSpawn != null) {
						Vector2 MouseLocation2D = dActions.DebugTools.MouseLocation.ReadValue<Vector2>();
						Vector3 MouseLocation = new Vector3(MouseLocation2D.x, MouseLocation2D.y, 0);
						Ray ray = Camera.main.ScreenPointToRay(MouseLocation);
						RaycastHit hit;

						if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000.0f)) {
							Instantiate(toSpawn, hit.point + offset, Quaternion.identity);

							if (toSpawn == BasicPrefab || toSpawn == ExplodingPrefab) {
								enemiesAlive += 1;
							}
						}
					}
				}
			}

			if (playerController.pActions.Player.MeterModifier.phase == InputActionPhase.Performed && playerController.pActions.Player.DEBUGRestart.WasPerformedThisFrame()) {
				Debug.Log("Restart called");
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			}
			if (playerController.pActions.Player.MeterModifier.phase == InputActionPhase.Performed && playerController.pActions.Player.DEBUGHeal.WasPerformedThisFrame()) {
				playerController.health = playerController.healthMax;
				playerController.meter = playerController.meterMax;
			}
			if (playerController.pActions.Player.MeterModifier.phase == InputActionPhase.Performed && playerController.pActions.Player.DEBUGLevelSkip.WasPerformedThisFrame()) {
				if (SceneManager.GetActiveScene().buildIndex == 4) { SceneManager.LoadScene(0); }
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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


		// Manage Spawns
		if (enemiesKilledInLevel < enemyKillingGoal && canSpawn) {
			spawnTokens += TokensPerSecond * Time.deltaTime;
			if (enemiesAlive < HighEnemies) {
				// Determine enemy amount
				int amountEnemy = 0;
				float[][] weightEnemyAmount = new float[][]{
					new float[] {0.1f, 3.0f, 6.0f}, // Low Enemies
					new float[] {3.0f, 4.0f, 3.0f}, // Med Enemies
					new float[] {6.0f, 3.0f, 0.1f}, // High Enemies
				};
				int weightIndexEnemyAmount = 0;
				if (enemiesAlive > LowEnemies) { weightIndexEnemyAmount = 1; }
				if (enemiesAlive > TargetEnemies) { weightIndexEnemyAmount = 2; }

				int choiceEnemyAmount = Util.RollWeightedChoice(weightEnemyAmount[weightIndexEnemyAmount]);
				// NOTE(Roskuski): Random.Range(int, int)'s upper bound is EXCLUSIVE. NOT INCLUSIVE.
				switch (choiceEnemyAmount) {
					case 0:
						if (spawnTokens > TokenCost_SmallSpawn) {
							amountEnemy = Random.Range(3, 5);
							spawnTokens -= TokenCost_SmallSpawn;
						}
						break;
					case 1:
						if (spawnTokens > TokenCost_MediumSpawn) {
							amountEnemy = Random.Range(5, 8);
							spawnTokens -= TokenCost_MediumSpawn;
						}
						break;
					case 2:
						if (spawnTokens > TokenCost_BigSpawn) {
							amountEnemy = Random.Range(7, 10);
							spawnTokens -= TokenCost_BigSpawn;
						}
						break;
					default:
						Debug.Assert(false);
						break;
				}


				if (amountEnemy > 0) {
					// detremine enemy contents
					int amountBasic = 0;
					int amountExploding = 0;
					for (int count = 0; count < amountEnemy; count += 1) {
						int choiceEnemyKind = Util.RollWeightedChoice(new float[] {6f, 1f});
						switch (choiceEnemyKind) {
							case 0:
								amountBasic += 1;
								break;
							case 1:
								amountExploding += 1;
								break;
							default:
								Debug.Assert(false);
								break;
						}
					}

					// choose a spawn point
					int indexSpawnPoint = -1;
					do {
						indexSpawnPoint = Random.Range(0, eSpawns.Length);
					} while (Physics.CheckSphere(eSpawns[indexSpawnPoint].position, 10f, Mask.Get(Layers.PlayerHitbox)));

					OrbSpawnPrefab.GetComponent<OrbSpawn>().basicAmount = amountBasic;
					OrbSpawnPrefab.GetComponent<OrbSpawn>().explodingAmount = amountExploding;
					Instantiate(OrbSpawnPrefab, eSpawns[indexSpawnPoint]);
				}
			}
		}

		if (enemiesKilledInLevel >= enemyKillingGoal && enemiesAlive <= 0 && transitioningLevel == false) {
			StartCoroutine(Win());
		}
	}

	IEnumerator Win() {
		transitioningLevel = true;
		if (SceneManager.GetActiveScene().buildIndex == 4) {
			Initializer.timesWon++;
			Initializer.Save();
			statusTextboxText.text = "YOU WIN!!!";
			Debug.Log("YOUR THE BUIGESS FUCKIN WINNER:; DAMMM");
			yield return new WaitForSeconds(5);
			SceneManager.LoadScene(1);
		}
		else {
			Initializer.Save();
			Debug.Log("YOU WIN!! Next stage starting shortly...");
			statusTextboxText.text = "Stage Clear!";
			yield return new WaitForSeconds(5);
			storedPlayerHealth = playerController.health;
			storedPlayerMeter = playerController.meter;
			enemyKillingGoal += 10;
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
		statsButton = transform.Find("OptionsUI/StatsButton").GetComponent<Button>();
		statsButton.Select();
	}

	public void OnStats() {
		optionsUI.enabled = false;
		statsUI.enabled = true;
		statsText = transform.Find("StatsUI/StatsText").GetComponent<TMP_Text>();
		statsBackButton = transform.Find("StatsUI/StatsBackButton").GetComponent<Button>();
		statsText.text = "Enemies Killed: " + Initializer.allEnemiesKilled
			+ "\nRuns started: " + Initializer.runsStarted
			+ "\n Wins: " + Initializer.timesWon;
		statsBackButton.Select();
	}

	public void OnStatsBack() {
		statsText.text = "";
		statsUI.enabled = false;
		optionsUI.enabled = true;
		statsButton.Select();
	}

	public void OnOptionsBack() {
		pauseUI.enabled = true;
		optionsUI.enabled = false;
		resumeButton.Select();
	}

	public void OnQuit() {
		SceneManager.LoadScene(1);
		Time.timeScale = 1;
		Initializer.Save();
	}

	public void OnQuitToDesktop() {
		Initializer.Save();
		Application.Quit();
	}

	void OnEnable() { dActions.Enable(); }
	void OnDisable() { dActions.Disable(); }

	private void OnApplicationQuit() { Initializer.Save(); }
}
