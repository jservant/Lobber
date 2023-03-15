using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LobberMotionAudio : MonoBehaviour
{
	public AK.Wwise.Event PlayFS;
	public AK.Wwise.Event PlayAttack;
    public AK.Wwise.Event PlayGetHit;

    void FS()
	{
		PlayFS.Post(gameObject);
	}

	void Attack()
	{
		PlayAttack.Post(gameObject);
	}

    void GetHit()
    {
        PlayGetHit.Post(gameObject);
    }

}