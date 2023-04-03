using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotater : MonoBehaviour {

	public float cSpeed = 30f;
	float yConst;
	PlayerController player;
	Vector2 cInput;

	void Start() {
		player = transform.Find("/GameManager").GetComponent<GameManager>().playerController;
		transform.position = player.transform.position;
		yConst = transform.position.y;
	}

	// Update is called once per frame
	void FixedUpdate() {
		cInput = player.pActions.Player.Aim.ReadValue<Vector2>();
		transform.position = new Vector3(player.transform.position.x, yConst, player.transform.position.z);
		transform.Rotate(cInput.y * cSpeed * Time.fixedDeltaTime, cInput.x * cSpeed * Time.fixedDeltaTime, 0f, Space.World);
	}
}
