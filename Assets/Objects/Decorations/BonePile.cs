using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BonePile : MonoBehaviour
{
	PlayerController player;
	public GameObject headPop;
	public float randomForce;
	GameManager gameMan;

	public int headTotal; //about how many heads the pile can contain
	int heads = 0;

	float hitflashTimer = 0f;
	MeshRenderer model;
	Material[] materials;
	public Material hitflashMat;

	private void Start() {
		player = transform.Find("/Player").GetComponent<PlayerController>();
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		model = GetComponent<MeshRenderer>();
		materials = model.materials;
		heads = Random.Range(headTotal - 3, headTotal + 3);
	}

    void Update() {
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
		if (other.gameObject.layer == (int)Layers.PlayerHitbox) { // player is hitting enemy
			Vector3 spawnPoint = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
			gameMan.SpawnParticle(0, spawnPoint, 2f);
			hitflashTimer = 0.25f;
			int random = Random.Range(2, 3);
			SpawnHeads(random);
			if (heads <= 0) Destroy(gameObject);
		}
	}

	public void SpawnHeads(int number) {
		heads -= number;
		for (int i = 0; i < number; i++) {
			GameObject headInstance = Instantiate(headPop, transform.position, transform.rotation);
			HeadPickup hpop = headInstance.transform.Find("Head").GetComponent<HeadPickup>();
			hpop.randomForce = randomForce;
			hpop.flightAngle = 70f;
		}
    }
}
