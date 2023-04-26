using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleProp : MonoBehaviour
{
	[Header("References:")]
	public GameObject headPop;
	PlayerController player;
	GameManager gameMan;

	[Header("Integrity:")]
	public bool canDropHeads = false;
	float hitflashTimer = 0f;
	MeshRenderer model;
	Material[] materials;
	public Material hitflashMat;
	public AK.Wwise.Event ImpactSound;

	[Header("Head pop:")]
	public float randomForce;
	public int headTotal; //about how many heads the pile can contain
	public int headOffset;
	public bool isUnlimited;
	int heads = 0;

	private void Start() {
		player = transform.Find("/Player").GetComponent<PlayerController>();
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		model = GetComponent<MeshRenderer>();
		materials = model.materials;
		heads = Random.Range(headTotal - headOffset, headTotal + headOffset);
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
		if (other.gameObject.layer == (int)Layers.PlayerHitbox || other.gameObject.layer == (int)Layers.EnemyHitbox || other.gameObject.layer == (int)Layers.AgnosticHitbox) {
			Impact_Sound();
			if (!canDropHeads) { Destroy(gameObject); }
			else {
				Vector3 spawnPoint = new Vector3(transform.position.x, transform.position.y + 1f, transform.position.z);
				gameMan.SpawnParticle(0, spawnPoint, 2f);
				hitflashTimer = 0.15f;
				int random = Random.Range(1, headOffset);
				SpawnHeads(random);
				if (heads <= 0) Destroy(gameObject);
			}
		}
	}

	public void SpawnHeads(int number) {
		if (!isUnlimited) heads -= number;
		for (int i = 0; i < number; i++) {
			GameObject headInstance = Instantiate(gameMan.Pickups[0], transform.position, transform.rotation);
			Pickup hpop = headInstance.transform.Find("Head").GetComponent<Pickup>();
			hpop.randomForce = randomForce;
			hpop.flightAngle = 70f;
		}
    }

	void Impact_Sound() {
		ImpactSound.Post(gameObject);
	}
}
