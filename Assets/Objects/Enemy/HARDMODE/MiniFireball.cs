using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniFireball : MonoBehaviour
{
    public GameManager gameMan;

    public float damage;
    float shrinkTime = 0f;
    float minScale = 1f;

    public AK.Wwise.Event explosionSound;

    // Start is called before the first frame update
    void Start()
    {
        gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
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
    }

    public void Grow() {
        explosionSound.Post(gameObject);
        gameMan.SpawnParticle(9, transform.position, 1f);
        shrinkTime = 1f;
    }
}
