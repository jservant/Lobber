using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OrbSpawn : MonoBehaviour
{
    public bool spawnNow;
    public GameObject orbPrefab;

    void Update()
    {
        if (spawnNow == true)
        {
            spawnNow = false;
            Instantiate (orbPrefab, transform);
        }

    }

}
