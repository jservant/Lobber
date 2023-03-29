using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobberMotionAudio : MonoBehaviour
{
	public AK.Wwise.Event PlayFS;
	public AK.Wwise.Event PlayAttack1;
    public AK.Wwise.Event PlayAttack2;
    public AK.Wwise.Event PlayGetHit;
    public AK.Wwise.Event PlayChop;
    public AK.Wwise.Event PlayLob;

    void FS()
	{
		PlayFS.Post(gameObject);
	}

	void Attack1()
	{
		PlayAttack1.Post(gameObject);
	}

    void Attack2()
    {
        PlayAttack2.Post(gameObject);
    }

    void GetHit()
    {
        PlayGetHit.Post(gameObject);
    }

    void Chop()
    {
        PlayChop.Post(gameObject);
    }

    void Lob()
    {
        PlayLob.Post(gameObject);
    }
}