using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomForce : MonoBehaviour
{
    public float ranForce;
    public Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        var rf1 = Random.Range(-ranForce, ranForce);
        var rf2 = Random.Range(-ranForce, ranForce);
        var rf3 = Random.Range(-ranForce, ranForce);
        rb.AddForce(0f, Mathf.Abs(rf1), 0f, ForceMode.Impulse);
        

    }

    // Update is called once per frame
    void Update()
    {
        rb.AddTorque(transform.up * ranForce, ForceMode.Impulse);
    }
}
