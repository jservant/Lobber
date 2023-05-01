using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
	Canvas mainUI;
	Canvas optionsUI;

	TMP_Text globalKillcount;

	private void Start() {
		mainUI = transform.Find("MainUI").GetComponent<Canvas>();
		optionsUI = transform.Find("OptionsUI").GetComponent<Canvas>();
		globalKillcount = transform.Find("MainUI/GlobalKillcount").GetComponent<TMP_Text>();
	}

	public static void OnPlay() {
		GameManager.storedPlayerHealth = 10;
		GameManager.storedPlayerMeter = 3;
		GameManager.score = 0;
		GameManager.levelCount = 1;
		GameManager.enemyKillingGoal = 20;
		GameManager.crystalHarvestingGoal = 3;
		GameManager.enemiesKilledInRun = 0;
		Initializer.save.versionLatest.runsStarted++;
		SceneManager.LoadScene((int)Scenes.Level_A); // disabling B for now

	}

	public void OnOptions() {
		optionsUI.enabled = true;
		mainUI.enabled = false;
	}

	public void OnOptionsBack() {
		optionsUI.enabled = false;
		mainUI.enabled = true;
	}

	public void OnQuit() {
		Application.Quit();
	}
}
