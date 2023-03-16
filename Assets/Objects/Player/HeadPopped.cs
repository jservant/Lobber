using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadPopped : MonoBehaviour {
    private Rigidbody rb;
    public float lifetime;

    // Start is called before the first frame update
    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update() {
        if (lifetime >= 0) {
            lifetime -= Time.deltaTime;
        }
        else DestroySkull();

        if (transform.position.y <= 0) {
            DestroySkull();
        }
    }

    void DestroySkull() {
        Destroy(this.transform.parent.gameObject);
    }
}
