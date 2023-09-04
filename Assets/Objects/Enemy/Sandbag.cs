using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandbag : MonoBehaviour {
	public bool hasHealth;
	public bool canBeKnockedBack;
	public bool canRespawn;
	private Vector3 respawnPoint;
	KnockbackInfo knockbackInfo;
	float remainingKnockbackTime;

	public float apperanceDelay;

	[SerializeField] [BitField] PlayerController.AttackBitMask VulnerabilityMask;
	[SerializeField] bool VurnerableToHeadProjectiles;

	Vector3 movementDelta;

	public float maxHealth;
	public float health;
	bool shouldDie = false;
	bool wasHitByChop = false;

	GameManager gameMan;

	public SkinnedMeshRenderer model;
	public CapsuleCollider collider;
	Material[] materials;
	public Material hitflashMat;
	float hitflashTimer = 0;
	public Animator animator;
	public GameObject killIndicator;
	public Transform corpsePos;

	public bool showIndicator;

	public AK.Wwise.Event Get_Hit_Sound;
	public AK.Wwise.Event Destroy_Sound;

	void Awake() {
		health = maxHealth;
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		materials = model.materials;
		respawnPoint = transform.position;

		//if (showIndicator) killIndicator.SetActive(true);
	}

	void FixedUpdate() {
		Util.PerformCheckedLateralMovement(this.gameObject, 0.75f, 0.6f, movementDelta * Time.fixedDeltaTime, ~Mask.Get(Layers.StickyLedge));
		Util.PerformCheckedVerticalMovement(this.gameObject, 2f, 0.2f, 0.5f, 30.0f);
	}

	// Update is called once per frame
	void Update() {
		movementDelta = Vector3.zero;

		if (apperanceDelay >= 0f) {
			apperanceDelay -= Time.deltaTime;
			model.enabled = false;
			collider.enabled = false;
			
			if (apperanceDelay < 0f) {
				model.enabled = true;
				collider.enabled = true;
			}
		}

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

		movementDelta += Util.ProcessKnockback(ref remainingKnockbackTime, knockbackInfo);

		if (transform.position.y < -45f) {
			shouldDie = true;
		}

		if (shouldDie) {
			if (canRespawn) {
				Sandbag sandbag = gameMan.SandbagPrefab.GetComponent<Sandbag>();
				sandbag.hasHealth = hasHealth;
				sandbag.canBeKnockedBack = canBeKnockedBack;
				sandbag.canRespawn = canRespawn;
				sandbag.apperanceDelay = apperanceDelay;
				sandbag.VulnerabilityMask = VulnerabilityMask;
				sandbag.VurnerableToHeadProjectiles = VurnerableToHeadProjectiles;
				sandbag.maxHealth = maxHealth;
				sandbag.health = health;
				sandbag.apperanceDelay = 2f;
				sandbag.model.enabled = false;
				sandbag.collider.enabled = false;

				Instantiate(gameMan.SandbagPrefab, respawnPoint, transform.rotation);
			}

			if (!wasHitByChop) gameMan.SpawnCorpse(3, corpsePos.position, transform.rotation, 2f, true);
			else gameMan.SpawnCorpse(3, corpsePos.position, transform.rotation, 2f, false);
			gameMan.SpawnParticle(4, corpsePos.position, 1f);
			Destroy_Sound.Post(gameObject);
			Destroy(gameObject);
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHitbox) {
			bool validHit = false;
			Vector3 sourcePosition = Vector3.zero;
			if (showIndicator) killIndicator.SetActive(false);

			if (other.GetComponentInParent<PlayerController>() != null) {
				int currentAttackBit = 1 << (int)gameMan.playerController.currentAttack;
				if (((int)VulnerabilityMask & currentAttackBit) != 0) {
					validHit = true;
				}
				sourcePosition = gameMan.playerController.transform.position;
			}
			else if (VurnerableToHeadProjectiles && other.GetComponentInParent<HeadProjectile>() != null) {
				validHit = true;
				sourcePosition = other.transform.position;
			}

			if (validHit) {
				Vector3 deltaNoY = sourcePosition - this.transform.position;
				deltaNoY.y = 0f;
				//this.transform.rotation = Quaternion.LookRotation(deltaNoY, Vector3.up);
				gameMan.SpawnParticle(12, other.transform.position, 1.2f);
				gameMan.SpawnParticle(15, other.transform.position, 0.7f);

				if (canBeKnockedBack) {
					GetKnockbackInfo getKnockbackInfo = other.gameObject.GetComponent<GetKnockbackInfo>();
					if (getKnockbackInfo != null) {
						knockbackInfo = getKnockbackInfo.GetInfo(this.gameObject);
						remainingKnockbackTime = knockbackInfo.time;
					}
				}

				if (other.GetComponentInParent<PlayerController>() != null) {
					if (gameMan.playerController.currentAttack == PlayerController.Attacks.Chop) {
						gameMan.playerController.ChangeMeter(1f);
						gameMan.SpawnParticle(12, other.transform.position, 1.6f);
						wasHitByChop = true;
						shouldDie = true;
					}
				}

				if (hasHealth) health -= 1f;
				hitflashTimer = 0.15f;
				if (health <= 0) {
					shouldDie = true;
				}


				animator.SetFloat("HitX", deltaNoY.normalized.x);
				animator.SetFloat("HitY", deltaNoY.normalized.z);
				animator.Play("Base Layer.GetHit_Tree", 0, 0f);
				Sound_Hit();
			}
		}
	}

	void Sound_Hit() {
		Get_Hit_Sound.Post(gameObject);
	}
}
