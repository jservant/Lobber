using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobberMotionAudioSkel : MonoBehaviour
{
	public AK.Wwise.Event PlaySkelFS;
	public AK.Wwise.Event PlaySkelAttack;
    public AK.Wwise.Event PlaySkelGetHit;
    public AK.Wwise.Event PlaySkelJump;
    public AK.Wwise.Event PlaySkelLand;

    void SkelFS()
	{
		PlaySkelFS.Post(gameObject);
	}

	void SkelAttack()
	{
		PlaySkelAttack.Post(gameObject);
	}

    void SkelGetHit()
    {
        PlaySkelGetHit.Post(gameObject);
    }

    void SkelJump()
    {
        PlaySkelJump.Post(gameObject);
    }

    void SkelLand()
    {
        PlaySkelLand.Post(gameObject);
    }

}