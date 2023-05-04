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
	public enum Objectives : int {
		None = 0,
		KillTheEnemies, // basically done, needs to be better implemented into a more official obj switcher
		DestroyTheShrines,
		HarvestTheCrystals, // workinonit
	}
	public static Objectives currentObjective = 0;

	[Header("Persistent Variables:")]
	public Transform player;
	public PlayerController playerController;
	public static int storedPlayerHealth = 0;
	public static float storedPlayerMeter = 0;
	public static int enemiesKilledInRun = 0;
	public static int enemyKillingGoal = 20;
	public static int crystalHarvestingGoal = 3;
	public static int shrineDestroyingGoal = 3;
	public static int shrineMaxHealth = 15;
	public static int pickupDropChance = 0;
	public static int levelCount = 0;

	[Header("Non-static objective variables:")]
	public int enemiesKilledInLevel = 0;
	public int crystalCount = 0;
	public bool isCrystalEnemyAlive = false;
	public int shrinesDestroyed = 0;
	public Transform waypointTarget;
	public Vector3 waypointOffset;

	[Header("UI")]
	public Canvas mainUI;
	public Image crystalPickupImage;
	public Canvas pauseBG;
	public Button resumeButton;
	public Canvas pauseUI;
	public Canvas optionsUI;
	public Button statsButton;
	public Canvas statsUI;
	public Button statsBackButton;
	public TMP_Text statsText;
	public TMP_Text statsText2;
	public TMP_Text statusTextboxText;
	public TMP_Text objectiveText;
	public TMP_Text waypointDistanceText;
	public Image waypointMarker;

	public Transform healthBar;
	public Transform meterBar;
	public Image meterImage;

	// NOTE(Roskuski): These are in the same order PlayerController.AttackButton (Bottom, Right, Left, Top)
	public Sprite[] attackIconSprites;
	public Image[] attackIconObjects;
	public TMP_Text[] iconText;

	//public GameObject inputDisplayUI;
	public float unlitTextOpacity; //0 = transparent, 1 = opaque;
	public Color tempColorLit;
	public Color tempColorUnlit;

	float objectiveFadeTimer;
	bool isTimerOver = false;
	EventSystem eSystem;

	[Header("Prefabs:")]
	public GameObject PlayerPrefab;
	public HeadProjectile SkullPrefab;
	public GameObject BasicPrefab;
	public GameObject ExplodingPrefab;
	public GameObject NecroPrefab;
	public GameObject NecroProjectilePrefab;
	public GameObject FlashPrefab;
	public GameObject OrbSpawnPrefab;
	public GameObject[] Pickups;
	public GameObject crystalDropoffPrefab;
	public GameObject shrinePrefab;

	[Header("Spawning:")]
	public Transform[] enemySpawnPoints;
	public GameObject[] shrineObjects;
	public Transform[] playerRespawnPoints;
	public Transform crystalDropoffSpawn;
	public int enemiesAlive = 0;
	public int goldenSkullDropChance = 2; //out of 100
	public int goldSkullBuffer = 10;
	bool transitioningLevel = false;
	
	[SerializeField] float spawnTokens;
	float spawnDelay;

	public const float TokenCost_SmallSpawn = 30;
	public const float TokenCost_MediumSpawn = 60;
	public const float TokenCost_BigSpawn = 80;
	public const float TokensPerSecond = 5.0f;
	public const int HighEnemies = 18;
	public const int TargetEnemies = 12;
	public const int LowEnemies = 4;

	[Header("Bools:")]
	public bool updateTimeScale = true;
	public bool canSpawn = true;
	public bool debugTools = true;
	public bool waypointTracking = true;
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

		dActions = new DebugActions();
		eSystem = GetComponent<EventSystem>();
		mainUI = transform.Find("MainUI").GetComponent<Canvas>();
		crystalPickupImage = transform.Find("MainUI/HasCrystalImage").GetComponent<Image>();
		pauseBG = transform.Find("PauseBG").GetComponent<Canvas>();
		resumeButton = transform.Find("PauseUI/ResumeButton").GetComponent<Button>();
		pauseUI = transform.Find("PauseUI").GetComponent<Canvas>();
		optionsUI = transform.Find("OptionsUI").GetComponent<Canvas>();
		statsUI = transform.Find("StatsUI").GetComponent<Canvas>();
		statusTextboxText = transform.Find("StatusTextbox/StatusTextboxText").GetComponent<TMP_Text>();
		statusTextboxText.text = "";
		objectiveText = transform.Find("StatusTextbox/ObjectiveText").GetComponent<TMP_Text>();
		objectiveText.text = "";
		waypointMarker = transform.Find("/GameManager/MainUI/WaypointMarker").GetComponent<Image>();
		waypointDistanceText = transform.Find("/GameManager/MainUI/WaypointMarker/WaypointDistanceText").GetComponent<TMP_Text>();
		meterImage = transform.Find("MainUI/MeterBar").GetComponent<Image>();
		//inputDisplayUI = transform.Find("MainUI/InputDisplay").gameObject;
		Time.timeScale = 1;
		spawnTokens = 100;
		objectiveFadeTimer = 5f;

		UpdatePlayerSpawns();
		
		if (canSpawn) {
			GameObject enemySpawnParent = GameObject.Find("EnemySpawns");
			enemySpawnPoints = enemySpawnParent.GetComponentsInChildren<Transform>();
			Transform[] TempArray = new Transform[enemySpawnPoints.Length - 1];
			for (int index = 0; index < enemySpawnPoints.Length; index += 1) {
				if (index - 1 >= 0) {
					TempArray[index - 1] = enemySpawnPoints[index];
				}
			}
			enemySpawnPoints = TempArray;

			if (SceneManager.GetActiveScene().buildIndex == (int)Scenes.Tutorial) {
				currentObjective = Objectives.None;
			} else {
                float[] objectiveChoices = new float[] { 0f, 4f, 3f, 3f };
                objectiveChoices[(int)currentObjective] = 0;
                //objectiveChoices[(int)Objectives.KillTheEnemies] = 0;
                //objectiveChoices[(int)Objectives.DestroyTheShrines] = 0;
                //objectiveChoices[(int)Objectives.HarvestTheCrystals] = 0;
                // PREVIOUS TWO LINES ARE TEMP while they don't work
                currentObjective = (Objectives)Util.RollWeightedChoice(objectiveChoices);
            }

        }
		else {
			playerController.health = playerController.healthMax;
			playerController.meter = playerController.meterMax;
		}
		crystalDropoffSpawn = transform.Find("/CrystalDropoffSpawn");


		switch (currentObjective) {
			case Objectives.None:
				waypointTracking = false; // temp?
				break;

			case Objectives.KillTheEnemies:
				// assign enemy killing goal to a UI object here
				statusTextboxText.text = "Level " + levelCount +
				"\nKill the Enemies!";
				waypointTracking = false; // temp?
				break;

			case Objectives.DestroyTheShrines:
				int indexSpawnPoint = -1;
				// @TODO(Roskuski): Make sure shrineDestroyGoal cannot exceed spawnpoints. if it does this becomes an infinite loop
				shrineObjects = new GameObject[shrineDestroyingGoal];
				for (int count = 0; count < shrineDestroyingGoal; count++) {
					do {
						indexSpawnPoint = Random.Range(0, enemySpawnPoints.Length);
					} while (Physics.CheckSphere(enemySpawnPoints[indexSpawnPoint].position, 10f, Mask.Get(Layers.EnemyHurtbox)));
					RaycastHit hitInfo;
					bool didHit = Physics.Raycast(enemySpawnPoints[indexSpawnPoint].position, Vector3.down, out hitInfo, 10f, Mask.Get(Layers.Ground));
					Debug.Assert(didHit, "A Shrine failed to find the ground!"); // NOTE(Roskuski) should always happen if spawnpoint are configured correctly!
					shrineObjects[count] = Instantiate(shrinePrefab, hitInfo.point, Quaternion.AngleAxis(180f, Vector3.up), enemySpawnPoints[indexSpawnPoint]);
				}
				statusTextboxText.text = "Level " + levelCount +
				"\nDestroy the Shrines!";
				waypointTracking = false; // temp
				break;

			case Objectives.HarvestTheCrystals:
				Instantiate(crystalDropoffPrefab, crystalDropoffSpawn.position, crystalDropoffSpawn.rotation);
				Debug.Log("Crystal dropoff should have spawned at " + crystalDropoffSpawn.position);
				statusTextboxText.text = "Level " + levelCount +
				"\nHarvest the Crystals!";
				waypointTarget = crystalDropoffSpawn;
				break;

			default:
				statusTextboxText.text = "Level " + levelCount +
				"\nsomething is wrong";
				waypointTracking = false;
				break;
		}

		tempColorLit.a = 1f;
		tempColorUnlit.a = unlitTextOpacity;
	}

	private void Update() {
		if (updateTimeScale) {
			if (frozenTime > 0) {
				if (frozenTime > 4) frozenTime = 4;
				Time.timeScale = 0.0f;
				frozenTime -= Time.unscaledDeltaTime;
			}
			else {
				Time.timeScale = 1.0f;
			}
		}

		if (!isTimerOver) {
			objectiveFadeTimer -= Time.deltaTime;
			if (objectiveFadeTimer <= 0) {
				isTimerOver = true;
				statusTextboxText.text = "";
			}
		}

		bool isMenuOpen = pauseUI.enabled || optionsUI.enabled;

		//pickup drop chance adjustment
		if (playerController.meter < playerController.meterMax / 2) {
			pickupDropChance = 90;
		} else if (playerController.meter < playerController.meterMax / 7) {
			pickupDropChance = 65;
		} else {
			pickupDropChance = 25;
		}

		//Objective text setter
		switch (currentObjective) {
			case Objectives.None:
				break;

			case Objectives.KillTheEnemies:
				objectiveText.text = "Enemies killed: " + enemiesKilledInLevel + "/" + enemyKillingGoal;
				if (enemiesKilledInLevel >= enemyKillingGoal && enemiesAlive <= 0 && transitioningLevel == false) {
					StartCoroutine(Win());
				}
				break;

			case Objectives.DestroyTheShrines:
				objectiveText.text = "Shrines destroyed: " + shrinesDestroyed + "/" + shrineDestroyingGoal;
				if (shrinesDestroyed >= shrineDestroyingGoal && transitioningLevel == false) {
					StartCoroutine(Win());
				}
				break;

			case Objectives.HarvestTheCrystals:
				objectiveText.text = "Crystals harvested: " + crystalCount + "/" + crystalHarvestingGoal;
				if (crystalCount >= crystalHarvestingGoal && transitioningLevel == false) {
					StartCoroutine(Win());
				}
				break;

			default:
				objectiveText.text = "something is wrong";
				Debug.Assert(false, "Invalid objective " + currentObjective);
				break;
		}

		UpdateHealthBar();
		UpdateMeter();
		UpdateIcons(); //if (inputDisplayUI.activeSelf == true) {  }

		// Manage Spawns
		if (canSpawn && !transitioningLevel) {
			spawnTokens += TokensPerSecond * Time.deltaTime;
			if (enemiesAlive < HighEnemies) {
				// Determine enemy amount
				int amountEnemy = 0;
				float[] weightEnemyAmount = new float[4]{
					0f, // No Spawn
					Mathf.Lerp(0f, 5f, 1 - (Mathf.Abs(HighEnemies - enemiesAlive) / TargetEnemies)), // Small Spawn 
					Mathf.Lerp(0f, 7f, 1 - (Mathf.Abs(TargetEnemies - enemiesAlive) / TargetEnemies)), // Med Spawn
					Mathf.Lerp(0f, 5f, 1 - (Mathf.Abs(LowEnemies - enemiesAlive) / TargetEnemies)), // Large Spawn
				};

				switch (currentObjective) {
					case Objectives.None:
						weightEnemyAmount = new float[] { 1f };
						break;

					case Objectives.KillTheEnemies:
						if (enemiesAlive + enemiesKilledInLevel >= enemyKillingGoal) {
							weightEnemyAmount = new float[] { 1f };
						}
						break;

					case Objectives.DestroyTheShrines:
						break;

					case Objectives.HarvestTheCrystals:
						// @TODO(Roskuski): Do spawn pacing mechanics
						break;
				}

				int choiceEnemyAmount = Util.RollWeightedChoice(weightEnemyAmount);
				// NOTE(Roskuski): Random.Range(int, int)'s upper bound is EXCLUSIVE. NOT INCLUSIVE.
				switch (choiceEnemyAmount) {
					case 0:
						break;

					case 1:
						if (spawnTokens > TokenCost_SmallSpawn) {
							amountEnemy = Random.Range(3, 5);
							spawnTokens -= TokenCost_SmallSpawn;
						}
						break;

					case 2:
						if (spawnTokens > TokenCost_MediumSpawn) {
							amountEnemy = Random.Range(5, 8);
							spawnTokens -= TokenCost_MediumSpawn;
						}
						break;

					case 3:
						if (spawnTokens > TokenCost_BigSpawn) {
							amountEnemy = Random.Range(7, 10);
							spawnTokens -= TokenCost_BigSpawn;
						}
						break;

					default:
						Debug.Assert(false);
						break;
				}

				switch (currentObjective) {
					case Objectives.KillTheEnemies:
						if (amountEnemy > (enemiesAlive + enemiesKilledInLevel - enemyKillingGoal) && enemiesAlive + enemiesKilledInLevel >= enemyKillingGoal) {
							amountEnemy = (enemiesAlive + enemiesKilledInLevel - enemyKillingGoal);
						}
						break;
					default:
						break;
				}

				if (amountEnemy > 0) {
					// detremine enemy contents
					int amountBasic = 0;
					int amountExploding = 0;
					for (int count = 0; count < amountEnemy; count += 1) {
						int choiceEnemyKind = Util.RollWeightedChoice(new float[] {9f, 1f});
						switch (choiceEnemyKind) {
							case 0:
								amountBasic += 1;
								break;

							case 1:
								amountExploding += 1;
								// @TODO(Roskuski): We should probably cap Exploding spawn count so you can't roll a lot of them.
								break;

							default:
								Debug.Assert(false);
								break;
						}
					}

					OrbSpawnPrefab.GetComponent<OrbSpawn>().basicAmount = amountBasic;
					OrbSpawnPrefab.GetComponent<OrbSpawn>().explodingAmount = amountExploding;

					// choose a spawn point
					switch (currentObjective) {
						case Objectives.None:
							break;

						case Objectives.KillTheEnemies:
							{
								float[] spawnPointWeights = new float[enemySpawnPoints.Length];
								for (int index = 0; index < enemySpawnPoints.Length; index += 1) {
									if (!Physics.CheckSphere(enemySpawnPoints[index].position, 10f, Mask.Get(Layers.PlayerHitbox)) && enemySpawnPoints[index].transform.Find("OrbSpawnV2(Clone)") == null) {
										spawnPointWeights[index] = 1f;
									}
									else {
										spawnPointWeights[index] = 0f;
									}
								}

								int spawnChoice = Util.RollWeightedChoice(spawnPointWeights);
								Instantiate(OrbSpawnPrefab, enemySpawnPoints[spawnChoice]);
							}
							break;

						case Objectives.DestroyTheShrines:
							{
								float[] distanceToPlayer = new float[shrineObjects.Length];
								float minDistance = 100;
								int minDistanceIndex = -1;
								for (int index = 0; index < shrineObjects.Length; index += 1) {
									distanceToPlayer[index] = Vector3.Distance(shrineObjects[index].transform.position, playerController.transform.position);
									if (distanceToPlayer[index] < minDistance) {
										minDistance = distanceToPlayer[index];
										minDistanceIndex = index;
									}
								}
								
								float[] spawnPointWeights = new float[distanceToPlayer.Length];
								for (int index = 0; index < shrineObjects.Length; index += 1) {
									spawnPointWeights[index] = minDistance / distanceToPlayer[index];
									if (enemySpawnPoints[index].transform.Find("OrbSpawnV2(Clone)") != null) {
										spawnPointWeights[index] = 0f;
									}
								}

								int shrineChoice = Util.RollWeightedChoice(spawnPointWeights);
								Instantiate(OrbSpawnPrefab, shrineObjects[shrineChoice].transform.parent.position + Vector3.up * 5.4f, Quaternion.identity, shrineObjects[shrineChoice].transform.parent);
							}
							break;

						case Objectives.HarvestTheCrystals:
							{
								// @TODO(Roskuski): We should do crystal chance here.
								float[] spawnPointWeights = new float[enemySpawnPoints.Length];
								for (int index = 0; index < enemySpawnPoints.Length; index += 1) {
									if (!Physics.CheckSphere(enemySpawnPoints[index].position, 10f, Mask.Get(Layers.PlayerHitbox)) && enemySpawnPoints[index].transform.Find("OrbSpawnV2(Clone)") == null) {
										spawnPointWeights[index] = 1f;
									}
									else {
										spawnPointWeights[index] = 0f;
									}
								}

								int spawnChoice = Util.RollWeightedChoice(spawnPointWeights);
								Instantiate(OrbSpawnPrefab, enemySpawnPoints[spawnChoice]);
							}
							break;

						default:
							Debug.Assert(false, "Invalid Objective " + currentObjective);
							break;
					}
				}
			}
		}

		// respawn player if falling
		if (playerController.transform.position.y <= -20f) {
			playerController.movement = Vector3.zero; playerController.mInput = Vector2.zero; playerController.remainingKnockbackTime = 0f;

			int[] enemyCounts = new int[playerRespawnPoints.Length];
			int leastEnemyIndex = -1;
			int leastEnemyAmount = 1000;
			for (int index = 0; index < playerRespawnPoints.Length; index += 1) {
				enemyCounts[index] = Physics.OverlapSphere(playerRespawnPoints[index].position, 5f, Mask.Get(Layers.EnemyHurtbox)).Length;
				if (enemyCounts[index] < leastEnemyAmount) {
					leastEnemyAmount = enemyCounts[index];
					leastEnemyIndex = index;
				}
			}

			float[] spawnPointWeights = new float[playerRespawnPoints.Length];
			for (int index = 0; index < playerRespawnPoints.Length; index += 1) {
				if (leastEnemyAmount == 0) {
					if (enemyCounts[index] == 0) {
						spawnPointWeights[index] = 1f;
					}
					else {
						spawnPointWeights[index] = 0f;
					}
				}
				else {
					if (index == leastEnemyIndex) {
						spawnPointWeights[index] = 1f;
					}
					else {
						spawnPointWeights[index] = 0f;
					}
				}
			}
			int spawnPointIndex = Util.RollWeightedChoice(spawnPointWeights);
			playerController.transform.position = playerRespawnPoints[spawnPointIndex].position;

			if (currentObjective != Objectives.None && transitioningLevel == false) playerController.Hit(1, null);
		}

		if (waypointTracking) {
			Vector2 markerScreenPosition = new Vector2(-1, -1);
			Vector3 targetPosition = waypointTarget.position + waypointOffset;

			Plane[] cameraPlanes = GeometryUtility.CalculateFrustumPlanes(UnityEngine.Camera.main);
			if (GeometryUtility.TestPlanesAABB(cameraPlanes, new Bounds(targetPosition, new Vector3(0.1f, 0.1f, 0.1f)))) {
				markerScreenPosition = UnityEngine.Camera.main.WorldToScreenPoint(targetPosition);
			}
			else {
				float minDistance = Mathf.Infinity;
				Ray rayToTarget = new Ray(playerController.transform.position, targetPosition - playerController.transform.position);
				foreach (Plane plane in cameraPlanes) {
					float distance;
					if (plane.Raycast(rayToTarget, out distance)) {
						if (minDistance > distance) {
							minDistance = distance;
						}
					}
				}

				if (minDistance != Mathf.Infinity) {
					Vector3 markerPosition = rayToTarget.GetPoint(minDistance);
					markerScreenPosition = UnityEngine.Camera.main.WorldToScreenPoint(markerPosition);
				}
			}

			// @TODO(Roskuski): These limts should respect the UI. Also I think it would look cool if the limit was shaped like an oval.
			// NOTE(Roskuski): Below assumes the anchor of the marker element is in the center.
			Vector2 MarkerMin = new Vector2(waypointMarker.GetPixelAdjustedRect().width / 2, waypointMarker.GetPixelAdjustedRect().height / 2);
			Vector2 MarkerMax = new Vector2(Screen.width - MarkerMin.x, Screen.height - MarkerMin.y);
			markerScreenPosition.x = Mathf.Clamp(markerScreenPosition.x, MarkerMin.x, MarkerMax.x);
			markerScreenPosition.y = Mathf.Clamp(markerScreenPosition.y, MarkerMin.y, MarkerMax.y);

			waypointMarker.transform.position = markerScreenPosition;
			// Change the meter text to the distance with the meter unit 'm'
			waypointDistanceText.text = ((int)Vector3.Distance(waypointTarget.position, player.transform.position)).ToString() + "m";
		}
	

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
					if (dActions.DebugTools.SpawnNecro.WasPerformedThisFrame()) {
						toSpawn = NecroPrefab;
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
						Ray ray = UnityEngine.Camera.main.ScreenPointToRay(MouseLocation);
						RaycastHit hit;

						if (Physics.Raycast(ray.origin, ray.direction, out hit, 1000.0f)) {
							Instantiate(toSpawn, hit.point + offset, Quaternion.identity);

							if (toSpawn == BasicPrefab || toSpawn == ExplodingPrefab || toSpawn == NecroPrefab) {
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
				float[] sceneChances = new float[] { 0, 1f, 1f, 1f };
				sceneChances[SceneManager.GetActiveScene().buildIndex] = 0;
				SceneManager.LoadScene(Util.RollWeightedChoice(sceneChances));
			}
			if (playerController.pActions.Player.MeterModifier.phase == InputActionPhase.Performed && playerController.pActions.Player.DEBUGGodmode.WasPerformedThisFrame()) {
				if (playerController.godMode) playerController.godMode = false;
				else playerController.godMode = true;
			}
			if (playerController.pActions.Player.MeterModifier.phase == InputActionPhase.Performed && playerController.pActions.Player.DEBUGKillAll.WasPerformedThisFrame()) {
				if (canSpawn) {
					KillAll();
					canSpawn = false;
				}
				else canSpawn = true;
			}
			/*if (playerController.pActions.Player.DEBUGDisableUI.WasPerformedThisFrame()) {
				if (inputDisplayUI.activeSelf == true) { inputDisplayUI.SetActive(false); }
				else { inputDisplayUI.SetActive(true); }
			}*/
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
	}

	public void UpdatePlayerSpawns() {
		GameObject parent = GameObject.Find("PlayerRespawnPoints");
		if (parent != null) {
			Transform[] points = parent.GetComponentsInChildren<Transform>();
			Transform[] TempArray = new Transform[points.Length - 1];
			for (int index = 0; index < points.Length; index += 1) {
				if (index - 1 >= 0) {
					TempArray[index - 1] = points[index];
				}
			}
			playerRespawnPoints = TempArray;
		}
	}

	public IEnumerator Win() {
		transitioningLevel = true;
		if (levelCount > Initializer.save.versionLatest.longestRun) { Initializer.save.versionLatest.longestRun = levelCount; }
		Initializer.Save();
		Debug.Log("YOU WIN!! Next stage starting shortly...");
		statusTextboxText.text = "Stage Clear!";
		float[] sceneChances = new float[] { 0, 1f, 1f, 1f };
		sceneChances[SceneManager.GetActiveScene().buildIndex] = 0;
		KillAll();
		yield return new WaitForSeconds(5);
		if (playerController.currentState == PlayerController.States.Death) {
			SceneManager.LoadScene((int)Scenes.Tutorial);
		} else {
			storedPlayerHealth = playerController.health;
			storedPlayerMeter = playerController.meter;
			levelCount++;
			switch (currentObjective) {
				case Objectives.KillTheEnemies:
					enemyKillingGoal += 10;
					break;

				case Objectives.DestroyTheShrines:
					shrineMaxHealth += 5;
					if (shrineMaxHealth >= 26f) {
						if (shrineDestroyingGoal <= 5) break;
						else if (shrineDestroyingGoal == 4) {
							shrineDestroyingGoal++;
							shrineMaxHealth -= 10;
						}
						else {
							shrineDestroyingGoal++;
							shrineMaxHealth -= 15;
						}
					}
					break;

				case Objectives.HarvestTheCrystals:
					crystalHarvestingGoal += 1;
					break;

				default:
					Debug.Assert(false, "Won with an invalid Objective " + currentObjective);
					break;
			}
			SceneManager.LoadScene(Util.RollWeightedChoice(sceneChances));
		}
	}

	// NOTE(Ryan): Can be called to freeze the game for the time specified.
	// Frames60 is the amount of time, based on a 16.66ms long frame
	public void FreezeFrames(int Frames60) {
		frozenTime += (float)(Frames60) / 60.0f;
	}

	// @TODO(Roskuski): Particles' parent objects do not destory themselves after their particles have. this will leave empty gameobjects lying around the scene as the game progresses, but since we don't have respawning destructible props we should have to worry about this leak.
	public void SpawnParticle(int particleID, Vector3 position, float scale) {
		ParticleSystem particle = particles[particleID];
		var TempParticle = Instantiate(particle, position, particle.gameObject.transform.rotation);
		TempParticle.gameObject.transform.localScale *= scale;
		Transform[] particleScales = TempParticle.transform.GetComponentsInChildren<Transform>();
		foreach (Transform t in particleScales) {
			t.localScale *= scale;
		}
	}

	public void DeterminePickups(Vector3 position, bool isCrystallized) {
		float meterBeforeUse = playerController.meter + PlayerController.AttackMeterCost[(int)playerController.currentAttack];
		if (playerController.currentAttack == PlayerController.Attacks.None) {
			meterBeforeUse = playerController.meter;
		}

		float skullChance = (60 / playerController.meterMax) * (playerController.meterMax - meterBeforeUse);
		int healthChance = (60 / playerController.healthMax) * (playerController.healthMax - playerController.health);

		if (isCrystallized == true && playerController.hasCrystal == false) {
			skullChance = 0;
			healthChance = 0;
			SpawnPickup((int)Pickup.Type.Crystal, position);
		}

		//Skull Pickup
		float pickupDecider = Random.Range(1, 100);
		if (pickupDecider <= skullChance) { //check for skulldrop
			if (pickupDecider <= goldenSkullDropChance && goldSkullBuffer <= 0) {
				SpawnPickup((int)Pickup.Type.GoldenSkull, position); //check for goldenskull
				goldSkullBuffer = 50;
			}
			else {
				SpawnPickup((int)Pickup.Type.Skull, position);
				if (goldSkullBuffer > 0) goldSkullBuffer--;
			}
		}

		//Health Pickup
		pickupDecider = Random.Range(1, 100);
		if (pickupDecider <= healthChance) SpawnPickup((int)Pickup.Type.Health, position); //check for healthdrop
		
	}

	public void SpawnPickup(int pickupID, Vector3 position) {
		Instantiate(Pickups[pickupID], position, Quaternion.identity);
	}

	public void UpdateHealthBar() {
		float healthMax = playerController.healthMax;
		float health = playerController.health;
		healthBar.localScale = new Vector3((health / healthMax), 1f, 1f);
	}

	public void UpdateMeter() {
		meterBar.localScale = new Vector3((playerController.meter / playerController.meterMax), 1f, 1f);
		if (playerController.frenzyTimer > 0) { meterImage.color = Color.yellow; }
		else { meterImage.color = Color.white; }
	}

	readonly string[] PlayerAttackToName = {
		"",
		"QUICK ATTACK",
		"FOLLOW UP",
		"FINISHER",
		"CHOP",
		"SLAM",
		"SPIN",
		"THROW",
		"DASH",
		"SUPER DASH",
		"SHOTGUN"
	};

	public void UpdateIcons() {
		// NOTE(Roskuski): 4 is the index of the ui meter button 

		if (playerController.meter >= 0.2f) { //Can I use meter?
			attackIconObjects[4].color = tempColorLit;
			iconText[4].color = tempColorLit;
		}
		else {
			var tempColor = attackIconObjects[4].color;
			tempColor.a = 0.15f;
			attackIconObjects[4].color = tempColor;
			iconText[4].color = tempColor;
		}
		
		PlayerController.QueueInfo[] availableQueueInfos = PlayerController.QueueInfoTable[(int)playerController.currentAttack];

		for (int index = (int)PlayerController.AttackButton.LightAttack; index <= (int)PlayerController.AttackButton.Dash; index += 1) {
			PlayerController.AttackButton attackButton = (PlayerController.AttackButton)index;
			if (playerController.pActions.Player.MeterModifier.phase == InputActionPhase.Performed) { //Am I currently trying to use meter?
				attackButton = (PlayerController.AttackButton)((int)index + 4); // NOTE(Roskuski): The Mod enum values are 4 away from the non-mod enum values
			}

			// NOTE(Roskuski) index here is off by one from the perspective of attackIconObjects Array
			// PlayerController.AttackButton.LightAttack corrisponds with attackIconObjects[0]
			PlayerController.Attacks nextAttack = availableQueueInfos[(int)attackButton].nextAttack;
			attackIconObjects[index - 1].sprite = attackIconSprites[(int)nextAttack];
			iconText[index - 1].text = PlayerAttackToName[(int)nextAttack];

			bool canUse = false;
			if (playerController.CanAffordMove(nextAttack)) {
				canUse = true;

				if (playerController.currentState == PlayerController.States.Attacking) {
					if ((availableQueueInfos[(int)attackButton].startQueuePercent < playerController.animr.GetCurrentAnimatorStateInfo(0).normalizedTime &&
						   availableQueueInfos[(int)attackButton].endQueuePercent > playerController.animr.GetCurrentAnimatorStateInfo(0).normalizedTime)) {
						canUse = true;
					}
					else {
						canUse = false;
					}
				}
			}

			if (canUse) {
				attackIconObjects[index - 1].color = tempColorLit;
				iconText[index - 1].color = tempColorLit;
			}
			else {
				attackIconObjects[index - 1].color = tempColorUnlit;
				iconText[index - 1].color = tempColorUnlit;
			}

			if (playerController.queuedAttackInfo.nextAttack == nextAttack && nextAttack != PlayerController.Attacks.None) {
				attackIconObjects[index - 1].color = Color.yellow;
				iconText[index - 1].color = Color.yellow;
			}
		}
	}

	public void KillAll() {
		OrbSpawn[] allOrbs = FindObjectsOfType<OrbSpawn>();
		foreach (OrbSpawn orb in allOrbs) {
			Destroy(orb.gameObject);
		}
		Basic[] allBasic = FindObjectsOfType<Basic>();
		foreach (Basic basicEnemy in allBasic) {
			Destroy(basicEnemy.gameObject);
		}
		Exploding[] allExplosive = FindObjectsOfType<Exploding>();
		foreach (Exploding explodingEnemy in allExplosive) {
			Destroy(explodingEnemy.gameObject);
		}
		Necro[] allNecro = FindObjectsOfType<Necro>();
		foreach (Necro necroEnemy in allNecro) {
			Destroy(necroEnemy.gameObject);
		}
	}

	public void OnResume() {
		eSystem.SetSelectedGameObject(null);
		updateTimeScale = true;
		Time.timeScale = 1;
		pauseUI.enabled = false;
		pauseBG.enabled = false;
	}

	public void OnRestart() {
		MenuManager.OnPlay();
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
		statsText2 = transform.Find("StatsUI/StatsText2").GetComponent<TMP_Text>();
		statsBackButton = transform.Find("StatsUI/StatsBackButton").GetComponent<Button>();
		statsText.text =
			"<b>ENEMIES:</b>" +
			"\nTotal Kills: " + (Initializer.save.versionLatest.basicEnemyKills + Initializer.save.versionLatest.explosiveEnemyKills + Initializer.save.versionLatest.necroEnemyKills + Initializer.save.versionLatest.bruteEnemyKills)
			+ (Initializer.save.versionLatest.basicEnemyKills > 0 ? "\nBasic: " + Initializer.save.versionLatest.basicEnemyKills : "\n??? : ???")
			+ (Initializer.save.versionLatest.explosiveEnemyKills > 0 ? "\nBomb Spiders: " + Initializer.save.versionLatest.explosiveEnemyKills : "\n??? : ???")
			+ (Initializer.save.versionLatest.necroEnemyKills > 0 ? "\nNecromancers: " + Initializer.save.versionLatest.necroEnemyKills : "\n??? : ???")
			+ (Initializer.save.versionLatest.bruteEnemyKills > 0 ? "\nBrutes: " + Initializer.save.versionLatest.bruteEnemyKills : "\n??? : ???");
		statsText2.text =
			"<b>RUNS:</b>"
			+ "\nRuns started: " + Initializer.save.versionLatest.runsStarted
			+ (Initializer.save.versionLatest.longestRun > 0 ? "\nLongest run: " + Initializer.save.versionLatest.longestRun + " Levels" : "\n??? : ???");
			//+ (Initializer.save.versionLatest.timesWon > 0 ? "\nWins: " + Initializer.save.versionLatest.timesWon : "\n??? : ???");
		statsBackButton.Select();
	}

	public void OnStatsBack() {
		statsText.text = "";
		statsText2.text = "";
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
		SceneManager.LoadScene((int)Scenes.Tutorial);
		enemiesKilledInRun = 0;
		Time.timeScale = 1;
		Initializer.Save();
	}

	public void OnQuitToDesktop() {
		enemiesKilledInRun = 0;
		Initializer.Save();
		Application.Quit();
	}

	void OnEnable() { dActions.Enable(); }
	void OnDisable() { dActions.Disable(); }

	private void OnApplicationQuit() { Initializer.Save(); }
}
