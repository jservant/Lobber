using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraRotater : MonoBehaviour {

	public float cSpeed = 30f;
	PlayerController player;
	//Vector2 cInput;

	void Start() {
		player = transform.Find("/GameManager").GetComponent<GameManager>().playerController;
	}

	// Update is called once per frame
	void FixedUpdate() {
		//cInput = player.pActions.Player.CameraControl.ReadValue<Vector2>();
		transform.position = player.transform.position;
		//transform.RotateAround(player.transform.position, Vector3.up, 5 * Time.deltaTime);
		//transform.Rotate(0f, cInput.x * cSpeed * Time.fixedDeltaTime, 0f, Space.World);
	}
}
