using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadPickup : MonoBehaviour {

	[Header("References:")]
	public Transform skull;
	public GameObject indicator;
	GameManager gameMan;
	PlayerController playerController;
	MeshRenderer headModel;
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
	public float timeUntilCollect = 0.5f; //small delay where the head can't initially be caught
	public bool isOnGround;
	public bool collected;
	public float randomForce;

	[Header("Bonuses:")]
	public int healthOnCatch = 2;
	public float meterValue;
	bool meterOrHealth = true;
	bool isGold = false;
	public int goldChance;

	void Start() {
		transform.rotation = Random.rotation;
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		playerController = gameMan.playerController;
		headModel = transform.Find("SkullPosition/Skeleton_Head").GetComponent<MeshRenderer>();
		timeUntilCollect = lifetime - timeUntilCollect;
		isOnGround = false;

		int goldRoll = Random.Range(1, goldChance);
		if (goldRoll == 1) {
			headModel.material = goldMat;
			isGold = true;
		}

		float healthChance = (66 / playerController.healthMax) * (playerController.healthMax - playerController.health);
		Debug.Log("Chance for this to be a health pickup: " + healthChance + "%");

		FindPoint();
	}

	void Update() {
		lifetime -= Time.deltaTime;

		if (lifetime <= 0) Destroy(this.gameObject);
		if (transform.position.y <= 0) if (indicator != null) Destroy(indicator);

		if (Physics.Raycast(transform.position, Vector3.down, 1.5f, ~(int)Layers.Ground)) {
			if (lifetime <= timeUntilCollect) {
				isOnGround = true;
				if (indicator != null) Destroy(indicator);
			}
		}

		if (isOnGround) {
			transform.rotation *= Quaternion.AngleAxis(rotationSpeed * Time.deltaTime, Vector3.up);

			if (!Physics.Raycast(transform.position, Vector3.down, 1.5f)) {
				transform.position += Vector3.down * gravity * Time.deltaTime;
			}

			if (Physics.CheckSphere(transform.position, gatherRadius, Mask.Get(Layers.PlayerHurtbox))) {
				transform.position += ((gameMan.player.transform.position + Vector3.up * 1) - transform.position).normalized * followSpeed * Time.deltaTime;
			}

			if (indicator != null) Destroy(indicator);
		}
		else UpdateIndicator();

		Flight();

		if (collected) Destroy(this);
	}

	void FindPoint() {
		bool foundPoint = false;

		if (transform.position.y > 0) {
			if (foundPoint == false) {
				// TODO(@Ryan): Cap ranforce at 3 and -3 if below or above
				float ranForceX = Random.Range(-randomForce, randomForce);
				float ranForceZ = Random.Range(-randomForce, randomForce);
				Vector3 point = new Vector3(transform.position.x + ranForceX, 5f, transform.position.z + ranForceZ); //calculates randomized point on XZ axis
				RaycastHit hit;

				if (Physics.Raycast(point, Vector3.down, out hit, 10f, ~(int)Layers.Ground)) { //Checks to see if it's above a collider on the ground layer
					foundPoint = true;
					targetPoint = point;
					targetPoint.y = hit.point.y + 0.2f;
					float ranX = Random.Range(-500f, 500f);
					float ranY = Random.Range(-500f, 500f);
					float ranZ = Random.Range(-500f, 500f);
					spinForce = new Vector3(ranX, ranY, ranZ);
					CalculateFlight();
				}
				else FindPoint();
			}
		}
	}

	void CalculateFlight() {
		// Calculate distance to target
		float target_Distance = Vector3.Distance(transform.position, targetPoint);

		// Calculate the velocity needed to throw the object to the target at specified angle.
		float projectile_Velocity = target_Distance / (Mathf.Sin(2 * flightAngle * Mathf.Deg2Rad) / gravity);

		// Extract the X  Y componenent of the velocity
		velocity = new Vector2(Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(flightAngle * Mathf.Deg2Rad), Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(flightAngle * Mathf.Deg2Rad));

		// Calculate flight time.
		flightTime = target_Distance / velocity.x;

		// Rotate projectile to face the target.
		transform.rotation = Quaternion.LookRotation(targetPoint - transform.position);
	}

	void Flight() {
		if ((currentFlightTime < flightTime) && !isOnGround) {
			transform.Translate(0, (velocity.y - (gravity * currentFlightTime)) * Time.deltaTime, velocity.x * Time.deltaTime);
			skull.Rotate(spinForce * Time.deltaTime);
			currentFlightTime += Time.deltaTime;
		}
	}

	void UpdateIndicator() {
		if (indicator != null) indicator.transform.position = targetPoint;
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, gatherRadius);
	}

	void OnDestroy() {
		Destroy(this.transform.parent.gameObject);
		if (collected) {
			if (isGold) {
				gameMan.playerController.meter = gameMan.playerController.meterMax;
				gameMan.playerController.frenzyTimer = 5f;
			}
			else { gameMan.playerController.meter += meterValue; }
			if (!isOnGround) gameMan.playerController.health += healthOnCatch;
		}
	}
}
