using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public AK.Wwise.Event[] musicTracks;

    public static int currentTrack;
    public int playOnSceneStart;

    // Start is called before the first frame update
    void Start()
    {
        if (playOnSceneStart != 0) PlayMusic(playOnSceneStart);
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void PlayMusic(int id) {
        StopMusic();
        musicTracks[id].Post(gameObject);
    }

    public void StopMusic() {
        for (int i = 6; i < 10; i++) {
            musicTracks[i].Post(gameObject);
        }
    }
}
