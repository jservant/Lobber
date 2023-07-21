using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunSphere : MonoBehaviour
{
    public float stunSphereRadius = 3f;
    GetKnockbackInfo getKnockbackInfo;
    public float framesActive;

    // Start is called before the first frame update
    void Start()
    {
        framesActive = framesActive / 60;
    }

    // Update is called once per frame
    void Update()
    {
        if (framesActive > 0) {
            framesActive -= Time.deltaTime;
        }
        else Destroy(gameObject);
    }

    void OnDrawGizmos() {
        Gizmos.color = Color.white;
        Gizmos.DrawWireSphere(transform.position, stunSphereRadius);
        Gizmos.DrawWireSphere(transform.position, 1f);
    }
}
