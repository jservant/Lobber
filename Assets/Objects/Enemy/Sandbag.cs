using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sandbag : MonoBehaviour
{
    public bool hasHealth;
    public bool canBeKnockedBack;
	KnockbackInfo knockbackInfo;
	float remainingKnockbackTime;

	public float maxHealth;
    public float health;

	GameManager gameManager;

	public SkinnedMeshRenderer model;
	Material[] materials;
	public Material hitflashMat;
	float hitflashTimer = 0;

	void Start()
    {
        health = maxHealth;
		gameManager = transform.Find("/GameManager").GetComponent<GameManager>();
		materials = model.materials;
	}

    // Update is called once per frame
    void Update()
    {
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
			if (canBeKnockedBack) {
				GetKnockbackInfo getKnockbackInfo = other.gameObject.GetComponent<GetKnockbackInfo>();
				if (getKnockbackInfo != null) {
					knockbackInfo = getKnockbackInfo.GetInfo(this.gameObject);
					remainingKnockbackTime = knockbackInfo.time;
				}
			}

			if (hasHealth) health -= 1f;
			hitflashTimer = 0.1f;
			if (health <= 0) {
				Destroy(gameObject);
			}
		}
	}
}
