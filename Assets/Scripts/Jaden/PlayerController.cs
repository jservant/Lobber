using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    CapsuleCollider capCol;
    DefaultPlayerActions pActions;
    //public PlayerInput pInput;

    //Vector3 playerVelocity;
    Vector2 mInput;
    float pSpeed = 2f;
    
    private void Awake()
    {
        capCol = GetComponent<CapsuleCollider>();
        //pInput = GetComponent<PlayerInput>();
        pActions = new DefaultPlayerActions();
    }

    private void OnEnable()
    {
        pActions.Player.Enable();
    }
    
    private void OnDisable()
    {
        pActions.Player.Disable();
    }

    private void Update()
    {
        mInput = pActions.Player.Move.ReadValue<Vector2>();
        Vector3 movement = new Vector3(mInput.x, 0, mInput.y);
        transform.Translate(movement * pSpeed * Time.deltaTime);
    }

/*    public void Move(InputAction.CallbackContext context)
    {
        //if (context.started) print("oog");
        Vector2 moveValue = context.ReadValue<Vector2>();
        transform.Translate(new Vector3(moveValue.x, 0, moveValue.y));
    }*/
}
