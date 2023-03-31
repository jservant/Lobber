using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbSpawn : MonoBehaviour {
	public float despawnTime;
	public bool currentlySpawning;

	bool[] spawnedEnemies;
	float angleOffset;

	public GameObject enemy;
	public Animator anim;
	GameManager gameMan;
	[SerializeField] GameObject orbPrefab;

	void Start() {
		//orbPrefab = transform.Find("Orb"); // can't find properly despite being assigned in editor??? throwing errors even though it works
		anim = orbPrefab.GetComponent<Animator>();
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
	}

	void Update() {
	}

	public IEnumerator Spawning(int spawnAmount) {
		// ensure that this coroutine only runs one at a time.
		if (!currentlySpawning && gameMan != null) {
			gameMan.enemiesAlive += spawnAmount;
			currentlySpawning = true;

			orbPrefab.SetActive(true);
			anim.SetBool("DeSpawn", false);
			spawnedEnemies = new bool[spawnAmount];
			angleOffset = Random.Range(0.0f, 90.0f);
			yield return new WaitForSeconds(despawnTime / 2);

			for (int spawnCount = 0; spawnCount < spawnAmount; spawnCount += 1) {
				// Make sure we spawn a enemy that "hasn't been spawned yet"
				int randomIndex = Random.Range(0, spawnedEnemies.Length);
				while (spawnedEnemies[randomIndex]) {
					randomIndex += 1;
					if (randomIndex >= spawnedEnemies.Length) { randomIndex = 0; }
				}
				spawnedEnemies[randomIndex] = true;

				float angle = (float)randomIndex * 360.0f / (float)spawnAmount;
				Instantiate(enemy, this.transform.position + Vector3.down * 1.5f, Quaternion.AngleAxis(angle, Vector3.up));
				yield return new WaitForSeconds(0.3f);
			}

			yield return new WaitForSeconds(despawnTime / 2);

			anim.SetBool("DeSpawn", true);
			yield return new WaitForSeconds(0.4f);

			orbPrefab.SetActive(false);
			currentlySpawning = false;
		}
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(transform.position, 5f);
	}
}
