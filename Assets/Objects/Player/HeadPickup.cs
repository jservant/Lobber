using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadPickup : MonoBehaviour {
	public float RotationSpeed;
	public float FollowSpeed;
	public float FallSpeed;
	public float GatherRadius;

	Vector3 popDirection;
	float popTime;

	GameManager gameMan;
	// Start is called before the first frame update
	void Start() {
		transform.rotation = Random.rotation;
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
	}

	// Update is called once per frame
	void Update() {
		popTime += Time.deltaTime;
		transform.rotation *= Quaternion.AngleAxis(RotationSpeed * Time.deltaTime, Vector3.up);

		if (!Physics.Raycast(transform.position, Vector3.down, 1.5f)) {
			transform.position += Vector3.down * FallSpeed * Time.deltaTime;
		}

		if (Physics.CheckSphere(transform.position, GatherRadius, Mask.Get(Layers.PlayerHurtbox))) {
			transform.position += ((gameMan.player.transform.position + Vector3.up * 1) - transform.position).normalized * FollowSpeed * Time.deltaTime;
		}
	}

	void OnDrawGizmosSelected() {
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(transform.position, GatherRadius);
	}
}
