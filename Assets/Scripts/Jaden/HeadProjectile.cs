using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadProjectile : MonoBehaviour
{
    [SerializeField] float speed = 25f;
    [SerializeField] int damage = 8;

    void Update()
    {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == 6) {
            other.GetComponent<Enemy>().ReceiveDamage(damage);
        }
    }
}
