using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadPickup : MonoBehaviour {
	//Popup Variables
	private Rigidbody rb;
	public float lifetime;
	public int healthOnCatch;
	public GameObject indicator;
	RandomForce randomForce;

	public bool canCollect;
	private float timeUntilCollect = 1.0f; //small delay where the head can't initially be caught

	//Pickup Variables
	public float RotationSpeed;
	public float FollowSpeed;
	public float FallSpeed;
	public float GatherRadius;
	public float value;

	public bool isOnGround;
	public bool collected;

	//Vector3 popDirection;
	//float popTime;

	GameManager gameMan;
	// Start is called before the first frame update
	void Start() {
		rb = GetComponent<Rigidbody>();
		randomForce = GetComponent<RandomForce>();
		transform.rotation = Random.rotation;
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		timeUntilCollect = lifetime - timeUntilCollect;
		isOnGround = false;
		canCollect = false;
	}

	// Update is called once per frame
	void Update() {
		//popTime += Time.deltaTime;
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
			randomForce.enabled = false;
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
	}

	void UpdateIndicator() {
		if (indicator != null) indicator.transform.position = new Vector3(transform.position.x, indicator.transform.position.y, transform.position.z);
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
