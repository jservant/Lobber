using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    Transform player;

    void Start()
    {
        player = transform.Find("/GameManager").GetComponent<GameManager>().player;
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = new Vector3(player.position.x, player.position.y + 8, player.position.z - 10);
    }
}
