using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public Transform player;

    void Start()
    {
        player = transform.Find("/Player");
        if (player != null) {
            Debug.Log("Object Named Player found");
        } else Debug.Log("Object Named Player Not found");
    }
}
