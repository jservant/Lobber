using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Sliders : MonoBehaviour
{
    public Slider slider;
    public float masterVolume;
    public float musicVolume;
    public float soundVolume;
    public float screenshakeAmount;

    /*private void Start() {
        slider = gameObject.GetComponent<Slider>();
    }*/

    public void SetSlider(string sliderType) {
        float sliderValue = slider.value;

        //if (volumeType == "Master") {
        if (sliderType == "Master") {
            masterVolume = sliderValue;
            AkSoundEngine.SetRTPCValue("MasterVolume", masterVolume);
            Initializer.save.versionLatest.masterVolume = masterVolume;
        }

        if (sliderType == "Music") {
            musicVolume = sliderValue;
            AkSoundEngine.SetRTPCValue("MusicVolume", musicVolume);
            Initializer.save.versionLatest.musicVolume = musicVolume;
        }

        if (sliderType == "Sound") {
            soundVolume = sliderValue;
            AkSoundEngine.SetRTPCValue("SFXVolume", soundVolume);
            Initializer.save.versionLatest.sfxVolume = soundVolume;
        }

        if (sliderType == "Screenshake") {
            screenshakeAmount = sliderValue;
            Initializer.save.versionLatest.screenshakePercentage = screenshakeAmount;
        }
    }
}
