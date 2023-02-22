using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbSpawn : MonoBehaviour
{

    public bool spawnNow;
    public GameObject orbPrefab;

    // Start is called before the first frame update
    void Start()
    {

    }

    void Update()
    {
        if (spawnNow == true)
        {
            spawnNow = false;
            Instantiate (orbPrefab, transform);
        }

    }

}
