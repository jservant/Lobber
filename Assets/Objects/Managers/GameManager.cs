using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
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
	public CameraShake cameraShake;
	public VignetteEffect vignette;
	public static int storedPlayerHealth = 0;
	public static float storedPlayerMeter = 0;
	public static int enemiesKilledInRun = 0;
	public static int enemyKillingGoal = 15;
	public static int crystalHarvestingGoal = 2;
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
	public TMP_Text crystalCountText;
	public Canvas pauseBG;
	public Button resumeButton;
	public Button tutorialSkipButton;
	public Canvas tutorialSkipUI;
	public CanvasGroup tutorialSkipGroup;
	public Canvas pauseUI;
	public CanvasGroup pauseGroup;
	public Canvas cancelConfirmUI;
	public CanvasGroup cancelConfirmGroup;
	public Canvas optionsUI;
	public CanvasGroup optionsGroup;
	public Canvas audioUI;
	public CanvasGroup audioGroup;
	public TMP_Dropdown resolutionDropdown;
	public TMP_Dropdown graphicsDropdown;
	public Button optionsBackButton;
	public Canvas statsUI;
	public CanvasGroup statsGroup;
	public Button statsBackButton;
    public Canvas creditsUI;
    public CanvasGroup creditsGroup;
    public TMP_Text statsText;
	public TMP_Text statsText2;
	public TMP_Text statusTextboxText;
	public TMP_Text objectiveText;
	public TMP_Text debugText;
	public Image waypointMarker;

	public Material healthBar;
	public Material meterBar;
	public Material healthBarDrain;
	public float barDrainTime = 0f;
	public float previousHealth = 10f;
	public float meterCostFlashTime = 0f;
	public Image[] meterCostSegments;

	/*public Transform healthBar;
	public Transform meterBar;
	public Image meterImage;*/ //Old healthbar transforms

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
	public GameObject SandbagPrefab;
	public GameObject OrbSpawnPrefab;
	public GameObject[] Pickups;
	public int maxPickupsInAir;
	private float pickupTime;
	public List<GameObject> pickupsInAir = new List<GameObject>();
	public GameObject crystalDropoffPrefab;
	public GameObject shrinePrefab;
	public GameObject crystalPatch;

	[Header("Spawning:")]
	public Transform[] enemySpawnPoints;
	public GameObject[] shrineObjects;
	public Transform[] playerRespawnPoints;
	public Transform[] crystalDropoffSpawns;
	public int enemiesAlive = 0;
	public int goldenSkullDropChance = 2; //out of 100
	public int goldSkullBuffer = 10;

	[SerializeField] float spawnTokens;

	[Header("Spawner Variables:")]
	public static float TokenCost_SmallSpawn = 20;
	public static float TokenCost_MediumSpawn = 40;
	public static float TokenCost_BigSpawn = 80;
	public static float TokensPerSecond = 3.5f;
	public static int HighEnemies = 6;
	public static int TargetEnemies = 4;
	public static int LowEnemies = 2;
	public static int SmallSpawn_Low = 3;
	public static int SmallSpawn_High = 4;
	public static int MediumSpawn_Low = 4;
	public static int MediumSpawn_High = 5;
	public static int BigSpawn_Low = 5;
	public static int BigSpawn_High = 7;
	public static float BasicWeight = 9f;
	public static float ExplodingWeight = 0f;
	public static float NecroWeight = 0f;
	public static float armoredEnemyChance = 0f;
	public static float armorChanceLow = 0f;
	public static float armorChanceHigh = 0f;

	[Header("Bools:")]
	public bool updateTimeScale = true;
	public bool canSpawn = true;
	public bool debugTools = true;
	public bool debugTextActive;
	public bool waypointTracking = true;
	public bool armorEnabled;
	DebugActions dActions;
	float frozenTime = 0;
	bool transitioningLevel = false;
	public static bool shouldWipeSave = false;

	[Header("Particle System:")]
	public ParticleSystem[] particles;
	public GameObject[] flashes;
	public GameObject[] corpses;

	public RenderPipelineAsset[] qualityLevels;
	public static Resolution[] resolutions;

	void Awake() {
		//Initializer.Load();
		player = transform.Find("/Player");
		playerController = player.GetComponent<PlayerController>();
		if (player != null) {
			Debug.Log("Object Named Player found");
		}
		else Debug.LogWarning("Object Named Player Not found");

		cameraShake = transform.Find("/CameraPoint").GetComponentInChildren<CameraShake>();
		vignette = transform.Find("/CameraPoint").GetComponentInChildren<VignetteEffect>();
		dActions = new DebugActions();
		eSystem = GetComponent<EventSystem>();
		mainUI = transform.Find("MainUI").GetComponent<Canvas>();
		crystalPickupImage = transform.Find("MainUI/HasCrystalImage").GetComponent<Image>();
		crystalCountText = transform.Find("MainUI/CrystalCountText").GetComponent<TMP_Text>();
		crystalCountText.text = "";
		pauseBG = transform.Find("PauseBG").GetComponent<Canvas>();
		resumeButton = transform.Find("PauseUI/ResumeButton").GetComponent<Button>();
		tutorialSkipUI = transform.Find("TutorialSkipUI").GetComponent<Canvas>();
		tutorialSkipGroup = transform.Find("TutorialSkipUI").GetComponent<CanvasGroup>();
		tutorialSkipButton = transform.Find("TutorialSkipUI/YesButton").GetComponent<Button>();
		pauseUI = transform.Find("PauseUI").GetComponent<Canvas>();
		pauseGroup = transform.Find("PauseUI").GetComponent<CanvasGroup>();
		cancelConfirmUI = transform.Find("CancelConfirmUI").GetComponent<Canvas>();
		cancelConfirmGroup = transform.Find("CancelConfirmUI").GetComponent<CanvasGroup>();
		optionsUI = transform.Find("OptionsUI").GetComponent<Canvas>();
		optionsGroup = transform.Find("OptionsUI").GetComponent<CanvasGroup>();
		audioUI = transform.Find("AudioUI").GetComponent<Canvas>();
		audioGroup = transform.Find("AudioUI").GetComponent<CanvasGroup>();
		resolutionDropdown = transform.Find("OptionsUI/VisualSettings/Resolution/ResolutionDropdown").GetComponent<TMP_Dropdown>();
		graphicsDropdown = transform.Find("OptionsUI/VisualSettings/Graphics/GraphicsDropdown").GetComponent<TMP_Dropdown>();
		statsUI = transform.Find("StatsUI").GetComponent<Canvas>();
		statsGroup = transform.Find("StatsUI").GetComponent<CanvasGroup>();
		creditsUI = transform.Find("CreditsUI").GetComponent<Canvas>();
		creditsGroup = transform.Find("CreditsUI").GetComponent<CanvasGroup>();
		statusTextboxText = transform.Find("StatusTextbox/StatusTextboxText").GetComponent<TMP_Text>();
		statusTextboxText.text = "";
		objectiveText = transform.Find("StatusTextbox/ObjectiveText").GetComponent<TMP_Text>();
		objectiveText.text = "";
		waypointMarker = transform.Find("/GameManager/MainUI/WaypointMarker").GetComponent<Image>();
		//meterImage = transform.Find("MainUI/MeterBar").GetComponent<Image>();
		//inputDisplayUI = transform.Find("MainUI/InputDisplay").gameObject;
		Time.timeScale = 1;
		spawnTokens = TokenCost_BigSpawn;
		objectiveFadeTimer = 5f;
		pickupTime = 0;

		int resolutionIndex = 0;
		resolutions = Screen.resolutions;
		resolutionDropdown.ClearOptions();
		List<string> resolutionOptions = new List<string>();
		for (int i = 0; i < resolutions.Length; i++) {
			string resolutionOption = resolutions[i].width + " x " + resolutions[i].height + " @ " + resolutions[i].refreshRate + "hz";
			resolutionOptions.Add(resolutionOption);

			if (Initializer.save.versionLatest.resolutionOption != 0) { resolutionIndex = Initializer.save.versionLatest.resolutionOption; }
			else if (resolutions[i].width == Screen.currentResolution.width && resolutions[i].height == Screen.currentResolution.height) {
				resolutionIndex = i;
				Initializer.save.versionLatest.resolutionOption = resolutionIndex;
			}
		}

		resolutionDropdown.AddOptions(resolutionOptions);
		resolutionDropdown.value = resolutionIndex;
		resolutionDropdown.RefreshShownValue();

		int qualityIndex = 0;
		graphicsDropdown.ClearOptions();
		List<string> qualityOptions = new List<string>();
		for (int i = 0; i < qualityLevels.Length; i++) {
			string qualityOption = qualityLevels[i].name;
			qualityOptions.Add(qualityOption);

			if (QualitySettings.GetQualityLevel() == i) {
				qualityIndex = i;
			}
		}
		graphicsDropdown.AddOptions(qualityOptions);
		graphicsDropdown.value = qualityIndex;
		graphicsDropdown.RefreshShownValue();

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

			float[] objectiveChoices = new float[] { 0f, 6f, 0f, 4f };
			objectiveChoices[(int)currentObjective] = 0;
			currentObjective = (Objectives)Util.RollWeightedChoice(objectiveChoices);
		}
		else {
			currentObjective = Objectives.None;
			playerController.health = playerController.healthMax;
			playerController.meter = 0;
		}


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
				Transform dropoff;
				dropoff = crystalDropoffSpawns[Random.Range(0, crystalDropoffSpawns.Length)];
				Instantiate(crystalDropoffPrefab, dropoff.position, dropoff.rotation);
				Debug.Log("Crystal dropoff should have spawned at " + dropoff.position);
				statusTextboxText.text = "Level " + levelCount +
				"\nHarvest the Crystals!";
				waypointTarget = dropoff;
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

		//pickup drop chance adjustment
		if (playerController.meter < playerController.meterMax / 7) {
			pickupDropChance = 90;
		} else if (playerController.meter < playerController.meterMax / 2) {
			pickupDropChance = 65;
		} else {
			pickupDropChance = 25;
		}

		if (pickupTime > 0) {
			pickupTime -= Time.deltaTime;
		}
		else if (pickupsInAir.Count > 0) pickupsInAir.Clear();

		//Objective text setter
		switch (currentObjective) {
			case Objectives.None:
				break;

			case Objectives.KillTheEnemies:
				if (enemiesKilledInLevel > enemyKillingGoal) enemiesKilledInLevel = enemyKillingGoal;
				objectiveText.text = "Enemies killed: " + enemiesKilledInLevel + "/" + enemyKillingGoal;
				if (enemiesKilledInLevel >= enemyKillingGoal && transitioningLevel == false) {
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
		if (debugTextActive) debugText.text = "LevelCount: " + levelCount +
			"\n" + "Tokens per Second: " + TokensPerSecond +
			"\n" + "Current Tokens:  " + Mathf.RoundToInt(spawnTokens) +
			"\n" + "Spawn Rates- " +
			"\n" + "Small: " + TokenCost_SmallSpawn +
			"\n" + "Med:   " + TokenCost_MediumSpawn +
			"\n" + "Large: " + TokenCost_BigSpawn +
			"\n" + "Enemy Numbers- " +
			"\n" + "Low:    " + LowEnemies +
			"\n" + "Target: " + TargetEnemies +
			"\n" + "High:   " + HighEnemies +
			"\n" + "Enemy Spawns- " +
			"\n" + "Small: " + SmallSpawn_Low + ", " + SmallSpawn_High +
			"\n" + "Med:   " + MediumSpawn_Low + ", " + MediumSpawn_High +
			"\n" + "Large: " + BigSpawn_Low + ", " + BigSpawn_High +
			"\n" + "Enemies Alive: " + enemiesAlive +
			"\n" + "Basic Weight: " + BasicWeight +
			"\n" + "Explo Weight: " + ExplodingWeight +
			"\n" + "Necro Weight: " + NecroWeight +
			"\n" + "Armor Chance: " + armoredEnemyChance;


		// Manage Spawns
		if (canSpawn && !transitioningLevel) {
			float tps = TokensPerSecond;
			if (enemiesAlive < 1) tps = TokensPerSecond * 3f;
			spawnTokens += tps * Time.deltaTime;
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
							amountEnemy = Random.Range(SmallSpawn_Low, SmallSpawn_High);
							spawnTokens -= TokenCost_SmallSpawn;
						}
						break;

					case 2:
						if (spawnTokens > TokenCost_MediumSpawn) {
							amountEnemy = Random.Range(MediumSpawn_Low, MediumSpawn_High);
							spawnTokens -= TokenCost_MediumSpawn;
						}
						break;

					case 3:
						if (spawnTokens > TokenCost_BigSpawn) {
							amountEnemy = Random.Range(BigSpawn_Low, BigSpawn_High);
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
					int amountNecro = 0;
					for (int count = 0; count < amountEnemy; count += 1) {
						int choiceEnemyKind = Util.RollWeightedChoice(new float[] {BasicWeight, ExplodingWeight, NecroWeight}); //9f, 1f, 0.5f
						switch (choiceEnemyKind) {
							case 0:
								amountBasic += 1;
								break;

							case 1:
								amountExploding += 1;
								// @TODO(Roskuski): We should probably cap Exploding spawn count so you can't roll a lot of them.
								break;
								
							case 2:
								amountNecro += 1;
								// @TODO(Roskuski): We should probably cap Exploding spawn count so you can't roll a lot of them.
								break;

							default:
								Debug.Assert(false);
								break;
						}
					}

					OrbSpawnPrefab.GetComponent<OrbSpawn>().basicAmount = amountBasic;
					OrbSpawnPrefab.GetComponent<OrbSpawn>().explodingAmount = amountExploding;
					OrbSpawnPrefab.GetComponent<OrbSpawn>().necroAmount = amountNecro;

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
		}

		if (debugTools) {
			if (Time.timeScale > 0.9) { // if game is not paused
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
				ResetSpawnerValues();
				SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
			}
			if (playerController.pActions.Player.MeterModifier.phase == InputActionPhase.Performed && playerController.pActions.Player.DEBUGHeal.WasPerformedThisFrame()) {
				playerController.health = playerController.healthMax;
				playerController.meter = playerController.meterMax;
			}
			if (playerController.pActions.Player.MeterModifier.phase == InputActionPhase.Performed && playerController.pActions.Player.DEBUGLevelSkip.WasPerformedThisFrame()) {
				float[] sceneChances = new float[] { 0, 1f, 1f, 1f, 1f, 1f, 1f };
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
			if (playerController.pActions.Player.MeterModifier.phase == InputActionPhase.Performed && playerController.pActions.Player.DEBUGSlowTime.WasPerformedThisFrame()) {
				if (Time.timeScale > 0) {
					Time.timeScale -= 0.1f;
                }
			}
		}

		if (playerController.pActions.Player.Pause.WasPerformedThisFrame() && playerController.animr.GetBool("isDead") == false && pauseBG.enabled == false) {
			if (SceneManager.GetActiveScene().buildIndex == (int)Scenes.Tutorial && Initializer.save.versionLatest.tutorialComplete == false) {
				if (tutorialSkipUI.enabled) {
					eSystem.SetSelectedGameObject(null);
					updateTimeScale = true;
					Time.timeScale = 1;
					pauseBG.enabled = false;
					tutorialSkipUI.enabled = false;
					tutorialSkipGroup.interactable = false;
				}
				else {
					tutorialSkipButton.Select();
					updateTimeScale = false;
					Time.timeScale = 0;
					pauseBG.enabled = true;
					tutorialSkipUI.enabled = true;
					tutorialSkipGroup.interactable = true;
				}
			}
			else {
				if (optionsUI.enabled == true) {
					pauseGroup.interactable = true;
					pauseUI.enabled = true;
					optionsUI.enabled = false;
					optionsGroup.interactable = false;
					resumeButton.Select();
				}
				else if (pauseUI.enabled == false) {
					updateTimeScale = false;
					Time.timeScale = 0;
					pauseBG.enabled = true;
					pauseUI.enabled = true;
					if (SceneManager.GetActiveScene().buildIndex == (int)Scenes.Tutorial) {
						TMP_Text replayTutorialText = transform.Find("PauseUI/QuitButton/QuitText").GetComponent<TMP_Text>();
						replayTutorialText.text = "Replay Tutorial";
						replayTutorialText.fontSize = 55;
					}
					pauseGroup.interactable = true;
					resumeButton.Select();
				}
				else {
					eSystem.SetSelectedGameObject(null);
					updateTimeScale = true;
					Time.timeScale = 1;
					pauseUI.enabled = false;
					pauseGroup.interactable = false;
					pauseBG.enabled = false;
				}
			}
		}

		if (playerController.pActions.Player.Zoom.WasPerformedThisFrame()) {
			cameraShake._CameraZoom();
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
		float[] sceneChances = new float[] { 0, 1f, 1f, 1f, 1f, 1f, 1f };
		sceneChances[SceneManager.GetActiveScene().buildIndex] = 0;
		KillAll();
		yield return new WaitForSeconds(5);
		if (playerController.currentState == PlayerController.States.Death) {
			SceneManager.LoadScene((int)Scenes.Tutorial);
		} else {
			storedPlayerHealth = playerController.health;
			storedPlayerMeter = playerController.meter;
			levelCount++;
			UpdateSpawnerValues();
			switch (currentObjective) {
				case Objectives.KillTheEnemies:
					enemyKillingGoal += 15;
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
					if (crystalHarvestingGoal < 8) { crystalHarvestingGoal += 1; }
					break;

				default:
					Debug.Assert(false, "Won with an invalid Objective " + currentObjective);
					break;
			}
			SceneManager.LoadScene(Util.RollWeightedChoice(sceneChances));
		}
	}

	// NOTE(Ryan): Causes enemy spawns to scale with levelCount. Currently there is no cap...
	public void UpdateSpawnerValues() {
		TokensPerSecond = 3 + (levelCount * 0.5f);
		TokenCost_SmallSpawn = levelCount * 2 + 20f;
		TokenCost_MediumSpawn = levelCount * 4 + 40f;
		TokenCost_BigSpawn = levelCount * 6 + 70f;

		LowEnemies = Mathf.RoundToInt(levelCount * 0.5f) + 1;
		if (LowEnemies > 10) LowEnemies = 10;
		TargetEnemies = levelCount + 3;
		if (TargetEnemies > 21) TargetEnemies = 21;
		HighEnemies = (levelCount * 2) + 4;
		if (HighEnemies > 40) HighEnemies = 40;

		SmallSpawn_Low = Mathf.RoundToInt(levelCount * 0.25f) + 4;
		if (SmallSpawn_Low > 9) SmallSpawn_Low = 9; //cap
		SmallSpawn_High = SmallSpawn_Low + 2;

		MediumSpawn_Low = Mathf.RoundToInt(levelCount * 0.5f) + 6;
		if (MediumSpawn_Low > 15) MediumSpawn_Low = 15; //cap
		MediumSpawn_High = MediumSpawn_Low + 2;

		BigSpawn_Low = Mathf.RoundToInt(levelCount * 0.75f) + 7;
		if (BigSpawn_Low > 21) BigSpawn_Low = 21; //cap
		BigSpawn_High = BigSpawn_Low + 2;

		//Enemy Weights & Armor Chance
		float randomWeight;
		float armorRandomChance;

		if (levelCount >= 2) {
			randomWeight = Random.Range(0.5f, 1f);

			ExplodingWeight = randomWeight;
		}

		if (levelCount >= 3) {
			randomWeight = 0.5f;

			NecroWeight = randomWeight;
		}

		if (levelCount >= 4) {
			randomWeight = Random.Range(0.5f, 2f);
			ExplodingWeight = randomWeight;

			randomWeight = 0.5f;
			NecroWeight = randomWeight;

			armorChanceLow = (levelCount - 3) * 3f;
			armorChanceHigh = (levelCount - 3) * 5f;
			if (armorChanceLow > 45) armorChanceLow = 45f;
			if (armorChanceHigh > 75) armorChanceHigh = 75f;
			armorRandomChance = Random.Range(armorChanceLow, armorChanceHigh);
			armoredEnemyChance = armorRandomChance;
		}

		if (levelCount >= 8) {
			randomWeight = Random.Range(0.5f, 3f);
			ExplodingWeight = randomWeight;

			randomWeight = Random.Range(0.5f, 1f);
			NecroWeight = randomWeight;
		}

		if (levelCount >= 12) {
			randomWeight = Random.Range(0.5f, 4f);
			ExplodingWeight = randomWeight;

			randomWeight = Random.Range(0.5f, 1.5f);
			NecroWeight = randomWeight;
		}
	}

	public static void ResetSpawnerValues() {
		TokensPerSecond = 3.5f;
		TokenCost_SmallSpawn = 20f;
		TokenCost_MediumSpawn = 40f;
		TokenCost_BigSpawn = 80f;
		LowEnemies = 2;
		TargetEnemies = 4;
		HighEnemies = 6;
		SmallSpawn_Low = 3;
		SmallSpawn_High = 4;
		MediumSpawn_Low = 4;
		MediumSpawn_High = 5;
		BigSpawn_Low = 5;
		BigSpawn_High = 7;

		//Enemy Weights & ArmorChance
		ExplodingWeight = 0f;
		NecroWeight = 0f;
		armoredEnemyChance = 0f;
		armorChanceLow = 0f;
		armorChanceHigh = 0f;
	}

	// NOTE(Ryan): Can be called to freeze the game for the time specified.
	// Frames60 is the amount of time, based on a 16.66ms long frame
	public void FreezeFrames(int Frames60) {
		frozenTime += (float)(Frames60) / 60.0f;
	}

	public void ShakeCamera(float intensity, float duration) {
		cameraShake._ShakeCamera(intensity * (Initializer.save.versionLatest.screenshakePercentage / 100), duration);
	}

	public void HurtVignette(float intensity, float duration) {
		vignette.TriggerVignette(intensity, duration);
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

	public void SpawnCorpse(int corpseID, Vector3 position, Quaternion rotation, float forceMultiplier, bool hasHead) {
		var corpse = Instantiate(corpses[corpseID], position, rotation);
		var forceScript = corpse.GetComponent<RandomForce>();
		if (forceMultiplier < 1f) forceMultiplier = 5f;
		forceScript.force *= forceMultiplier;
		forceScript.hasHead = hasHead;
    }

	// NOTE(@Jaden): Pickup determining function that's called on enemy death
	public void DeterminePickups(Vector3 position, bool isCrystallized) {
		float meterBeforeUse = playerController.meter + PlayerController.AttackMeterCost[(int)playerController.currentAttack];
		if (playerController.currentAttack == PlayerController.Attacks.None) {
			meterBeforeUse = playerController.meter;
		}

		float skullChance = (60 / playerController.meterMax) * (playerController.meterMax - meterBeforeUse);
		int healthChance = (35 / playerController.healthMax) * (playerController.healthMax - playerController.health);

		if (isCrystallized == true) {
			skullChance = 0;
			healthChance = 0;
			SpawnPickup((int)Pickup.Type.Crystal, position);
		}

		//Skull Pickup
		float pickupDecider = Random.Range(1, 100);
		if ((pickupDecider <= skullChance) && pickupsInAir.Count < maxPickupsInAir) { //check for skulldrop
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
		if ((pickupDecider <= healthChance) && pickupsInAir.Count < maxPickupsInAir) SpawnPickup((int)Pickup.Type.Health, position); //check for healthdrop
	}

	public void SpawnPickup(int pickupID, Vector3 position) {
		var pickup = Instantiate(Pickups[pickupID], position, Quaternion.identity);
		pickupsInAir.Add(pickup.gameObject);
		pickupTime = 1f;
	}

	public void UpdateHealthBar() {
		float healthMax = playerController.healthMax;
		float health = playerController.health;
		float segmentValue = (health / healthMax) * 10;
		float removedSegments = (healthBar.GetFloat("_TotalSegments") - segmentValue);
		healthBar.SetFloat("_RemovedSegments", removedSegments);

		float previousSegmentValue = 20f - ((previousHealth / healthMax) * 10);
		
		if (barDrainTime > 0) {
			barDrainTime -= Time.deltaTime;
			float drainValue = Mathf.Lerp(removedSegments, previousSegmentValue, barDrainTime / 0.5f);
			healthBarDrain.SetFloat("_RemovedSegments", drainValue);
		}

		/*float healthMax = playerController.healthMax;
		float health = playerController.health;
		healthBar.localScale = new Vector3((health / healthMax), 1f, 1f);*/ //Old Healthbar Logic
	}

	public void UpdateMeter() {
		float meterMax = playerController.meterMax;
		float meter = playerController.meter;
		float segmentValue = (meter / meterMax) * 8;
		float removedSegments = (meterBar.GetFloat("_TotalSegments") - segmentValue);
		meterBar.SetFloat("_RemovedSegments", removedSegments);
		if (playerController.frenzyTimer > 0) { meterBar.SetColor("_BarColor", Color.yellow); }
		else { meterBar.SetColor("_BarColor", Color.white); }

		/*meterBar.localScale = new Vector3((playerController.meter / playerController.meterMax), 1f, 1f);
		if (playerController.frenzyTimer > 0) { meterImage.color = Color.yellow; }
		else { meterImage.color = Color.white; }*/ //Old MeterBar Logic

		//Tick down flashing elements
		if (meterCostFlashTime > 0) {
			meterCostFlashTime -= Time.deltaTime;
			for (int i = 0; i < meterCostSegments.Length; i++) {
				if (meterCostSegments[i].color.a > 0f) {
					Color temp = meterCostSegments[i].color;
					temp.a = Mathf.Lerp(0f, 1f, meterCostFlashTime / 0.5f);
					meterCostSegments[i].color = temp;
				}
            }

		}
	}

	public void MeterSpendFail(int index) {
		for (int i = 0; i < meterCostSegments.Length; i++) {
			if (i == index) {
				Color temp = meterCostSegments[i].color;
				temp.a = 1f;
				meterCostSegments[i].color = temp;
			}
			else {
				Color temp = meterCostSegments[i].color;
				temp.a = 0f;
				meterCostSegments[i].color = temp;
			}
        }
		meterCostFlashTime = 0.5f;
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

	#region Menu Navigation
	public void OnResume() {
		eSystem.SetSelectedGameObject(null);
		updateTimeScale = true;
		Time.timeScale = 1;
		pauseUI.enabled = false;
		pauseGroup.interactable = false;
		if (SceneManager.GetActiveScene().buildIndex == (int)Scenes.Tutorial) {
			tutorialSkipUI.enabled = false;
			tutorialSkipGroup.interactable = false;
		}
		pauseBG.enabled = false;
	}

	public static void OnTutorialSkip() {
		Initializer.save.versionLatest.tutorialComplete = true;
		Initializer.Save();
		SceneManager.LoadScene((int)Scenes.Tutorial);
	}

	public void OnRestart() {
        pauseUI.enabled = false;
        pauseGroup.interactable = false;
        cancelConfirmUI.enabled = true;
        cancelConfirmGroup.interactable = true;
        TMP_Text cancelConfirmText = cancelConfirmUI.transform.Find("CancelText").GetComponent<TMP_Text>();
        Button restartConfirmButton = cancelConfirmUI.transform.Find("RestartYesButton").GetComponent<Button>();
        Button quitConfirmButton = cancelConfirmUI.transform.Find("QuitYesButton").GetComponent<Button>();
        Button deleteSaveConfirmButton = cancelConfirmUI.transform.Find("DeleteSaveYesButton").GetComponent<Button>();
        Button quitToDesktopButton = cancelConfirmUI.transform.Find("QuitToDesktopButton").GetComponent<Button>();
        cancelConfirmText.text = "Restart?"; 
		quitConfirmButton.gameObject.SetActive(false);
		deleteSaveConfirmButton.gameObject.SetActive(false); ;
		quitToDesktopButton.gameObject.SetActive(false);
        restartConfirmButton.Select();
    }

	public static void OnRestartConfirm() {
		storedPlayerHealth = 10;
		storedPlayerMeter = 3;
		levelCount = 1;
		enemyKillingGoal = 15;
		crystalHarvestingGoal = 2;
		enemiesKilledInRun = 0;
		ResetSpawnerValues();
		Initializer.save.versionLatest.runsStarted++;
		SceneManager.LoadScene((int)Scenes.GrassBridge);
	}

	public void OnOptions() {
		pauseUI.enabled = false;
		pauseGroup.interactable = false;
		optionsUI.enabled = true;
		optionsGroup.interactable = true;
        Toggle fsToggle = transform.Find("OptionsUI/VisualSettings/Fullscreen/FullscreenToggle").GetComponent<Toggle>();
        fsToggle.isOn = Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen;
        Toggle rumbleToggle = transform.Find("OptionsUI/VisualSettings/Rumble/RumbleToggle").GetComponent<Toggle>();
        rumbleToggle.isOn = Initializer.save.versionLatest.rumble;
        optionsBackButton = transform.Find("OptionsUI/OptionsBackButton").GetComponent<Button>();
		optionsBackButton.Select();
	}

	public void OnAudio() {
		optionsUI.enabled = false;
		optionsGroup.interactable = false;
		audioUI.enabled = true;
		audioGroup.interactable = true;
		optionsBackButton = transform.Find("AudioUI/OptionsBackButton").GetComponent<Button>();
		optionsBackButton.Select();
		Slider masterSlider = transform.Find("AudioUI/Master/MasterSlider").GetComponent<Slider>();
		Slider musicSlider = transform.Find("AudioUI/Music/MusicSlider").GetComponent<Slider>();
		Slider sfxSlider = transform.Find("AudioUI/SFX/SFXSlider").GetComponent<Slider>();
		masterSlider.value = Initializer.save.versionLatest.masterVolume;
		musicSlider.value = Initializer.save.versionLatest.musicVolume;
		sfxSlider.value = Initializer.save.versionLatest.sfxVolume;
	}

	public void OnCredits() {
        optionsUI.enabled = false;
        optionsGroup.interactable = false;
		creditsUI.enabled = true;
		creditsGroup.interactable = true;
		Animator creditsAnimator = transform.Find("CreditsUI/CreditsContainer").GetComponent<Animator>();
		creditsAnimator.Play("CreditsMovement");
        optionsBackButton = transform.Find("CreditsUI/OptionsBackButton").GetComponent<Button>();
        optionsBackButton.Select();
    }

    public void OnGraphics() {
		audioUI.enabled = false;
		audioGroup.interactable = false;
		optionsUI.enabled = true;
		optionsGroup.interactable = true;
		Toggle fsToggle = transform.Find("OptionsUI/VisualSettings/Fullscreen/FullscreenToggle").GetComponent<Toggle>();
		fsToggle.isOn = Screen.fullScreenMode == FullScreenMode.ExclusiveFullScreen;
        Slider screenshakeSlider = transform.Find("OptionsUI/Screenshake/ScreenshakeSlider").GetComponent<Slider>();
		screenshakeSlider.value = Initializer.save.versionLatest.screenshakePercentage;
        optionsBackButton = transform.Find("OptionsUI/OptionsBackButton").GetComponent<Button>();
		optionsBackButton.Select();
	}

	public void SetQuality(int qualityIndex) {
		QualitySettings.SetQualityLevel(qualityIndex);
		QualitySettings.renderPipeline = qualityLevels[qualityIndex];
	}

	public void SetResolution(int resolutionIndex) {
		Resolution resolution = resolutions[resolutionIndex];
		Initializer.save.versionLatest.resolutionOption = resolutionIndex;
		Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
	}

	public void SetFullScreen(bool isFullscreen) {
		Screen.fullScreenMode = isFullscreen ? FullScreenMode.FullScreenWindow : FullScreenMode.Windowed;
	}

	public void SetRumble(bool rumble) {
		Initializer.save.versionLatest.rumble = rumble;
	}

	//(TODO(@Jaden)): Make screenshake, rumble, corpse limit functions

	public void OnStats() {
		optionsUI.enabled = false;
		optionsGroup.interactable = false;
		audioUI.enabled = false;
		audioGroup.interactable = false;
		statsUI.enabled = true;
		statsGroup.interactable = true;
		statsText = transform.Find("StatsUI/StatsText").GetComponent<TMP_Text>();
		statsText2 = transform.Find("StatsUI/StatsText2").GetComponent<TMP_Text>();
		statsBackButton = transform.Find("StatsUI/StatsBackButton").GetComponent<Button>();
		statsText.text =
			"<b>ENEMIES:</b>" +
			"\nTotal Kills: " + (Initializer.save.versionLatest.basicEnemyKills + Initializer.save.versionLatest.explosiveEnemyKills + Initializer.save.versionLatest.necroEnemyKills + Initializer.save.versionLatest.bruteEnemyKills)
			+ (Initializer.save.versionLatest.basicEnemyKills > 0 ? "\nBasic: " + Initializer.save.versionLatest.basicEnemyKills : "\n??? : ???")
			+ (Initializer.save.versionLatest.explosiveEnemyKills > 0 ? "\nBomb Spiders: " + Initializer.save.versionLatest.explosiveEnemyKills : "\n??? : ???")
			+ (Initializer.save.versionLatest.necroEnemyKills > 0 ? "\nNecromancers: " + Initializer.save.versionLatest.necroEnemyKills : "\n??? : ???");
			//+ (Initializer.save.versionLatest.bruteEnemyKills > 0 ? "\nBrutes: " + Initializer.save.versionLatest.bruteEnemyKills : "\n??? : ???");
		statsText2.text =
			"<b>RUNS:</b>"
			+ "\nRuns started: " + Initializer.save.versionLatest.runsStarted
			+ (Initializer.save.versionLatest.longestRun > 0 ? "\nLongest run: " + Initializer.save.versionLatest.longestRun + " Levels" : "\n??? : ???");
		//+ (Initializer.save.versionLatest.timesWon > 0 ? "\nWins: " + Initializer.save.versionLatest.timesWon : "\n??? : ???");
		Button deleteSaveButton = statsUI.transform.Find("DeleteSaveButton").GetComponent<Button>();
		if (SceneManager.GetActiveScene().buildIndex != (int)Scenes.Tutorial) deleteSaveButton.gameObject.SetActive(false);
        statsBackButton.Select();
	}

	public void OnStatsBack() {
		statsText.text = "";
		statsText2.text = "";
		statsUI.enabled = false;
		statsGroup.interactable = false;
		optionsUI.enabled = true;
		optionsGroup.interactable = true;
        optionsBackButton = transform.Find("OptionsUI/OptionsBackButton").GetComponent<Button>();
        optionsBackButton.Select();
	}

	public void OnOptionsBack() {
		pauseUI.enabled = true;
		pauseGroup.interactable = true;
		optionsUI.enabled = false;
		optionsGroup.interactable = false;
		audioUI.enabled = false;
		audioGroup.interactable = false;
		creditsUI.enabled = false;
		creditsGroup.interactable = false;
       /* Animator creditsAnimator = transform.Find("CreditsUI/CreditsContainer").GetComponent<Animator>();
        creditsAnimator.enabled = false;*/
        resumeButton.Select();
		Initializer.Save();
	}

	public void OnQuit() {
		pauseUI.enabled = false;
		pauseGroup.interactable = false;
		cancelConfirmUI.enabled = true;
		cancelConfirmGroup.interactable = true;
		TMP_Text cancelConfirmText = cancelConfirmUI.transform.Find("CancelText").GetComponent<TMP_Text>();
		Button quitConfirmButton = cancelConfirmUI.transform.Find("QuitYesButton").GetComponent<Button>();
        Button restartConfirmButton = cancelConfirmUI.transform.Find("RestartYesButton").GetComponent<Button>();
        Button deleteSaveConfirmButton = cancelConfirmUI.transform.Find("DeleteSaveYesButton").GetComponent<Button>();
        if (SceneManager.GetActiveScene().buildIndex == (int)Scenes.Tutorial) {
            cancelConfirmText.text = "Replay Tutorial?";
        } else { cancelConfirmText.text = "Quit?"; }
		restartConfirmButton.gameObject.SetActive(false);
        deleteSaveConfirmButton.gameObject.SetActive(false);
        quitConfirmButton.Select();
    }

    public void OnCancelConfirmBack() {
        Button restartConfirmButton = cancelConfirmUI.transform.Find("RestartYesButton").GetComponent<Button>();
        Button quitConfirmButton = cancelConfirmUI.transform.Find("QuitYesButton").GetComponent<Button>();
        Button deleteSaveConfirmButton = cancelConfirmUI.transform.Find("DeleteSaveYesButton").GetComponent<Button>();
        Button quitToDesktopButton = cancelConfirmUI.transform.Find("QuitToDesktopButton").GetComponent<Button>();
        restartConfirmButton.gameObject.SetActive(true);
        deleteSaveConfirmButton.gameObject.SetActive(true);
        quitToDesktopButton.gameObject.SetActive(true);
        quitConfirmButton.gameObject.SetActive(true);
        cancelConfirmUI.enabled = false;
		cancelConfirmGroup.interactable = false;
        pauseUI.enabled = true;
        pauseGroup.interactable = true;
        resumeButton.Select();
	}

	public void OnQuitConfirm() {
		if (SceneManager.GetActiveScene().buildIndex == (int)Scenes.Tutorial) {
			Initializer.save.versionLatest.tutorialComplete = false;
		}
		enemiesKilledInRun = 0;
		Time.timeScale = 1;
		Initializer.Save();
		SceneManager.LoadScene((int)Scenes.Tutorial);
	}

	public void OnQuitToDesktop() {
		enemiesKilledInRun = 0;
		Initializer.Save();
		Application.Quit();
	}

    public void OnDeleteSave() {
        statsUI.enabled = false;
        statsGroup.interactable = false;
        cancelConfirmUI.enabled = true;
        cancelConfirmGroup.interactable = true;
        TMP_Text cancelConfirmText = cancelConfirmUI.transform.Find("CancelText").GetComponent<TMP_Text>();
        Button restartConfirmButton = cancelConfirmUI.transform.Find("RestartYesButton").GetComponent<Button>();
        Button quitConfirmButton = cancelConfirmUI.transform.Find("QuitYesButton").GetComponent<Button>();
        Button deleteSaveConfirmButton = cancelConfirmUI.transform.Find("DeleteSaveYesButton").GetComponent<Button>();
        Button quitToDesktopButton = cancelConfirmUI.transform.Find("QuitToDesktopButton").GetComponent<Button>();
        cancelConfirmText.text = "Really Delete Save?";
        restartConfirmButton.gameObject.SetActive(false); ;
        quitConfirmButton.gameObject.SetActive(false);
        quitToDesktopButton.gameObject.SetActive(false);
        deleteSaveConfirmButton.Select();
    }

	public void OnDeleteSaveConfirm() {
		Initializer.AssignDefaultValues();
		Initializer.Save();
        Time.timeScale = 1;
        SceneManager.LoadScene((int)Scenes.Tutorial);
    }
    #endregion

    void OnEnable() { dActions.Enable(); }
	void OnDisable() { dActions.Disable(); }

	private void OnApplicationQuit() { Initializer.Save(); }
}
