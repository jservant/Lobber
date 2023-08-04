using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public AK.Wwise.Event[] musicTracks;
    /* 0: None
     * 1: Tutorial Theme
     * 2: Intro Combat Theme
     * 3: Core Combat Theme
     * 4: Desert Theme
     * 5: Crystal Theme
     * 6-10: Stops*/

    public static int currentTrack;
    public int _currentTrack;
    public int currentScene;
    
    public int playOnSceneStart;

    // Start is called before the first frame update
    void Start()
    {
        if (playOnSceneStart != 0 && playOnSceneStart != currentTrack) PlayMusic(playOnSceneStart);
        currentScene = SceneManager.GetActiveScene().buildIndex;
        if (currentScene != 0 && currentTrack == 1) { PlayMusic(2); }
    }

    // Update is called once per frame
    void Update()
    {
        _currentTrack = currentTrack;
    }

    public void PlayMusic(int id) {
        StopMusic();
        musicTracks[id].Post(GameObject.Find("/MusicManager"), (uint)AkCallbackType.AK_MusicSyncExit, CheckTracks);
        currentTrack = id;
    }

    public void StopMusic() {
        for (int i = 6; i < musicTracks.Length; i++) {
            musicTracks[i].Post(GameObject.Find("/MusicManager"));
        }
        currentTrack = 0;
    }

    void CheckTracks(object in_cookie, AkCallbackType in_type, object in_info) {
        var scene = GameObject.Find("/MusicManager").GetComponent<MusicManager>().currentScene;
        Debug.Log("Playing music for Scene: " + scene);
        if (scene == 1 || scene == 2) { //Grass
            PlayMusic(3);
        }

        if (scene == 3 || scene == 4) { //Crystal
            if (currentTrack != 5) PlayMusic(5);
            else PlayMusic(3);
        }

        if (scene == 5 || scene == 6) { //Desert
            if (currentTrack != 4) PlayMusic(4);
            else PlayMusic(3);
        }
    }
}
