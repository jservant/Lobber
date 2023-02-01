using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public PlayerController player;

    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        if (player != null) {
            Debug.Log("PlayerController found.");
        } else Debug.Log("PlayerController has not been found.");
    }
}
