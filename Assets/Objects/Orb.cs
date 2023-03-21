using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    public float despawnTime;
    public GameObject enemy;
    public Animator anim;
    public Transform[] spawns;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        spawns = transform.Find("Spawns").GetComponentsInChildren<Transform>();

        StartCoroutine(Spawning(despawnTime));
    }

    IEnumerator Spawning(float despawnTime)
    {
        yield return new WaitForSeconds(despawnTime / 2);
        for (int s = 0; s < spawns.Length; s++) {
            Instantiate(enemy, spawns[s].transform.position, spawns[s].transform.rotation);
        }
        yield return new WaitForSeconds(despawnTime / 2);
        anim.SetBool("DeSpawn", true);
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
