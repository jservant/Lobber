using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnterOrb : MonoBehaviour
{
	private void OnTriggerEnter(Collider other) {
		if (other.gameObject.layer == (int)Layers.PlayerHurtbox) {
			GameManager.OnRestartConfirm();
		}
	}
}
