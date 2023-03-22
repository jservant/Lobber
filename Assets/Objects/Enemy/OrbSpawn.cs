using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbSpawn : MonoBehaviour {
    public float despawnTime;
    public GameObject enemy;
    public Animator anim;
    public Transform[] spawns;
    public bool spawnNow;
    GameManager gameMan;
    [SerializeField] GameObject orbPrefab;

    void Start() {
        spawns = transform.Find("Spawns").GetComponentsInChildren<Transform>();
        //orbPrefab = transform.Find("Orb"); // can't find properly despite being assigned in editor??? throwing errors even though it works
        anim = orbPrefab.GetComponent<Animator>();
        gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
    }

    void Update() {
        if (spawnNow == true) {
            spawnNow = false;
            StartCoroutine(Spawning(despawnTime));
        }
    }

    IEnumerator Spawning(float despawnTime) {
        orbPrefab.SetActive(true);
        if (gameMan.canSpawn == true) gameMan.canSpawn = false;
        yield return new WaitForSeconds(despawnTime / 2);
        for (int s = 0; s < spawns.Length; s++) {
            Instantiate(enemy, spawns[s].transform.position, spawns[s].transform.rotation);
        }
        yield return new WaitForSeconds(despawnTime / 2);
        anim.SetBool("DeSpawn", true);
        yield return new WaitForSeconds(0.5f);
        orbPrefab.SetActive(false);
        gameMan.canSpawn = true;
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.magenta;
        Gizmos.DrawWireSphere(transform.position, 5f);
    }
}
