using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Platform : MonoBehaviour
{
    public float hangTime; //time before the platform breaks
    private float currentTime = 0f;
    public GameObject[] meshes;

    public bool isTriggered;
    private bool canTrigger = true;
    private bool meshHidden;

    private float meshHideTime = 0.2f;
    private float meshHideCooldown = 0.6f;
    private float meshCdInterval = 0.6f;

    public float respawnTime; //time it takes for platform to respawn

    // Start is called before the first frame update
    void Start()
    {
        isTriggered = false;
        meshHidden = false;
    }

    // Update is called once per frame
    void Update()
    {
        if (isTriggered) {
            Countdown();
        }
    }

    private void OnTriggerStay(Collider other) {
        if (other.gameObject.layer == (int)Layers.PlayerHurtbox) {
            if (canTrigger) isTriggered = true;
        }
    }

    void Countdown() {
        if (currentTime < hangTime) {
            currentTime += Time.deltaTime;

            if (currentTime >= meshHideCooldown) {
                StartCoroutine(HideMesh(meshHideTime));
                meshHideCooldown += meshCdInterval;
                meshCdInterval -= 0.1f;
                if (meshCdInterval <= 0.3f) meshCdInterval = 0.3f;
            }
        }
        else {
            StartCoroutine(Respawn(respawnTime));
            canTrigger = false;
            isTriggered = false;
            meshHideCooldown = 0.6f;
            meshCdInterval = 0.6f;
            currentTime = 0;
        }
    }

    IEnumerator HideMesh(float duration) {
        for (int m = 0; m < meshes.Length; m++) {
            meshes[m].GetComponent<MeshRenderer>().enabled = false;
        }
        yield return new WaitForSeconds(duration);
        for (int m = 0; m < meshes.Length; m++) {
            meshes[m].GetComponent<MeshRenderer>().enabled = true;
        }
        yield break;
    }

    IEnumerator Respawn(float duration) {
        for (int m = 0; m < meshes.Length; m++) {
            meshes[m].SetActive(false);
        }
        yield return new WaitForSeconds(duration);
        canTrigger = true;
        for (int m = 0; m < meshes.Length; m++) {
            meshes[m].SetActive(true);
        }
        yield break;
    }
}
