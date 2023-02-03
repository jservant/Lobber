using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    CapsuleCollider capCol;
    BoxCollider hitbox;
    Rigidbody rb;
    Animator animr;
    bool animBuffer = false;
    MeshRenderer headMesh;

    Vector2 mInput;
    [SerializeField] float pSpeed = 2f;
    [SerializeField] int damage = 5;

    private void Awake()
    {
        capCol = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        animr = GetComponent<Animator>();
        hitbox = transform.Find("Axe_Controller/AxeHitbox").GetComponent<BoxCollider>();
        headMesh = transform.Find("Axe_Controller/AxeHitbox/Sphere").GetComponent<MeshRenderer>();
        if (headMesh != null) { Debug.Log("Axe headmesh found on player."); }
        else { Debug.LogWarning("Axe headmesh not found on player."); }
        if (hitbox != null) { Debug.Log("Axe hitbox found on player."); }
        else { Debug.LogWarning("Axe hitbox not found on player."); }
    }

    private void Update()
    {
        Vector3 movement = new Vector3(mInput.x, 0, mInput.y).normalized;
        rb.MovePosition(transform.position + movement * Time.deltaTime * pSpeed);

    }

    #region Player inputs
    public void Move(InputAction.CallbackContext context)
    {
        mInput = context.ReadValue<Vector2>();
        if (context.performed == true) { transform.rotation = Quaternion.LookRotation(new Vector3(mInput.x, 0, mInput.y)); }
        //Debug.Log("Move activated, current value: " + mInput);
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (animBuffer == false) StartCoroutine(AnimBuffer("attack", .73f, true));
    }

    public void Lob(InputAction.CallbackContext context)
    {
        if (animBuffer == false) {
            if (headMesh.enabled == true) { StartCoroutine(AnimBuffer("lobThrow", .7f, true)); headMesh.enabled = false; }
            else StartCoroutine(AnimBuffer("lob", .73f, true));
        }
    }
    #endregion

    private void OnTriggerEnter(Collider other) // trigger SHOULD be axe hitbox
    {
        if (other.gameObject.layer == 6 && animr.GetBool("lob") == false) {//Enemy
            other.gameObject.GetComponent<Enemy>().ReceiveDamage(damage);
        }
        if (other.gameObject.layer == 6 && animr.GetBool("lob") == true) {
            //todo: enemy instantly dies
            Debug.Log("Lob landed");
            headMesh.enabled = true;
        }
    }

    //TODO(@Jaden): Add OnTriggerEnter to make axe hitbox work, remember to do hitstun on enemy
    // so it doesn't melt their health

    #region Minor utility functions
    IEnumerator AnimBuffer(string animName, float duration, bool offWhenDone)
    {
        animr.SetBool(animName, true);
        animBuffer = true;
        yield return new WaitForSeconds(duration);
        animBuffer = false;
        if (offWhenDone) { animr.SetBool(animName, false); }
    }
    #endregion
}
