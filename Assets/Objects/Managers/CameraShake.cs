using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class CameraShake : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;
    private CinemachineBasicMultiChannelPerlin noise;
    private float shakeTimer;
    public float shakeTime;
    private float startingIntensity;

    private void Awake() 
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        shakeTime = 0;
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (shakeTime > 0) {
            shakeTime -= Time.deltaTime;
            noise.m_AmplitudeGain = Mathf.Lerp(0f, startingIntensity, shakeTime / shakeTimer);
        }
        else {
            shakeTime = 0;
            noise.m_AmplitudeGain = 0f;
        }
    }

    public void _ShakeCamera(float intensity, float duration) {
        noise.m_AmplitudeGain = intensity;
        startingIntensity = intensity;
        shakeTimer = duration;
        shakeTime = duration;
    }
}
