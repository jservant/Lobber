using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grow : MonoBehaviour {
	public float lifetime;
	private float currentLifetime;
	public float scaleSpeed;

	// Start is called before the first frame update
	void Start() {
		transform.localScale = new Vector3(0f, 0f, 1f);
	}

	// Update is called once per frame
	void Update() {
		currentLifetime += Time.deltaTime;
		transform.localScale = new Vector3(currentLifetime * scaleSpeed, currentLifetime * scaleSpeed, 1f);

		if (currentLifetime >= lifetime) Destroy(gameObject);
	}
}
