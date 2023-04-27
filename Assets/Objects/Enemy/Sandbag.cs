using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandbag : MonoBehaviour {
	public bool hasHealth;
	public bool canBeKnockedBack;
	KnockbackInfo knockbackInfo;
	float remainingKnockbackTime;

	Vector3 movementDelta;

	public float maxHealth;
	public float health;

	GameManager gameManager;

	public SkinnedMeshRenderer model;
	Material[] materials;
	public Material hitflashMat;
	float hitflashTimer = 0;

	public AK.Wwise.Event Get_Hit_Sound;

	void Start() {
		health = maxHealth;
		gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
		materials = model.materials;
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
	}

	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHitbox) {
			if (canBeKnockedBack) {
				GetKnockbackInfo getKnockbackInfo = other.gameObject.GetComponent<GetKnockbackInfo>();
				if (getKnockbackInfo != null) {
					knockbackInfo = getKnockbackInfo.GetInfo(this.gameObject);
					remainingKnockbackTime = knockbackInfo.time;
				}
			}

			if (hasHealth) health -= 1f;
			hitflashTimer = 0.15f;
			if (health <= 0) {
				Destroy(gameObject);
			}

			Sound_Hit();
		}
	}

	void Sound_Hit() {
		Get_Hit_Sound.Post(gameObject);
	}
}
