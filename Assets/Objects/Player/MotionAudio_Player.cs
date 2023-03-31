using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MotionAudio_Player : MonoBehaviour
{
    public AK.Wwise.Event Character_Attack1;

    void CharacterAttack1()
    {
        Character_Attack1.Post(gameObject);
    }

    public AK.Wwise.Event Character_Attack2;

    void CharacterAttack2()
    {
        Character_Attack2.Post(gameObject);
    }

    public AK.Wwise.Event Character_Chop;

    void CharacterChop()
    {
        Character_Chop.Post(gameObject);
    }

    public AK.Wwise.Event Character_Death;

    void CharacterDeath()
    {
        Character_Death.Post(gameObject);
    }

    public AK.Wwise.Event Character_GetHit;

    void CharacterGetHit()
    {
        Character_GetHit.Post(gameObject);
    }

    public AK.Wwise.Event Character_Idle;

    void CharacterIdle()
    {
        Character_Idle.Post(gameObject);
    }

    public AK.Wwise.Event Character_Lethal_Dash;

    void CharacterLethalDash()
    {
        Character_Lethal_Dash.Post(gameObject);
    }

    public AK.Wwise.Event Character_Lob_Throw;

    void CharacterLobThrow()
    {
        Character_Lob_Throw.Post(gameObject);
    }

    public AK.Wwise.Event Character_Roll;

    void CharacterRoll()
    {
        Character_Roll.Post(gameObject);
    }

    public AK.Wwise.Event Character_Run;

    void CharacterRun()
    {
        Character_Run.Post(gameObject);
    }

    public AK.Wwise.Event Character_Shotgun;

    void CharacterShotgun()
    {
        Character_Shotgun.Post(gameObject);
    }

    public AK.Wwise.Event Character_Slam;

    void CharacterSlam()
    {
        Character_Slam.Post(gameObject);
    }

    public AK.Wwise.Event Character_Spin;

    void CharacterSpin()
    {
        Character_Spin.Post(gameObject);
    }

    public AK.Wwise.Event Character_Attack3;

    void CharacterAttack3()
    {
        Character_Attack3.Post(gameObject);
    }
}
