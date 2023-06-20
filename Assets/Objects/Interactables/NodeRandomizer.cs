using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NodeRandomizer : MonoBehaviour
{
    public GameObject[] prop;
    public bool spawnProp;

    void Start()
    {
        DisableAll();
        if (spawnProp) PickObject();
    }
    public void DisableAll() {
        foreach (GameObject g in prop) {
            if (g != null) g.SetActive(false);
        }
    }

    public void PickObject() {
        int random = 0;
        random = Random.Range(0, prop.Length);
        if (prop[random] != null) prop[random].SetActive(true);
    }
}
