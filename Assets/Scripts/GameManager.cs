using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public Transform player;
    PlayerInput pInput;

    void Start() {
        player = transform.Find("/Player");
        if (player != null) {
            Debug.Log("Object Named Player found");
        } else Debug.LogWarning("Object Named Player Not found");
    }

    public void Restart(InputAction.CallbackContext context) {
        Debug.Log("Restart called");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
}
