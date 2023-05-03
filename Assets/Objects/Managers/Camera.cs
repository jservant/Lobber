using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Camera : MonoBehaviour {

	public float cSpeed = 30f;
	float yConst;
	GameManager gameManager;
	PlayerController playerController;
	Vector2 cInput;
	public GameObject camera2;
	public bool canRotate;

	public Transform followTarget;

	void Start() {
		gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
		playerController = gameManager.playerController;
		transform.position = playerController.transform.position;
		yConst = transform.position.y;
	}

	// Update is called once per frame
	void FixedUpdate() {
		if (followTarget != null) {
			transform.position = followTarget.position;
		}
		else if (playerController.transform.position.y >= yConst) { transform.position = new Vector3(playerController.transform.position.x, playerController.transform.position.y, playerController.transform.position.z); }
		else { transform.position = new Vector3(playerController.transform.position.x, yConst, playerController.transform.position.z); }

		if (canRotate) {
			cInput = playerController.pActions.Player.Aim.ReadValue<Vector2>();
			transform.Rotate(0f, cInput.x * cSpeed * Time.fixedDeltaTime, 0f, Space.World);
			camera2.transform.Rotate(cInput.y * -cSpeed * Time.fixedDeltaTime, 0f, 0f, Space.Self);
		}
	}

}
