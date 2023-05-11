using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionAudio_Pest : MonoBehaviour
{
    public AK.Wwise.Event Pest_FS;
    
    public void PestFS()
    {
        Pest_FS.Post(gameObject);
    }

    public AK.Wwise.Event Pest_Bomb_Expload;

    public void PestBombExplode()
    {
        Pest_Bomb_Expload.Post(gameObject);
    }

    public AK.Wwise.Event Pest_Bomb_Fuse;

    public void PestBombFuse()
    {
        Pest_Bomb_Fuse.Post(gameObject);
    }

    public AK.Wwise.Event Pest_Bomb_Jump;

    void PestBombJump()
    {
        Pest_Bomb_Jump.Post(gameObject);
    }

    public AK.Wwise.Event Pest_Bomb_Hit;

    public void PestBombHit() {
        Pest_Bomb_Hit.Post(gameObject);
    }
}
