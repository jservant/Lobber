using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class Pickup : MonoBehaviour {

	public enum Type { Skull = 0, GoldenSkull, Health };
	public Type pickupType;

	[Header("References:")]
	public Transform skull;
	public GameObject indicator;
	GameManager gameMan;
	PlayerController playerController;
	public MeshRenderer headModel;
	TrailRenderer headTrail;
	public Material goldMat;

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
	private float lowLifetime = 3.5f; //when to start blinking to indicate low lifetime
	private float blinkDuration = 0.5f;
	private float blinkTime = 0f;

	[Header("Bonuses:")]
	public float meterValue;
	public int healthValue;
	public float meterCatchBonus; //how much bonus value is given when caught
	public int healthCatchBonus; 

	void Start() {
		if (pickupType != Type.Health) transform.rotation = Random.rotation;
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		playerController = gameMan.playerController;
		//headModel = transform.Find("SkullPosition/Skeleton_Head").GetComponent<MeshRenderer>();
		headTrail = GetComponent<TrailRenderer>();
		timeUntilCollect = lifetime - timeUntilCollect;
		isOnGround = false;

		if (transform.position.y > 0) {
			SetRandomFlightPath(); 
		}
	}

	void SetRandomFlightPath() {
		// NOTE(Roskuski): Loop terminates with a break.
		while (true) {
			// TODO(@Ryan): Cap ranforce at 3 and -3 if below or above
			float ranForceX = Random.Range(-randomForce, randomForce);
			float ranForceZ = Random.Range(-randomForce, randomForce);
			if (ranForceX < 3 && ranForceX >= 0) ranForceX = 3;
			if (ranForceZ > -3 && ranForceZ <= 0) ranForceX = -3;
			Vector3 point = new Vector3(transform.position.x + ranForceX, 5f, transform.position.z + ranForceZ); //calculates randomized point on XZ axis
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
			}
		}
	}
	
	void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer != (int)Layers.Ground && other.gameObject.layer != (int)Layers.Pickup && ((currentFlightTime < flightTime) && !isOnGround)) {
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

			if (!Physics.Raycast(transform.position, Vector3.down, 1.0f)) {
				transform.position += Vector3.down * gravity * Time.deltaTime;
			}

			if (Physics.CheckSphere(transform.position, gatherRadius, Mask.Get(Layers.PlayerHurtbox))) {
				transform.position += ((gameMan.player.transform.position + Vector3.up * 1) - transform.position).normalized * followSpeed * Time.deltaTime;
			}

			headTrail.enabled = false;
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

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, gatherRadius);
	}

	void OnDestroy() {
		Destroy(this.transform.parent.gameObject);
		if (!isOnGround) { meterValue += meterCatchBonus; healthValue += healthCatchBonus; } //get +1 value if you can catch it!
		if (collected) {
			if (pickupType == Type.Skull) { // skull
				gameMan.playerController.meter += meterValue;
			}

			if (pickupType == Type.GoldenSkull) { // golden skull
				gameMan.playerController.meter = gameMan.playerController.meterMax;
				gameMan.playerController.frenzyTimer = 5f;
			}

			if (pickupType == Type.Health) { // health
				gameMan.playerController.health += healthValue;
			}
		}
	}
}
