using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ChangeVolume : MonoBehaviour
{
    public Slider slider;
    public float masterVolume;
    public float musicVolume;
    public float soundVolume;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void SetVolume(string volumeType) {
        float sliderValue = slider.value;

        if (volumeType == "Master") {
            masterVolume = sliderValue;
            AkSoundEngine.SetRTPCValue("MasterVolume", masterVolume);
        }

        if (volumeType == "Music") {
            musicVolume = sliderValue;
            AkSoundEngine.SetRTPCValue("MusicVolume", musicVolume);
        }

        if (volumeType == "Sound") {
            soundVolume = sliderValue;
            AkSoundEngine.SetRTPCValue("SFXVolume", soundVolume);
        }
    }
}
