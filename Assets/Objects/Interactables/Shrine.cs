using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Shrine : MonoBehaviour {
	public float health = 15;

	TMP_Text healthText;
	GameManager gameManager;

	MeshRenderer model;
	Material[] materials;
	public Material hitflashMat;
	float hitflashTimer = 0;

	void Start() {
		gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
		health = GameManager.shrineMaxHealth;
		healthText = transform.Find("Visual/Canvas/HealthText").GetComponent<TMP_Text>();
		//healthUI.worldCamera = Camera.main.transform.Find("UI Camera").GetComponent<Camera>();
		model = transform.Find("Visual").GetComponent<MeshRenderer>();
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

		healthText.text = health.ToString();
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHitbox && hitflashTimer <= 0) {
			if (other.GetComponentInParent<PlayerController>() != null) {
				switch (gameManager.playerController.currentAttack) {
					case PlayerController.Attacks.LAttack:
					case PlayerController.Attacks.LAttack2:
						health -= 1;
						break;
					case PlayerController.Attacks.LAttack3:
					case PlayerController.Attacks.Spin:
					case PlayerController.Attacks.LethalDash:
						health -= 2;
						break;
					case PlayerController.Attacks.Slam:
						float posDifference = Mathf.Abs((gameManager.player.transform.position - transform.position).sqrMagnitude);
						Debug.Log(gameObject.name + "'s posDifference after slam: " + posDifference);
						if (posDifference < 40f) {
							health -= 8;
						}
						else if (posDifference < 80f) {
							health -= 4;
						}
						break;
					case PlayerController.Attacks.Chop:
						health -= 2f;
						break;
					default:
						Debug.Log("I, " + this.name + " was hit by an unhandled attack (" + gameManager.playerController.currentAttack + ")");
						break;
				}
				hitflashTimer = 0.1f;
			}
			else if (other.GetComponentInParent<HeadProjectile>() != null) {
				health -= 2f;
				hitflashTimer = 0.1f;
			}
			if (health <= 0) {
				gameManager.shrinesDestroyed++;
				Destroy(gameObject);
			}
		}
	}
}
