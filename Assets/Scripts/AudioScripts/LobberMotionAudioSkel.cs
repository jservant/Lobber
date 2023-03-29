using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobberMotionAudioSkel : MonoBehaviour
{
	public AK.Wwise.Event PlaySkelFS;
	public AK.Wwise.Event PlaySkelDash;
    public AK.Wwise.Event PlaySkelSlash;
    public AK.Wwise.Event PlaySkelGetHit;
    public AK.Wwise.Event PlaySkelDeath;
    public AK.Wwise.Event PlaySkelJump;
    public AK.Wwise.Event PlaySkelLand;
    public AK.Wwise.Event PlaySkelVO;


    void SkelFS()
	{
		PlaySkelFS.Post(gameObject);
	}

	void SkelDash()
	{
		PlaySkelDash.Post(gameObject);
	}

    void SkelSlash ()
    {
        PlaySkelSlash.Post(gameObject);
    }

    void SkelGetHit()
    {
        PlaySkelGetHit.Post(gameObject);
    }

    void SkelDeath()
    {
        PlaySkelDeath.Post(gameObject);
    }

    void SkelJump()
    {
        PlaySkelJump.Post(gameObject);
    }

    void SkelLand()
    {
        PlaySkelLand.Post(gameObject);
    }

    void SkelVO()
    {
        PlaySkelVO.Post(gameObject);
    }


}