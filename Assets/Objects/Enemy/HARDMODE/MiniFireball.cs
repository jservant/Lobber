using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniFireball : MonoBehaviour
{
    public GameManager gameMan;

    public Transform fire;
    private SphereCollider sc;

    public float damage;
    float shrinkTime = 0f;
    float minScale = 1f;
    float cooldownTime;

    public AK.Wwise.Event explosionSound;

    // Start is called before the first frame update
    void Start()
    {
        gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
        sc = GetComponent<SphereCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        if (shrinkTime > 0) {
            shrinkTime -= Time.deltaTime;
            var scale = Mathf.Lerp(minScale, 2f, shrinkTime / 1f);
            transform.localScale = new Vector3(scale, scale, scale);
        }
        else transform.localScale = new Vector3(minScale, minScale, minScale);

        if (cooldownTime > 0) {
            cooldownTime -= Time.deltaTime;
        }
        else {
            minScale = 1f;
        }

        if (shrinkTime <= 0 && minScale == 0) {
            fire.gameObject.SetActive(false);
            sc.enabled = false;
        }
        else {
            fire.gameObject.SetActive(true);
            sc.enabled = true;
        }
    }

    public void Grow() {
        explosionSound.Post(gameObject);
        gameMan.SpawnParticle(9, transform.position, 1f);
        minScale = 0f;
        cooldownTime = 8f;
        shrinkTime = 1f;
    }
}
