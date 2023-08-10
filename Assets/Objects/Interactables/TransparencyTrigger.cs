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
    public MeshRenderer mesh2;
    public Material[] materials2;
    public bool transparent;

    public bool isChopped;

    // Start is called before the first frame update
    void Start()
    {
        transparent = false;
        isChopped = false;
    }

    // Update is called once per frame
    void Update()
    {
        UpdateMaterial();
    }

    private void OnTriggerEnter(Collider other) {
        
            if (other.gameObject.layer == (int)Layers.PlayerHurtbox) {
                if (!isChopped) transparent = true;
            }
        
    }

    private void OnTriggerExit(Collider other) {
        
            if (other.gameObject.layer == (int)Layers.PlayerHurtbox) {
                if (!isChopped) transparent = false;
            }
        
    }

    public void UpdateMaterial() {
        if (transparent) {
            mesh.material = materials[1];
            if (mesh2 != null) mesh2.material = materials2[1];
        }
        else {
            mesh.material = materials[0];
            if (mesh2 != null) mesh2.material = materials2[0];
        }
    }
}
