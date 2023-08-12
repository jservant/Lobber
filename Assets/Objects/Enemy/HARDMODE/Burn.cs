using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Burn : MonoBehaviour
{
    GameManager gameMan;

    public float tickRate; //how often to trigger damage hitbox
    private float tickRateMax;
    public float framesActive; //how many frames to check for damage
    float framesActiveMax;
    float gravity = 4.9f;

    public float lifeTime;

    public MeshCollider mesh;

    public AK.Wwise.Event fireSound;
    public AK.Wwise.Event fireStop;

    // Start is called before the first frame update
    void Start()
    {
        gameMan = transform.Find("/GameManager").GetComponent<GameManager>();

        tickRateMax = tickRate;
        framesActiveMax = framesActive;
        framesActive = 0;
        mesh.enabled = false;
        fireSound.Post(gameObject);
    }

    // Update is called once per frame
    void Update()
    {
        if (gameMan.transitioningLevel) {
            Destroy(transform.parent.gameObject);
        }

        if (framesActive > 0) {
            framesActive -= 1;
        }
        else mesh.enabled = false;

        if (tickRate > 0) {
            tickRate -= Time.deltaTime;
        }
        else {
            TriggerBurn();
            tickRate = tickRateMax;
        }

        if (lifeTime > 0) {
            lifeTime -= Time.deltaTime;
        }
        else {
            Destroy(transform.parent.gameObject);
        }

        if (!Physics.Raycast(transform.position, Vector3.down, 0.6f)) {
            transform.parent.position += Vector3.down * gravity * Time.deltaTime;
        }
    }

    void TriggerBurn() {
        framesActive = framesActiveMax;
        mesh.enabled = true;
    }

    private void OnDestroy() {
        fireStop.Post(gameObject);
    }

    private void OnDrawGizmos() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, 8);
    }
}
