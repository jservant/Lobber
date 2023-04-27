using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbSpawn : MonoBehaviour {

	public int basicAmount;
	public int explodingAmount;

	float despawnTime = 3;
	bool[] spawnedEnemies;
	float angleOffset;

	Animator anim;
	GameManager gameMan;
	public Material crystalizedMat;
	bool hasCrystalEnemySpawned = false;

	void Start() {
		anim = transform.Find("SpawnOrbV2").GetComponent<Animator>();
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		StartCoroutine(Spawning());
	}

	public IEnumerator Spawning() {
		int spawnAmount = basicAmount + explodingAmount;
		gameMan.enemiesAlive += spawnAmount;

		anim.SetBool("DeSpawn", false);
		spawnedEnemies = new bool[basicAmount + explodingAmount];
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
			if (randomIndex >= basicAmount) { 
				Instantiate(gameMan.ExplodingPrefab, this.transform.position + Vector3.down * 1.5f, Quaternion.AngleAxis(angle, Vector3.up));
			}
			else {
				GameObject basicInstance = Instantiate(gameMan.BasicPrefab, this.transform.position + Vector3.down * 1.5f, Quaternion.AngleAxis(angle, Vector3.up));
				if (GameManager.currentObjective == GameManager.Objectives.HarvestTheCrystals && !hasCrystalEnemySpawned && !gameMan.playerController.hasCrystal) {
					basicInstance.GetComponent<Basic>().isCrystallized = true;
					SkinnedMeshRenderer basicModel = basicInstance.transform.Find("Skeleton_Base_Model").GetComponent<SkinnedMeshRenderer>();
					basicModel.material = crystalizedMat;
					hasCrystalEnemySpawned = true;
					Debug.Log("Crystallized Skeleton should have spawned");
				}
			}
			yield return new WaitForSeconds(0.3f);
		}

		yield return new WaitForSeconds(despawnTime / 2);

		anim.SetBool("DeSpawn", true);
		yield return new WaitForSeconds(0.4f);

		GameObject.Destroy(this.gameObject);
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(transform.position, 10f);
	}
}
