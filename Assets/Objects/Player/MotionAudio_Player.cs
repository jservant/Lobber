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

    public AK.Wwise.Event Character_Chop_Init;

    void CharacterChopInit()
    {
        Character_Chop_Init.Post(gameObject);
    }

    public AK.Wwise.Event Character_Chop_Tail;

    void CharacterChopTail()
    {
        Character_Chop_Tail.Post(gameObject);
    }

    public AK.Wwise.Event Character_Death;

    void CharacterDeath()
    {
        Character_Death.Post(gameObject);
    }

    public AK.Wwise.Event Character_GetHit;

    public void CharacterGetHit()
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

    public AK.Wwise.Event Character_Shotgun_Init;

    public void CharacterShotgunInit()
    {
        Character_Shotgun_Init.Post(gameObject);
    }

    public AK.Wwise.Event Character_Shotgun_Atk;

    public void CharacterShotgunAtk()
    {
        Character_Shotgun_Atk.Post(gameObject);
    }

    public AK.Wwise.Event Character_Slam_Init;

    void CharacterSlamInit()
    {
        Character_Slam_Init.Post(gameObject);
    }

    public AK.Wwise.Event Character_Slam_Atk;

    void CharacterSlamAtk()
    {
        Character_Slam_Atk.Post(gameObject);
    }

    public AK.Wwise.Event Character_Spin;

    public void CharacterSpin()
    {
        Character_Spin.Post(gameObject);
    }

    public AK.Wwise.Event Character_Attack3;

    void CharacterAttack3()
    {
        Character_Attack3.Post(gameObject);
    }

    public AK.Wwise.Event HeadPickup;

    public void Sound_HeadPickup() 
    {
        HeadPickup.Post(gameObject);
    }

    public AK.Wwise.Event HealthPickup;

    public void Sound_HealthPickup() {
        HealthPickup.Post(gameObject);
    }

    public AK.Wwise.Event CrystalPickup;

    public void Sound_CrystalPickup() {
        CrystalPickup.Post(gameObject);
    }

    public AK.Wwise.Event CrystalDrop;

    public void Sound_CrystalDrop() {
        CrystalDrop.Post(gameObject);
    }
}
