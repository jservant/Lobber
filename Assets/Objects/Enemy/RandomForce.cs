using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomForce : MonoBehaviour {
    public float ranForce;
    public float spinForce;
    public Rigidbody rb;

    // Start is called before the first frame update
    void Start() {
        var rf1 = Random.Range(-ranForce, ranForce);
        //var rf2 = Random.Range(-ranForce, ranForce);
        var rf3 = Random.Range(-ranForce, ranForce);

        rb.AddForce(rf1, ranForce * 2, rf3, ForceMode.Impulse);
    }

    // Update is called once per frame
    void Update() {
        transform.Rotate(spinForce * Time.deltaTime, 0f, 0f);
    }
}
