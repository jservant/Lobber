using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    CapsuleCollider capCol;
    public PlayerInput pInput;
    Animator animr;
    bool animBuffer = false;

    Vector2 mInput;
    [SerializeField] float pSpeed = 2f;

    private void Awake()
    {
        capCol = GetComponent<CapsuleCollider>();
        pInput = GetComponent<PlayerInput>();
        animr = GetComponent<Animator>();
    }

    private void Update()
    {
        Vector3 movement = new Vector3(mInput.x, 0, mInput.y);
        transform.Translate(movement * pSpeed * Time.deltaTime); // maybe rb movement?
    }

    public void Move(InputAction.CallbackContext context)
    {
        mInput = context.ReadValue<Vector2>();
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