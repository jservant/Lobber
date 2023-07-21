using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteEffect : MonoBehaviour
{
    private PlayerController player;
    public Volume globalVolume;
    public float lowHpThreshold; //when Lobber's hp drops below this value, show the vignette
    public float criticalHpThreshold; //when to start flashing red
    private Vignette vignette;
    public float fadeTime;
    private float currentFadeTime;
    private float startingIntensity;
    private float minimumIntensity;
    private float counter;

    // Start is called before the first frame update
    void Start()
    {
        player = transform.Find("/Player").GetComponent<PlayerController>();

        globalVolume.profile.TryGet<Vignette>(out vignette);

        minimumIntensity = 0;
        counter = 0;
    }

    // Update is called once per frame
    void Update()
    {
        if (currentFadeTime > 0) {
            currentFadeTime -= Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(minimumIntensity, startingIntensity, currentFadeTime / fadeTime);
        }
        else {
            currentFadeTime = 0;
            fadeTime = 0;
            vignette.intensity.value = minimumIntensity;
        }

        if (player.health < lowHpThreshold) {
            minimumIntensity = 0.4f;
            if (player.health < criticalHpThreshold) {
                minimumIntensity = 0.6f;
            }
        }
        else minimumIntensity = 0f;

        /*if (player.health < criticalHpThreshold) { //flashing effect
            if (counter > 0) {
                counter -= Time.deltaTime;
            }
            else {
                counter = 1f;
                TriggerVignette(0.6f, 1f);
            }
        }
        else counter = 0;*/

        if (player.health == 0) vignette.intensity.value = 1.0f;
    }

    public void TriggerVignette(float intensity, float duration) {
        vignette.intensity.value = intensity;
        startingIntensity = intensity;
        if (fadeTime < duration) fadeTime = duration;
        currentFadeTime += duration;
        if (currentFadeTime > fadeTime) currentFadeTime = fadeTime;
    }
}
