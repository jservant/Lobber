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
    public float currentFOV;
    private float targetFOV;
    public float zoomInFOV;
    public float zoomOutFOV;
    private float zoomTime;

    private void Awake() 
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        noise = vcam.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        shakeTime = 0;
        currentFOV = vcam.m_Lens.FieldOfView;
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

        if (zoomTime > 0) {
            zoomTime -= Time.deltaTime;
            vcam.m_Lens.FieldOfView = Mathf.Lerp(targetFOV, currentFOV, zoomTime / 0.2f);
        }
        else currentFOV = currentFOV = vcam.m_Lens.FieldOfView;
    }

    public void _ShakeCamera(float intensity, float duration) {
        noise.m_AmplitudeGain = intensity;
        startingIntensity = intensity;
        shakeTimer = duration;
        shakeTime = duration;
    }

    public void _CameraZoom(float sign) {
        if (zoomTime <= 0) {
            targetFOV = currentFOV + (sign * 12f);
            if (targetFOV < zoomInFOV) {
                targetFOV = zoomInFOV;
            }

            if (targetFOV > zoomOutFOV) {
                targetFOV = zoomOutFOV;
            }
            zoomTime = 0.2f;
        }
    }
}
