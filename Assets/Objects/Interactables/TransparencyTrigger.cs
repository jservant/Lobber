using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class TransparencyTrigger : MonoBehaviour
{
    public BoxCollider trigger;
    public MeshRenderer mesh;
    public Material[] materials;
    public bool transparent;

    // Start is called before the first frame update
    void Start()
    {
        transparent = false;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMaterial();
    }

    private void OnTriggerEnter(Collider other) {
        
            if (other.gameObject.layer == (int)Layers.PlayerHurtbox) {
                transparent = true;
            }
        
    }

    private void OnTriggerExit(Collider other) {
        
            if (other.gameObject.layer == (int)Layers.PlayerHurtbox) {
                transparent = false;
            }
        
    }

    public void UpdateMaterial() {
        if (transparent) {
            mesh.material = materials[1];
        }
        else mesh.material = materials[0];
    }
}
