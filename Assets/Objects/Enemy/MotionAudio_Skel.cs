using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionAudio_Skel : MonoBehaviour
{
    public AK.Wwise.Event Enemy_GetHit;
    
    public void CharacterGetHit()
    {
        Enemy_GetHit.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_ArmorHit;

    public void Sound_ArmorHit() {
        Enemy_ArmorHit.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_ArmorBreak;

    public void Sound_ArmorBreak() {
        Enemy_ArmorBreak.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_Attack_Dash;

    void Sound_EnemyDash()
    {
        Enemy_Attack_Dash.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_Attack_Slash;

    void Sound_EnemyAttackSlash()
    {
        Enemy_Attack_Slash.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_Run;

    void Sound_EnemyRun()
    {
        Enemy_Run.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_Spwan_Jump;

    void Sound_EnemySpwanJump()
    {
        Enemy_Spwan_Jump.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_Spwan_Land;

    void Sound_EnemySpwanLand()
    {
        Enemy_Spwan_Land.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_Stun;

    public void Sound_EnemyStun()
    {
        Enemy_Stun.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_Lob;

    public void Sound_EnemyLob() 
    {
        Enemy_Lob.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_VO;

    public void Sound_EnemyVO()
    {
        Enemy_VO.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_CrystalShatter;

    public void Sound_EnemyCrystalShatter() {
        Enemy_CrystalShatter.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_Sliced;

    public void Sound_EnemySliced() {
        Enemy_Sliced.Post(gameObject);
    }
}
