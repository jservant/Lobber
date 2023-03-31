using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionAudio_Skel : MonoBehaviour
{
    public AK.Wwise.Event Enemy_GetHit;

    void EnemyGetHit()
    {
        Enemy_GetHit.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_Attack_Dash;

    void EnemyDash()
    {
        Enemy_Attack_Dash.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_Attack_Slash;

    void EnemyAttackSlash()
    {
        Enemy_Attack_Slash.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_Run;

    void EnemyRun()
    {
        Enemy_Run.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_Spwan_Jump;

    void EnemySpwanJump()
    {
        Enemy_Spwan_Jump.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_Spwan_Land;

    void EnemySpwanLand()
    {
        Enemy_Spwan_Land.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_Stun;

    void EnemyStun()
    {
        Enemy_Stun.Post(gameObject);
    }

    public AK.Wwise.Event Enemy_VO;

    void EnemyVO()
    {
        Enemy_VO.Post(gameObject);
    }
}
