using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Shrine : MonoBehaviour
{
	public float health = 10;

	Canvas healthUI;
	GameManager gameManager;

	MeshRenderer model;
	Material[] materials;
	public Material hitflashMat;
	float hitflashTimer = 0;

	void Start()
    {
		gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
		healthUI = transform.Find("Canvas").GetComponent<Canvas>();
		//healthUI.worldCamera = Camera.main.transform.Find("UI Camera").GetComponent<Camera>();
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
			switch (gameManager.playerController.currentAttack) {
				case PlayerController.Attacks.LAttack:
					health -= 1;
					break;
				case PlayerController.Attacks.LAttack2:
					health -= 1;
					break;
				case PlayerController.Attacks.LAttack3:
					health -= 2;
					break;
				case PlayerController.Attacks.Spin:
					health -= 2;
					break;
				case PlayerController.Attacks.LethalDash:
					health -= 2;
					break;
				case PlayerController.Attacks.Slam:
					float posDifference = Mathf.Abs((gameManager.player.transform.position - transform.position).sqrMagnitude);
					Debug.Log(gameObject.name + "'s posDifference after slam: " + posDifference);
					if (posDifference < 40f) {
						health -= 3;
					}
					else if (posDifference < 80f) {
						health -= 2;
					}
					break;
				case PlayerController.Attacks.Chop:
					health -= 2f;
					break;
				default:
					Debug.Log("I, " + this.name + " was hit by an unhandled attack (" + gameManager.playerController.currentAttack + ")");
					break;
			}
			if (health <= 0) {
				gameManager.shrinesDestroyed++;
				Destroy(gameObject);
			}
		}
	}
}
