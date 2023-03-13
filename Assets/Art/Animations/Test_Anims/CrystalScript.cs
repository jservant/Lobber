using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrystalScript : MonoBehaviour
{
    public GameObject player;

    public Material shader1;
    public Material shader2;
    public Material shader3;
    public Material transparent;
    Renderer rend;

    public Material current_mat;

    public float duration = 2.0f;

    // Start is called before the first frame update
    void Start()
    {
        player = GameObject.Find("Player");
        rend = GetComponent<Renderer>();

        current_mat = GetComponent<Renderer>().material;
        //shader1 = Material.Find("Crystal");
       // shader2 = Material.Find("Crystal2");
        //shader3 = Material.Find("Crystal3");
    }

    // Update is called once per frame
    void Update()
    {
    
    }

    private void OnTriggerEnter(Collider player)
    {
        float lerp = Mathf.PingPong(Time.time, duration) / duration;
        GetComponent<Renderer>().material = transparent;
        rend.material.Lerp(shader2, transparent, duration);
    }

    private void OnTriggerExit(Collider player)
    {
        GetComponent<Renderer>().material = current_mat;
    }


    //void OnCollisionExit(Collision player)
    //{
    //    rend.material.shader = shader2;
    //}
}
