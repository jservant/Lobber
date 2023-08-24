using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pickup : MonoBehaviour {

	public enum Type { Skull = 0, GoldenSkull, Health, Crystal, RedSkull };
	public Type pickupType;

	[Header("References:")]
	public Transform skull;
	public GameObject indicator;
	GameManager gameMan;
	PlayerController playerController;
	public MeshRenderer headModel;
	TrailRenderer headTrail;
	public AK.Wwise.Event goldenSkullDrop;
	public AK.Wwise.Event goldenSkullStop;
	public Transform flash;

	[Header("Movement:")]
	public float flightAngle; //degree to which head flies upwards
	private float flightTime;
	private float currentFlightTime = 0f;
	private Vector3 targetPoint; //the point this is traveling to
	private Vector3 spinForce;
	private Vector2 velocity;
	public float gravity = 9.8f;
	public float rotationSpeed;
	public float followSpeed;
	public float gatherRadius;

	[Header("Lifespan:")]
	public float lifetime;
	public float timeUntilCollect; //small delay where the head can't initially be caught
	public bool isOnGround;
	public bool collected;
	public float randomForce;
	public float forceOffset;
	private float lowLifetime = 4f; //when to start blinking to indicate low lifetime
	private float blinkDuration = 0.5f;
	private float blinkTime = 0f;
	private float debugAttempts = 100000f;

	[Header("Bonuses:")]
	public float meterValue;
	public int healthValue;
	public float meterCatchBonus; //how much bonus value is given when caught
	public int healthCatchBonus; 

	void Start() {
		if (pickupType != Type.Health) transform.rotation = Random.rotation;
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		playerController = gameMan.playerController;
		headTrail = this.transform.Find("Position").GetComponent<TrailRenderer>();
		timeUntilCollect = lifetime - timeUntilCollect;
		isOnGround = false;

		if (transform.position.y > -4) {
			SetRandomFlightPath(); 
		}

		if (pickupType == Type.GoldenSkull) goldenSkullDrop.Post(gameObject);
	}

	void SetRandomFlightPath() {
		// NOTE(Roskuski): Loop terminates with a break.
		float ranForceX = 0f;
		float ranForceZ = 0f;

		while (true) {
			// TODO(@Ryan): Cap ranforce at 7 and -7 if below or above

			ranForceX = Random.Range(-randomForce, randomForce);
			if (Mathf.Abs(ranForceX) < forceOffset) ranForceX += forceOffset * Mathf.Sign(ranForceX);
			ranForceZ = Random.Range(-randomForce, randomForce);
			if (Mathf.Abs(ranForceZ) < forceOffset) ranForceZ += forceOffset * Mathf.Sign(ranForceZ);

			Vector3 point = new Vector3(transform.position.x + ranForceX, transform.position.y + 5f, transform.position.z + ranForceZ); //calculates randomized point on XZ axis
			RaycastHit rayHit;
			if (Physics.Raycast(point, Vector3.down, out rayHit, 100f)) {
				NavMeshHit navHit;
				if (NavMesh.SamplePosition(rayHit.point, out navHit, 0.1f, NavMesh.AllAreas)) { //Checks to see if it's above a collider on the ground layer
					targetPoint = point;
					targetPoint.y = navHit.position.y + 0.2f;
					float ranX = Random.Range(-500f, 500f);
					float ranY = Random.Range(-500f, 500f);
					float ranZ = Random.Range(-500f, 500f);
					spinForce = new Vector3(ranX, ranY, ranZ);
					if (pickupType == Type.Health) spinForce = new Vector3(0f, rotationSpeed, 0f);

					// Calculate distance to target
					float target_Distance = Vector3.Distance(transform.position, targetPoint);

					// Calculate the velocity needed to throw the object to the target at specified angle.
					float projectile_Velocity = target_Distance / (Mathf.Sin(2 * flightAngle * Mathf.Deg2Rad) / gravity);

					// Extract the X  Y componenent of the velocity
					velocity = new Vector2(Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(flightAngle * Mathf.Deg2Rad), Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(flightAngle * Mathf.Deg2Rad));

					// Calculate flight time.
					flightTime = target_Distance / velocity.x;
					currentFlightTime = 0f;

					// Rotate projectile to face the target.
					transform.rotation = Quaternion.LookRotation(targetPoint - transform.position);
					break;
				}
                else {
					debugAttempts -= 1;
					if (debugAttempts <= 0) {
						Destroy(gameObject);
						Debug.Log("Couldn't find landing point with ranForce X " + ranForceX + " and ranforce Z " + ranForceZ + "(No Nav Mesh Hit)");
						break;
					}
				}
			}
			else {
				debugAttempts -= 1;
				if (debugAttempts <= 0) {
					Destroy(gameObject);
					Debug.Log("Couldn't find landing point with ranForce X " + ranForceX + " and ranforce Z " + ranForceZ + "(No Ground Layer)");
					break;
				}
			}
		}
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != (int)Layers.Ground && other.gameObject.layer != (int)Layers.Pickup && other.gameObject.layer != (int)Layers.SoundTrigger && other.gameObject.layer != (int)Layers.StickyLedge && ((currentFlightTime < flightTime) && !isOnGround)) {
			SetRandomFlightPath();
		}
	}

	void Update() {
		lifetime -= Time.deltaTime;
		if (lifetime <= lowLifetime) StartBlinking();
		if (lifetime <= 0) Destroy(this.gameObject);
		if (transform.position.y <= 0) if (indicator != null) Destroy(indicator);

		if (Physics.Raycast(transform.position, Vector3.down, 1.0f, Mask.Get(Layers.Ground))) {
			if (lifetime <= timeUntilCollect) {
				isOnGround = true;
				if (indicator != null) Destroy(indicator);
			}
		}

		if (isOnGround && !collected) {
			transform.rotation *= Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.up);

			if (!Physics.Raycast(transform.position, Vector3.down, 1.0f) && !gameMan.transitioningLevel) {
				transform.position += Vector3.down * gravity * Time.deltaTime;
			}

			if (Physics.CheckSphere(transform.position, gatherRadius, Mask.Get(Layers.PlayerHurtbox))) {
				transform.position += ((gameMan.player.transform.position + Vector3.up * 1) - transform.position).normalized * followSpeed * Time.deltaTime;
			}

			headTrail.enabled = false;
			if (gameMan.transitioningLevel) {
				gatherRadius = 300f;
				followSpeed = 40f;
			}
            else {
				if (pickupType == Type.RedSkull) Spawn();
            }

			if (indicator != null) Destroy(indicator);
		}
		else UpdateIndicator();

		// Fly though the air
		if ((currentFlightTime < flightTime) && !isOnGround) {
			transform.Translate(0, (velocity.y - (gravity * currentFlightTime)) * Time.deltaTime, velocity.x * Time.deltaTime);
			skull.Rotate(spinForce * Time.deltaTime);
			currentFlightTime += Time.deltaTime;
		}

		if (collected) Destroy(this);
		if (pickupType == Type.GoldenSkull) flash.position = transform.position;
	}

	void UpdateIndicator() {
		if (indicator != null) indicator.transform.position = targetPoint;
	}

	void StartBlinking() {
		
		if (blinkTime < blinkDuration) {
			headModel.enabled = false;
		}
		else {
			headModel.enabled = true;
		}

		if (blinkTime >= blinkDuration * 2f) {
			blinkTime = 0f;
			blinkDuration -= 0.1f;
			if (blinkDuration <= 0f) blinkDuration = 0.1f;
		}

		blinkTime += Time.deltaTime;
	}

	public void Sound_GoldenSkullDrop() {
		goldenSkullDrop.Post(gameObject);
	}

	void Spawn() {
		var enemyPrefab = gameMan.BasicPrefab[1];
		var enemyInstance = Instantiate(enemyPrefab, transform.position, Quaternion.AngleAxis(180, Vector3.up));
		var enemy = enemyInstance.GetComponent<Basic>();
		enemy.health -= 5;
		goldenSkullStop.Post(gameObject);
		Util.SpawnFlash(gameMan, 8, transform.position, false);
		gameMan.SpawnParticle(12, transform.position, 1f);
		Destroy(this.gameObject);
    }

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, gatherRadius);
	}

	void OnDestroy() {
		Destroy(this.transform.parent.gameObject);
		if (!isOnGround) { //get +1 value if you can catch it!
			meterValue += meterCatchBonus; 
			healthValue += healthCatchBonus;
			gameMan.AddToKillStreak(0, 1f);
			Initializer.save.versionLatest.headsCaught++;
		} 
		if (collected) {
			switch(pickupType) {
				case Type.Skull:
					playerController.meter += meterValue;
					gameMan.MeterDialGrow(0.5f);
					Util.SpawnFlash(gameMan, 2, transform.position, true);
					break;
				case Type.GoldenSkull:
					playerController.meter = gameMan.playerController.meterMax;
					playerController.frenzyTimer = 8f;
					gameMan.MeterDialGrow(0.5f);
					Util.SpawnFlash(gameMan, 1, transform.position, true);
					break;
				case Type.Health:
					playerController.health += healthValue;
					gameMan.HealthDialGrow(0.5f);
					Util.SpawnFlash(gameMan, 8, transform.position, true);
					gameMan.SpawnParticle(16, transform.position, 0.4f);
					break;
				case Type.Crystal:
					Util.SpawnFlash(gameMan, 6, transform.position, true);
					if (playerController.crystalCount == 0) {
                        GameObject crystalPatchInstance = Instantiate(gameMan.crystalPatch, playerController.crystalHolster.position, playerController.crystalHolster.rotation);
                        crystalPatchInstance.transform.parent = playerController.crystalHolster;
                        //TODO(@Jaden): Add a way to spawn a new UI image for each crystal held going right by a degree of units
                        gameMan.crystalPickupImage.enabled = true;
                        gameMan.waypointMarker.enabled = true;
						gameMan.waypointTracking = true;
						CrystalDropoff.indicator.enabled = true;
                    }
                    playerController.crystalCount++;
					if (playerController.crystalCount > 1) {
						gameMan.crystalCountText.text = "x" + playerController.crystalCount;
					}
					if (gameMan.transitioningLevel) {
						playerController.health += 1;
						gameMan.HealthDialGrow(0.5f);
					}
					break;
				case Type.RedSkull:
					playerController.meter += meterValue;
					gameMan.MeterDialGrow(0.5f);
					Util.SpawnFlash(gameMan, 8, transform.position, true);
					break;

				default:
					Debug.Log("Something is wrong with a pickup");
					break;
			}
		}
		if (pickupType == Type.GoldenSkull) goldenSkullStop.Post(gameObject);

	}
}
