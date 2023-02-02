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
        transform.Translate(movement * pSpeed * Time.deltaTime); // maybe rb movement?
        //rb.velocity = movement * pSpeed * Time.deltaTime;
        transform.rotation = Quaternion.FromToRotation(Vector3.zero, movement);
        Debug.Log(Quaternion.LookRotation(rb.velocity, Vector3.up));
                             //Quaternion.LookRotation(rb.velocity, Vector3.up);
        // TODO(@Jaden): Rotate player on x axis relative to where player is moving

/*        //movement
        if (movement.magnitude >= 0.1)
        {
            float targetAngle = Mathf.Atan2(movement.x, movement.z) * Mathf.Rad2Deg + cam.eulerAngles.y;
            float angle = Mathf.SmoothDampAngle(transform.eulerAngles.y, targetAngle, ref turnVelocity, turnSpeed);
            transform.rotation = Quaternion.Euler(0f, angle, 0f);

            Vector3 moveDirection = Quaternion.Euler(0f, targetAngle, 0f) * Vector3.forward;
            pInput.Move(moveDirection.normalized * pSpeed * Time.deltaTime);
        }*/
    }

    public void Move(InputAction.CallbackContext context)
    {
        mInput = context.ReadValue<Vector2>();
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
