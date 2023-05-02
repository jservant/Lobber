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

	bool waypointTracking = true;
	// Indicator icon
	Image waypointMarker;
	// UI Text to display the distance
	TMP_Text distanceText;
	// The target (location, enemy, etc..)
	Transform target;
	// To adjust the position of the icon
	public Vector3 waypointOffset;

	void Start() {
		gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
		playerController = gameManager.playerController;
		waypointMarker = transform.Find("/GameManager/MainUI/WaypointMarker").GetComponent<Image>();
		distanceText = transform.Find("/GameManager/MainUI/WaypointMarker/WaypointDistanceText").GetComponent<TMP_Text>();
		transform.position = playerController.transform.position;
		yConst = transform.position.y;

		switch (GameManager.currentObjective) {
			case GameManager.Objectives.HarvestTheCrystals:
				target = transform.Find("/CrystalDropoffSpawn");
				break;
			default:
				//target = playerController.transform;
				waypointTracking = false;
				break;
		}
	}

	// Update is called once per frame
	void FixedUpdate() {
		//cInput = player.pActions.Player.Aim.ReadValue<Vector2>();
		if (playerController.transform.position.y >= yConst) { transform.position = new Vector3(playerController.transform.position.x, playerController.transform.position.y, playerController.transform.position.z); }
		else { transform.position = new Vector3(playerController.transform.position.x, yConst, playerController.transform.position.z); }
		//transform.Rotate(0f, cInput.x * cSpeed * Time.fixedDeltaTime, 0f, Space.World);
		//camera2.transform.Rotate(cInput.y * -cSpeed * Time.fixedDeltaTime, 0f, 0f, Space.Self);
	}

	// Credit for waypoint code:
	// https://github.com/OBalfaqih/Unity-Tutorials/blob/master/Unity%20Tutorials/WaypointMarker/Scripts/MissionWaypoint.cs
	private void Update() {
		if (waypointTracking) {
			// Giving limits to the icon so it sticks on the screen
			// Below calculations with the assumption that the icon anchor point is in the middle
			// Minimum X position: half of the icon width
			float minX = waypointMarker.GetPixelAdjustedRect().width / 2;
			// Maximum X position: screen width - half of the icon width
			float maxX = Screen.width - minX;

			// Minimum Y position: half of the height
			float minY = waypointMarker.GetPixelAdjustedRect().height / 2;
			// Maximum Y position: screen height - half of the icon height
			float maxY = Screen.height - minY;

			// Temporary variable to store the converted position from 3D world point to 2D screen point
			Vector2 pos = UnityEngine.Camera.main.WorldToScreenPoint(target.position + waypointOffset);

			// Check if the target is behind us, to only show the icon once the target is in front
			if (Vector3.Dot((target.position - transform.position), transform.forward) < 0) {
				// Check if the target is on the left side of the screen
				if (pos.x < Screen.width / 2) {
					// Place it on the right (Since it's behind the player, it's the opposite)
					pos.x = maxX;
				}
				else {
					// Place it on the left side
					pos.x = minX;
				}
			}

			// Limit the X and Y positions
			pos.x = Mathf.Clamp(pos.x, minX, maxX);
			pos.y = Mathf.Clamp(pos.y, minY, maxY);

			// Update the marker's position
			waypointMarker.transform.position = pos;
			// Change the meter text to the distance with the meter unit 'm'
			distanceText.text = ((int)Vector3.Distance(target.position, transform.position)).ToString() + "m";
		}
	}
}
