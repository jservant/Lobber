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
        //transform.position = new Vector3(player.position.x, player.position.y + 8 * 2.5f, player.position.z - 10 * 2.5f);
        //transform.LookAt(player.position);
        //transform.Translate(Vector3.right * Time.deltaTime);
    }
}
