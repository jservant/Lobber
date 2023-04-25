using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shrine : MonoBehaviour
{
	public float health;

	GameManager gameManager;

	MeshRenderer model;
	Material[] materials;
	public Material hitflashMat;
	float hitflashTimer = 0;

	void Start()
    {
		gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
		model = GetComponent<MeshRenderer>();
		materials = model.materials;
	}

	private void Update() {
		hitflashTimer -= Time.deltaTime;
		Material[] materialList = model.materials;
		for (int i = 0; i < materialList.Length; i++) {
			if (hitflashTimer > 0) {
				materialList[i] = hitflashMat;
			}
			else {
				materialList[i] = materials[i];
			}
		}
		model.materials = materialList;
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHitbox) {
			health--;
			hitflashTimer = 0.1f;
			if (health <= 0) {
				gameManager.shrinesDestroyed++;
				Destroy(gameObject);
			}
		}
	}
}
