using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadPickup : MonoBehaviour {
	//Popup Variables
	private Rigidbody rb;
	public float lifetime;
	public int healthOnCatch;
	public GameObject indicator;
	public float randomForce;

	private Vector3 targetPoint; //the point this is traveling to
	public bool canCollect;
	private float timeUntilCollect = 1.0f; //small delay where the head can't initially be caught
	public float flightTime; //total time this spends in the air
	private float currentFlightTime = 0f;

	//Pickup Variables
	public float RotationSpeed;
	public float FollowSpeed;
	public float FallSpeed;
	public float GatherRadius;
	public float value;

	public bool isOnGround;
	public bool collected;

	GameManager gameMan;
	// Start is called before the first frame update
	void Start() {
		rb = GetComponent<Rigidbody>();
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

		if (Physics.Raycast(transform.position, Vector3.down, 1.5f)) {
			if (canCollect) {
				isOnGround = true;
				if (indicator != null) Destroy(indicator);
			}
		}

		if (isOnGround) {
			rb.isKinematic = true;
			transform.rotation *= Quaternion.AngleAxis(RotationSpeed * Time.deltaTime, Vector3.up);
			
			if (!Physics.Raycast(transform.position, Vector3.down, 1.5f)) {
				transform.position += Vector3.down * FallSpeed * Time.deltaTime;
			}

			if (Physics.CheckSphere(transform.position, GatherRadius, Mask.Get(Layers.PlayerHurtbox))) {
				transform.position += ((gameMan.player.transform.position + Vector3.up * 1) - transform.position).normalized * FollowSpeed * Time.deltaTime;
			}

			if (indicator != null) Destroy(indicator);
		}
		else UpdateIndicator();

		CalculateFlight();
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
				}
				else FindPoint();
			}
		}
	}

	void CalculateFlight() {
		currentFlightTime += Time.deltaTime;
		//transform.position
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
