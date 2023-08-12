using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NecroProjectile : MonoBehaviour {
	[SerializeField] Vector3 moveDirection;
	[SerializeField] public float MoveSpeed = 14f;
	[SerializeField] public float TurnSpeed = 180.0f;
	public float lifeTime;
	[SerializeField] bool forcedFall = false;
	bool isPlayerProjectile;

	public GameObject stunSphere;

	// NOTE(Roskuski): External references
	GameManager gameMan;

	public AK.Wwise.Event Fireball;
	public AK.Wwise.Event Deflect;

	void Start() {
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();

		if (this.transform.parent != null) {
			moveDirection = this.transform.parent.rotation * Vector3.forward;
		}
	}

	void OnTriggerEnter(Collider other) {
		if (this.transform.parent == null) {

			switch ((Layers)other.gameObject.layer) {
				case Layers.PlayerHitbox:
					if (!isPlayerProjectile && other.CompareTag("Fireball") == false) {
						if (other.GetComponent<HeadProjectile>() != null) {
							Destroy(other.gameObject);
						}
						SwapLayer();
					}
					break;

				case Layers.PlayerHurtbox:
					if (!isPlayerProjectile) {
						if (other.GetComponent<PlayerController>() != null && gameMan.playerController.currentAttack != PlayerController.Attacks.Dashing && gameMan.playerController.currentAttack != PlayerController.Attacks.LethalDash) {
							GameObject.Destroy(this.gameObject);
							Fireball.Post(gameObject);
						}
					}
					break;

				case Layers.Ground:
					if (isPlayerProjectile) Explode();
					Fireball.Post(gameObject);
					GameObject.Destroy(this.gameObject);
					break;

				case Layers.EnemyHurtbox:
					if (isPlayerProjectile) Explode();
					Fireball.Post(gameObject);
					GameObject.Destroy(this.gameObject);
					break;

				case Layers.NoToonShader:
					if (isPlayerProjectile) Explode();
					Fireball.Post(gameObject);
					GameObject.Destroy(this.gameObject);
					break;

				case Layers.AgnosticHurtbox:
					if (isPlayerProjectile) Explode();
					if (other.GetComponent<ExplosiveTrap>() != null) {
						other.GetComponent<ExplosiveTrap>().SpringTrap();
                    }
					Fireball.Post(gameObject);
					GameObject.Destroy(this.gameObject);
					break;

				default:
					break;
			}
		}

	}

	void FixedUpdate() {
		if (this.transform.parent == null) {
			if (!isPlayerProjectile) {
				Vector3 deltaToPlayer = gameMan.player.position - this.transform.position + Vector3.up * 1.00f;
				moveDirection = Vector3.RotateTowards(moveDirection.normalized, deltaToPlayer.normalized, Mathf.PI * 2f * (TurnSpeed / 360) * Time.fixedDeltaTime, 0);

				if (deltaToPlayer.magnitude > 0.50f) {
					//forcedFall = true; 
				}
				if (forcedFall) {
					this.transform.position += Vector3.down * 2.0f * Time.fixedDeltaTime;
					moveDirection.y = 0;
				}
			}
			this.transform.position += moveDirection * MoveSpeed * Time.fixedDeltaTime;
			this.transform.rotation = Quaternion.LookRotation(moveDirection, Vector3.up);

		}
	}

	void Update() {
		if (this.transform.parent == null) {
			this.transform.localScale = new Vector3(1, 1, 1);
		}

		if (lifeTime > 0) {
			lifeTime -= Time.deltaTime;
		}
		else {
			Destroy(this.gameObject);
		}

		if (gameMan.transitioningLevel && !isPlayerProjectile) {
			SwapLayer();
        }
	}

	void SwapLayer() {
		isPlayerProjectile = true;
		moveDirection = -moveDirection;
		moveDirection.y = 0f;
		MoveSpeed = MoveSpeed * 2f;
		this.gameObject.layer = LayerMask.NameToLayer("PlayerHitbox");
		Util.SpawnFlash(gameMan, 7, transform.position, true);
		Deflect.Post(gameObject);
		lifeTime = 4f;
    }

	void Explode() {
		var _stunSphere = Instantiate(stunSphere, transform.position, Quaternion.identity);
		_stunSphere.GetComponent<StunSphere>().damage = 6f;
		gameMan.SpawnParticle(9, transform.position, 2f);
		Util.SpawnFlash(gameMan, 3, transform.position, false);
		Destroy(this.gameObject);
	}

    private void OnDestroy() {
		gameMan.SpawnParticle(9, transform.position, 1f);
		Util.SpawnFlash(gameMan, 7, transform.position, true);
	}
}
