using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveTrap : MonoBehaviour {
	public GameObject hitbox;
	private Collider capsule;
	private bool isArmed;
	public float armTime; //time it takes for the trap to rearm itself
	private float currentArmTime;
	public float triggerTime; //active frames of the trap hitbox
	public float currentTriggerTime;
	public Animator anim;
	public MeshRenderer barrel;
	public Transform explosionPoint;

	private GameManager gameMan;
	private PlayerController player;

	private bool check = true;

	public AK.Wwise.Event ExplosionSound;

	void Start() {
		capsule = this.GetComponent<CapsuleCollider>();
		hitbox.SetActive(false);
		isArmed = true;
		anim.Play("BombBarrel");
		currentArmTime = armTime;
		currentTriggerTime = 0f;
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		player = transform.Find("/Player").GetComponent<PlayerController>();
	}

	void Update() {
		if (isArmed == false) {
			currentArmTime -= Time.deltaTime;
			barrel.enabled = false;
			capsule.enabled = false;
			check = true;
			if (currentArmTime <= 0) isArmed = true;
		}
		else {
			barrel.enabled = true;
			capsule.enabled = true;
			if (check) {
				anim.Play("BombBarrel", -1, 0f);
				check = false;
			}
		}

		if (currentTriggerTime <= 0) {
			hitbox.SetActive(false);
		}
		else currentTriggerTime -= Time.deltaTime;

	}

	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHitbox || other.gameObject.layer == (int)Layers.AgnosticHitbox) {
			if (isArmed) SpringTrap();
		}
	}

	public void SpringTrap() {
		isArmed = false;
		currentArmTime = armTime;
		currentTriggerTime = triggerTime;
		hitbox.SetActive(true);
		gameMan.SpawnParticle(2, explosionPoint.position, 1f);
		Vector3 newPoint = new Vector3(explosionPoint.position.x, explosionPoint.position.y + 0.5f, explosionPoint.position.z);
		Util.SpawnFlash(gameMan, 4, newPoint, false);
		Explosion_Sound();
		gameMan.ShakeCamera(5f, 0.5f);
		if (GameObject.Find("HapticManager") != null) HapticManager.PlayEffect(player.hapticEffects[1], this.transform.position);
	}

	void Explosion_Sound() {
		ExplosionSound.Post(gameObject);
	}

	void OnDrawGizmos() {
		//Gizmos.color = Color.red;
		//Gizmos.DrawWireSphere(transform.position, 10f);
	}
}
