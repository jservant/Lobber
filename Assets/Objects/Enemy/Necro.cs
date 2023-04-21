using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/* Movement Patterns
 * - Prioty, maintain LoS while also being at the same elevation of the player
 * Create "floor" volumes That dictate it's minium height, pair that with trying to go to the same height as the player?
 */

public class Necro : MonoBehaviour {
	enum Directive {
		Spawn = 0, 
		Attack,
		Wander,
	}
	Directive directive;


	Vector3 movementDelta;

	void Start() {
	}

	void FixedUpdate() {
		this.transform.position += movementDelta * Time.fixedDeltaTime;
	}

	void Update() {
		;
	}
}
