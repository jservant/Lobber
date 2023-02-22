using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Orb : MonoBehaviour
{
    public int despawnTime;

    public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();

        StartCoroutine(TimeToDie());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator TimeToDie()
    {
        yield return new WaitForSeconds(despawnTime);
        anim.SetBool("DeSpawn", true);
        yield return new WaitForSeconds(0.5f);
        Destroy(gameObject);
    }
}
