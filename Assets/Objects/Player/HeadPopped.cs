using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadPopped : MonoBehaviour {
	private Rigidbody rb;
	GameManager gameMan;

	public float value;
	public float lifetime;

	public bool collected;

	// Start is called before the first frame update
	void Start() {
		rb = GetComponent<Rigidbody>();
		gameMan = this.transform.Find("/GameManager").GetComponent<GameManager>();
	}

	// Update is called once per frame
	void Update() {
		if (lifetime >= 0) {
			lifetime -= Time.deltaTime;
		}
		else Destroy(this.gameObject);

		if (transform.position.y <= 0) {
			Destroy(this.gameObject);
		}
	}

	void OnDestroy() {
		Destroy(this.transform.parent.gameObject);
		if (collected) gameMan.playerController.meter += value;
	}
}
