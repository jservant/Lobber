using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SampleTrap : MonoBehaviour {
	public GameObject hitbox;
	private bool isArmed;
	public float armTime; //time it takes for the trap to rearm itself
	private float currentArmTime;
	public float triggerTime; //active frames of the trap hitbox
	public float currentTriggerTime;
	private Renderer render;
	public Material[] materials;

	void Start() {
		hitbox.SetActive(false);
		isArmed = true;
		currentArmTime = armTime;
		currentTriggerTime = 0f;
		render = GetComponent<Renderer>();
	}

	void Update() {
		if (isArmed == false) {
			render.material = materials[1];
			currentArmTime -= Time.deltaTime;
			if (currentArmTime <= 0) isArmed = true;
		}
		else render.material = materials[0];

		if (currentTriggerTime <= 0) {
			hitbox.SetActive(false);
		}
		else currentTriggerTime -= Time.deltaTime;

	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHitbox) {
			if (isArmed) SpringTrap();
		}
	}

	void SpringTrap() {
		isArmed = false;
		currentArmTime = armTime;
		currentTriggerTime = triggerTime;
		hitbox.SetActive(true);
	}
}
