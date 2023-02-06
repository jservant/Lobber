using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadProjectile : MonoBehaviour
{
    [SerializeField] float speed = 25f;

    // Update is called once per frame
    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }
}
