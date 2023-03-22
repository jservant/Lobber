using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadPickup : MonoBehaviour {
	//Popup Variables
	public float lifetime;
	public int healthOnCatch;
	public GameObject indicator;
	public float randomForce;

	private Vector3 targetPoint; //the point this is traveling to
	public bool canCollect;
	private float timeUntilCollect = 0.5f; //small delay where the head can't initially be caught

	public float flightAngle; //degree to which head flies upwards
	private float flightTime;
	private float currentFlightTime = 0f;
	public float gravity = 9.8f;
	private float Vx;
	private float Vy;

	public Transform skull;
	private Vector3 spinForce;

	//Pickup Variables
	public float RotationSpeed;
	public float FollowSpeed;
	public float GatherRadius;
	public float value;

	public bool isOnGround;
	public bool collected;

	GameManager gameMan;
	// Start is called before the first frame update
	void Start() {
		transform.rotation = Random.rotation;
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		timeUntilCollect = lifetime - timeUntilCollect;
		isOnGround = false;
		canCollect = false;

		FindPoint();
	}

	// Update is called once per frame
	void Update() {
		lifetime -= Time.deltaTime;

		if (lifetime <= 0) Destroy(this.gameObject);
		if (lifetime <= timeUntilCollect) canCollect = true;
		if (transform.position.y <= 0) if (indicator != null) Destroy(indicator);

		if (Physics.Raycast(transform.position, Vector3.down, 1.5f, ~(int)Layers.Ground)) {
			if (canCollect) {
				isOnGround = true;
				if (indicator != null) Destroy(indicator);
			}
		}

		if (isOnGround) {
			transform.rotation *= Quaternion.AngleAxis(RotationSpeed * Time.deltaTime, Vector3.up);

			if (!Physics.Raycast(transform.position, Vector3.down, 1.5f)) {
				transform.position += Vector3.down * gravity * Time.deltaTime;
			}

			if (Physics.CheckSphere(transform.position, GatherRadius, Mask.Get(Layers.PlayerHurtbox))) {
				transform.position += ((gameMan.player.transform.position + Vector3.up * 1) - transform.position).normalized * FollowSpeed * Time.deltaTime;
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
		Vx = Mathf.Sqrt(projectile_Velocity) * Mathf.Cos(flightAngle * Mathf.Deg2Rad);
		Vy = Mathf.Sqrt(projectile_Velocity) * Mathf.Sin(flightAngle * Mathf.Deg2Rad);

		// Calculate flight time.
		flightTime = target_Distance / Vx;

		// Rotate projectile to face the target.
		transform.rotation = Quaternion.LookRotation(targetPoint - transform.position);
	}

	void Flight() {
		if ((currentFlightTime < flightTime) && !isOnGround) {
			transform.Translate(0, (Vy - (gravity * currentFlightTime)) * Time.deltaTime, Vx * Time.deltaTime);
			skull.Rotate(spinForce * Time.deltaTime);
			currentFlightTime += Time.deltaTime;
		}
	}

	void UpdateIndicator() {
		if (indicator != null) indicator.transform.position = targetPoint;
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, GatherRadius);
	}

	void OnDestroy() {
		Destroy(this.transform.parent.gameObject);
		if (collected) {
			gameMan.playerController.meter += value;
			if (!isOnGround) gameMan.playerController.health += healthOnCatch;
		}
	}
}
