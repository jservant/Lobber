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

		Debug.Log("(MenuManager) Enemies killed: " + Initializer.allEnemiesKilled);
		if (Initializer.allEnemiesKilled > 0) { globalKillcount.text = "Total Kills:\n" + Initializer.allEnemiesKilled; }
		else { globalKillcount.text = ""; }
	}

	public void OnPlay() {
		GameManager.storedPlayerHealth = 10;
		GameManager.storedPlayerMeter = 3;
		SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
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
