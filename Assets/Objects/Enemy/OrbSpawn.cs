using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbSpawn : MonoBehaviour {

	public int basicAmount;
	public int explodingAmount;
	public int necroAmount;

	float despawnTime = 3;
	bool[] spawnedEnemies;
	float angleOffset;

	public Animator anim;
	GameManager gameMan;
	public Material crystalizedMat;

	public bool isPlayerPortal;
	public bool isTutorialPortal;

	void Start() {
		anim = transform.Find("SpawnOrbV2").GetComponent<Animator>();
		gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
		if (!isPlayerPortal && !isTutorialPortal) StartCoroutine(Spawning());
		else if (isPlayerPortal) StartCoroutine(QuickPortal());
		
		Util.SpawnFlash(gameMan, 11, transform.position, true);
	}

	public IEnumerator Spawning() {
		int spawnAmount = basicAmount + explodingAmount;
		gameMan.enemiesAlive += spawnAmount;

		anim.SetBool("DeSpawn", false);
		spawnedEnemies = new bool[basicAmount + explodingAmount + necroAmount];
		angleOffset = Random.Range(0.0f, 90.0f);
		yield return new WaitForSeconds(despawnTime / 2);

		for (int spawnCount = 0; spawnCount < spawnAmount; spawnCount += 1) {
			// Make sure we spawn a enemy that "hasn't been spawned yet"
			int randomIndex = Random.Range(0, spawnedEnemies.Length);
			int redCheck = 0;
			if (gameMan.hardModeActive) {
				float _redChance = Random.Range(0, 99);
				if (_redChance < GameManager.redChance) {
					redCheck = 1;
                }
            }

			while (spawnedEnemies[randomIndex]) {
				randomIndex += 1;
				if (randomIndex >= spawnedEnemies.Length) { randomIndex = 0; }
			}
			spawnedEnemies[randomIndex] = true;

			float angle = (float)randomIndex * 360.0f / (float)spawnAmount;
			if (randomIndex >= basicAmount + explodingAmount) {
				Vector3 spawnPosition = Vector3.zero;
				// @TODO(Roskuski): We wouldn't want this to fail too many times in a row...
				for (int count = 0; count < 30; count += 1) {
					Vector3 testDirection = Quaternion.AngleAxis(Random.Range(0f, 360f), Vector3.up) * Vector3.forward;
					float testLateralDistance = Random.Range(0, 270f); // NOTE(Roskuski): the arena is roughly 250 units on the long side.
					RaycastHit lateralInfo;
					// @TODO(Roskuski): Make sure that the origin will be above any terrain/props.
					if (Physics.Raycast(this.transform.position + Vector3.up * 10f, testDirection, out lateralInfo, testLateralDistance)) {
						testLateralDistance = lateralInfo.distance;
					}

					if (!Physics.Raycast(this.transform.position + testDirection * testLateralDistance, Vector3.down, 50f)) {
						spawnPosition = this.transform.position + testDirection * testLateralDistance;
						spawnPosition.y = this.transform.position.y;
						break;
					}
				}

				Instantiate(gameMan.NecroPrefab[redCheck], spawnPosition, Quaternion.LookRotation(this.transform.position - spawnPosition, Vector3.up));
			}
			else if (randomIndex >= basicAmount) { 
				Instantiate(gameMan.ExplodingPrefab[redCheck], this.transform.position + Vector3.down * 1.5f, Quaternion.AngleAxis(angle, Vector3.up));
			}
			else {
				GameObject basicInstance = Instantiate(gameMan.BasicPrefab[redCheck], this.transform.position + Vector3.down * 1.5f, Quaternion.AngleAxis(angle, Vector3.up));
				if (GameManager.currentObjective == GameManager.Objectives.HarvestTheCrystals && !gameMan.isCrystalEnemyAlive) {
					gameMan.isCrystalEnemyAlive = true;
					var basicScript = basicInstance.GetComponent<Basic>();
					basicScript.isCrystallized = true;
					if (basicScript.isHardMode) { //Make sure it's easy to kill, even in hardmode
						basicScript.health -= 5;
						basicScript.isHardMode = false;
					}
					SkinnedMeshRenderer basicModel = basicInstance.transform.Find("Skeleton_Base_Model").GetComponent<SkinnedMeshRenderer>();
					basicModel.material = crystalizedMat;
					Debug.Log("Crystallized Skeleton should have spawned");
				}
			}
			yield return new WaitForSeconds(0.3f);
		}

		yield return new WaitForSeconds(despawnTime / 2);

		anim.SetBool("DeSpawn", true);
		yield return new WaitForSeconds(0.4f);

		gameMan.SpawnParticle(12, transform.position, 1.5f);
		Util.SpawnFlash(gameMan, 11, transform.position, false);
		GameObject.Destroy(this.gameObject);
	}

	public IEnumerator QuickPortal() {
		anim.SetBool("DeSpawn", false);
		yield return new WaitForSeconds(0.6f);
		anim.SetBool("DeSpawn", true);
		yield return new WaitForSeconds(0.4f);

		gameMan.SpawnParticle(12, transform.position, 1.5f);
		Util.SpawnFlash(gameMan, 11, transform.position, false);
		GameObject.Destroy(this.gameObject);
	}

	void OnDrawGizmos() {
		Gizmos.color = Color.magenta;
		Gizmos.DrawWireSphere(transform.position, 10f);
	}

    private void OnDestroy() {
		
    }
}
