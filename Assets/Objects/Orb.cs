using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    public float despawnTime;
    public GameObject enemy;
    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();

        StartCoroutine(TimeToDie());
    }

    IEnumerator TimeToDie()
    {
        yield return new WaitForSeconds(despawnTime / 2);
        Instantiate(enemy, transform.position, transform.rotation);
        yield return new WaitForSeconds(despawnTime / 2);
        anim.SetBool("DeSpawn", true);
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
