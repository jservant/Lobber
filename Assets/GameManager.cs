using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    PlayerController player;

    // Start is called before the first frame update
    void Start()
    {
        player = FindObjectOfType<PlayerController>();
        if (player != null)
        {
            Debug.Log("PlayerController found.");
        }
        else Debug.Log("PlayerController has not been found.");
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
