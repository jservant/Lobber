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

    private void Start() {
        slider = GetComponent<Slider>();
    }

    public void SetVolume(string volumeType) {
        float sliderValue = slider.value;

        //if (volumeType == "Master") {
        if (volumeType == "Master") {
            masterVolume = sliderValue;
            AkSoundEngine.SetRTPCValue("MasterVolume", masterVolume);
            Initializer.save.versionLatest.masterVolume = masterVolume;
        }

        if (volumeType == "Music") {
            musicVolume = sliderValue;
            AkSoundEngine.SetRTPCValue("MusicVolume", musicVolume);
            Initializer.save.versionLatest.musicVolume = musicVolume;
        }

        if (volumeType == "Sound") {
            soundVolume = sliderValue;
            AkSoundEngine.SetRTPCValue("SFXVolume", soundVolume);
            Initializer.save.versionLatest.sfxVolume = soundVolume;
        }
    }
}
