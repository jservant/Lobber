using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    CapsuleCollider capCol;
    BoxCollider hitbox;
    Rigidbody rb;
    public PlayerInput pInput;
    Animator animr;
    bool animBuffer = false;

    Vector2 mInput;
    [SerializeField] float pSpeed = 2f;

    private void Awake()
    {
        capCol = GetComponent<CapsuleCollider>();
        rb = GetComponent<Rigidbody>();
        pInput = GetComponent<PlayerInput>();
        animr = GetComponent<Animator>();
    }

    private void Update()
    {
        Vector3 movement = new Vector3(mInput.x, 0, mInput.y).normalized;
        rb.MovePosition(transform.position + movement * Time.deltaTime * pSpeed);

    }

    public void Move(InputAction.CallbackContext context)
    {
        mInput = context.ReadValue<Vector2>();
        if (context.performed == true) { transform.rotation = Quaternion.LookRotation(new Vector3(mInput.x, 0, mInput.y)); }
        //Debug.Log("Move activated, current value: " + mInput);
    }

    public void Attack(InputAction.CallbackContext context)
    {
        if (animBuffer == false) StartCoroutine(AnimBuffer("isAttacking", .73f, true));
    }

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
