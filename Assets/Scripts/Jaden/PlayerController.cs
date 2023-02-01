using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    CapsuleCollider capCol;
    public PlayerInput pInput;

    Vector2 mInput;
    [SerializeField] float pSpeed = 2f;
    
    private void Awake()
    {
        capCol = GetComponent<CapsuleCollider>();
        pInput = GetComponent<PlayerInput>();
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
}