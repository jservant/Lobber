using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
	Canvas mainUI;
	Canvas optionsUI;

	private void Start() {

		mainUI = transform.Find("MainUI").GetComponent<Canvas>();
		optionsUI = transform.Find("OptionsUI").GetComponent<Canvas>();
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
