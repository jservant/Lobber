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

	[SerializeField] [BitField] PlayerController.AttackBitMask VulnerabilityMask;
	[SerializeField] bool VurnerableToHeadProjectiles;

	Vector3 movementDelta;

	public float maxHealth;
	public float health;

	GameManager gameMan;

	public SkinnedMeshRenderer model;
	Material[] materials;
	public Material hitflashMat;
	float hitflashTimer = 0;
	public Animator animator;

	public AK.Wwise.Event Get_Hit_Sound;

	void Start() {
		health = maxHealth;
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		materials = model.materials;
		respawnPoint = transform.position;
	}

	void FixedUpdate() {
		Util.PerformCheckedLateralMovement(this.gameObject, 0.75f, 0.6f, movementDelta * Time.fixedDeltaTime, ~Mask.Get(Layers.StickyLedge));
		Util.PerformCheckedVerticalMovement(this.gameObject, 0.75f, 0.2f, 0.5f, 30.0f);
	}

	// Update is called once per frame
	void Update() {
		movementDelta = Vector3.zero;

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

		if (transform.position.y < -20f) {
			if (canRespawn) Respawn();
			Destroy(gameObject);
		}
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHitbox) {
			bool validHit = false;
			Vector3 sourcePosition = Vector3.zero;

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
						if (canRespawn) Respawn();
						Destroy(gameObject);
                    }
				}

					if (hasHealth) health -= 1f;
				hitflashTimer = 0.15f;
				if (health <= 0) {
					if (canRespawn) Respawn();
					Destroy(gameObject);
				}

				animator.SetFloat("HitX", deltaNoY.normalized.x);
				animator.SetFloat("HitY", deltaNoY.normalized.z);
				animator.Play("Base Layer.GetHit_Tree", 0, 0f);
				Sound_Hit();
			}
		}
	}

	void Respawn() {
		var newSandbag = Instantiate(this, respawnPoint, transform.rotation);
    }

	void Sound_Hit() {
		Get_Hit_Sound.Post(gameObject);
	}
}
