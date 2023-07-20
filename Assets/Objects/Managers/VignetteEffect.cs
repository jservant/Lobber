using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class VignetteEffect : MonoBehaviour
{
    public Volume globalVolume;
    private Vignette vignette;
    public float fadeTime;
    private float currentFadeTime;
    private float startingIntensity;

    // Start is called before the first frame update
    void Start()
    {
        globalVolume.profile.TryGet<Vignette>(out vignette);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentFadeTime > 0) {
            currentFadeTime -= Time.deltaTime;
            vignette.intensity.value = Mathf.Lerp(0f, startingIntensity, currentFadeTime / fadeTime);
        }
        else {
            currentFadeTime = 0;
            fadeTime = 0;
            vignette.intensity.value = 0f;
        }
    }

    public void TriggerVignette(float intensity, float duration) {
        vignette.intensity.value = intensity;
        startingIntensity = intensity;
        if (fadeTime < duration) fadeTime = duration;
        currentFadeTime += duration;
        if (currentFadeTime > fadeTime) currentFadeTime = fadeTime;
    }
}
