using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DestructibleProp : MonoBehaviour {
	PlayerController player;
	GameManager gameMan;

	[Header("Integrity:")]
	public bool canDropHeads = false;
	public bool isHealthMachine;
	float hitflashTimer = 0f;
	MeshRenderer model;
	Material[] materials;
	public Material hitflashMat;
	public Transform particleSpawnPoint;
	public int particleID; //which particle from gameMan to spawn
	public float particleScale; //how big to scale the associated particles
	public AK.Wwise.Event ImpactSound;

	[Header("Head pop:")]
	public float randomForce;
	public float forceOffset;
	public int headTotal; //about how many heads the pile can contain
	public int headOffset;
	public bool isUnlimited;
	int heads = 0;

	private void Start() {
		player = transform.Find("/Player").GetComponent<PlayerController>();
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		model = this.GetComponent<MeshRenderer>();
		if (model == null) {
			model = transform.Find("Visual").GetComponent<MeshRenderer>(); // Special case for Bonepile gameObject
		}
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
			gameMan.SpawnParticle(particleID, particleSpawnPoint.position, particleScale);
			if (!canDropHeads) {
				Destroy(gameObject); 
			}
			else {
				if (other.gameObject.layer == (int)Layers.PlayerHitbox) {
					HeadProjectile head = other.GetComponentInParent<HeadProjectile>();
					PlayerController player = other.GetComponentInParent<PlayerController>();
					ExplosiveTrap explosiveTrap = other.GetComponentInParent<ExplosiveTrap>();

					if (player != null) {
						if (player.currentAttack == PlayerController.Attacks.Chop) {
							hitflashTimer = 0.25f;
							player.ChangeMeter(1);
							gameMan.SpawnParticle(12, other.transform.position, 1.6f);
							SpawnHeads(2);
						}
						else if (player.currentAttack == PlayerController.Attacks.Slam) {
							hitflashTimer = 0.25f;
							SpawnHeads(3);
						}
						else {
							hitflashTimer = 0.15f;
							gameMan.SpawnParticle(12, other.transform.position, 1.2f);
							SpawnHeads(1);
						}
					}
					else if (head != null) {
						hitflashTimer = 0.15f;
						SpawnHeads(1);
					}
					else if (explosiveTrap != null) {
						hitflashTimer = 0.25f;
						SpawnHeads(3);
					}

				}
				else {
					hitflashTimer = 0.15f;
					SpawnHeads(1);
				}

				if (heads <= 0) Destroy(gameObject);
			}
		}
	}

	public void SpawnHeads(int number) {
		if (!isUnlimited) heads -= number;
		for (int i = 0; i < number; i++) {

			int pickupID;
			if (isHealthMachine) pickupID = 2;
			else pickupID = 0;
			GameObject headInstance = Instantiate(gameMan.Pickups[pickupID], transform.position, transform.rotation);
			Pickup hpop = headInstance.transform.Find("Head").GetComponent<Pickup>();
			hpop.randomForce = randomForce;
			hpop.flightAngle = 70f;
			hpop.forceOffset = forceOffset;
		}
    }

	void Impact_Sound() {
		ImpactSound.Post(gameObject);
	}

    private void OnDrawGizmos() {
		if (canDropHeads) {
			Gizmos.color = Color.cyan;
			if (isHealthMachine) Gizmos.color = Color.red;
			Vector3 cubeSize = new Vector3(forceOffset * 2f, 7f, forceOffset * 2f);
			Gizmos.DrawWireCube(transform.position, cubeSize);
			Vector3 _cubeSize = new Vector3(randomForce * 2f, 7f, randomForce * 2f);
			Gizmos.DrawWireCube(transform.position, _cubeSize);
		}
	}
}
