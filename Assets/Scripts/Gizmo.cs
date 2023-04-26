using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gizmo : MonoBehaviour
{
	public Color sphereColor;
	public float sphereRadius;

	void OnDrawGizmos() {
		Gizmos.color = sphereColor;
		Gizmos.DrawWireSphere(transform.position, sphereRadius);
	}
}

