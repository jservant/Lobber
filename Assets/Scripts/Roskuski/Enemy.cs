using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    enum AiState
    {
        Wait,
        Idle,
        Wander,
        Seek,
    }

    public const float WAIT_TIME = 2;

    AiState state;
    float waitTimer;

    // Start is called before the first frame update
    void Start()
    {
        state = AiState.Wander;
    }

    // Update is called once per frame
    void Update()
    {
        if (state == AiState.Wait) {
        }
        else if (state == AiState.Idle) {
        }
        else if (state == AiState.Wander) {
            
        }
        else if (state == AiState.Seek) {
        }
        else {
            // @TODO(Roskuski): can I get unity to panic?
        }
    }
}
