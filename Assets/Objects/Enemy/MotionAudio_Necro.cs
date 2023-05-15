using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionAudio_Necro : MonoBehaviour
{
    public AK.Wwise.Event NecroGetHit;

    public void Necro_GetHit() {
        NecroGetHit.Post(gameObject);
    }

    public AK.Wwise.Event NecroSpawn;

    public void Necro_Spawn() {
        NecroSpawn.Post(gameObject);
    }

    public AK.Wwise.Event NecroIdle;

    public void Necro_Idle() {
        NecroIdle.Post(gameObject);
    }

    public AK.Wwise.Event NecroFireball;

    public void Necro_Fireball() {
        NecroFireball.Post(gameObject);
    }

    public AK.Wwise.Event NecroDeath;

    public void Necro_Death() {
        NecroDeath.Post(gameObject);
    }
}
