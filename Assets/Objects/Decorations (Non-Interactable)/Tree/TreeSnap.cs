using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeSnap : MonoBehaviour
{
    public GameManager gameMan;
    public PlayerController player;

    public GameObject trunk;
    public GameObject leaves;
    private Rigidbody rb;
    public bool isChopped;

    public float lifeTime;
    public float shrinkTime;
    private float shrinkTimer;

    public TransparencyTrigger trigger;

    public Transform particlePoint;

    public AK.Wwise.Event[] chopSounds;
    
    // Start is called before the first frame update
    void Start()
    {
        gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
        player = transform.Find("/Player").GetComponent<PlayerController>();
        rb = trunk.GetComponent<Rigidbody>();
        trigger = GetComponentInChildren<TransparencyTrigger>();
        rb.isKinematic = true;
        isChopped = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isChopped) {
            lifeTime -= Time.deltaTime;
            if (lifeTime <= 0 && rb != null) {
                rb.transform.localScale *= Mathf.Lerp(0, 1, shrinkTime / shrinkTimer);
                shrinkTime -= Time.deltaTime;
            }

            if (shrinkTime <= 0) Destroy(trunk.gameObject);
        }
    }

    private void OnTriggerEnter(Collider other) {
        if (other.gameObject.layer == (int)Layers.PlayerHitbox || other.gameObject.layer == (int)Layers.EnemyHitbox || other.gameObject.layer == (int)Layers.AgnosticHitbox) {
            if (!isChopped) {
                if (other.gameObject.layer == (int)Layers.PlayerHitbox) Chop(true);
                else Chop(false);
            }
        }
    }

    void Chop(bool triggerEffects) {
        isChopped = true;
        rb.isKinematic = false;
        RandomForce();
        SetLifeTime();
        trunk.transform.parent = null;
        leaves.SetActive(false);

        trigger.isChopped = true;
        trigger.transparent = false;
        trigger.UpdateMaterial();
        trigger.enabled = false;

        gameMan.SpawnParticle(15, particlePoint.transform.position, 1f);
        gameMan.SpawnParticle(12, particlePoint.transform.position, 1.2f);
        chopSounds[0].Post(this.gameObject);

        if (triggerEffects) {
            gameMan.ShakeCamera(5f, 0.1f);
            if (GameObject.Find("HapticManager") != null) HapticManager.PlayEffect(player.hapticEffects[2], this.transform.position);
            chopSounds[1].Post(this.gameObject);
        }
    }

    void RandomForce() {
        var force = 10f;
        var torque = 10f;

        //force
        var rf1 = Random.Range(-force, force);
        var rf2 = Random.Range(3f, force);
        var rf3 = Random.Range(-force, force);

        rb.AddForce(rf1, rf2, rf3, ForceMode.Impulse);

        //torque
        var tf1 = Random.Range(-torque, torque);
        //var tf2 = Random.Range(-torque, torque);
        var tf3 = Random.Range(-torque, torque);

        rb.AddTorque(tf1, 0, tf3, ForceMode.Impulse);
    }

    void SetLifeTime() {
        lifeTime = Random.Range(lifeTime - 1f, lifeTime + 1f);
        shrinkTimer = shrinkTime;
    }
}
