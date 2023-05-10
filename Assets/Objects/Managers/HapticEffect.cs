using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Haptic Effect", fileName = "HapticEffect")]
public class HapticEffect : ScriptableObject
{
   public enum EType {
        Oneshot,
        Continuous
    }

    [SerializeField] EType Type = EType.Oneshot;

    [SerializeField] bool variesWithDistance = false;
    [SerializeField] float maxDistance = 25f;

    [SerializeField] float duration = 0f;

    [SerializeField] float lowSpeedIntensity = 1f;
    [SerializeField] float highSpeedIntensity = 2f;

    [SerializeField] AnimationCurve lowSpeedMotor;
    [SerializeField] AnimationCurve highSpeedMotor;
    [SerializeField] AnimationCurve distanceFalloff;

    [System.NonSerialized] Vector3 effectPosition;
    [System.NonSerialized] float progress;
    public void Initialize(Vector3 _effectPosition) {
        effectPosition = _effectPosition;
        progress = 0f;
    }

    public bool Tick(Vector3 receiverPosition, out float lowSpeed, out float highSpeed) {
        progress += Time.unscaledDeltaTime / duration;

        //calculate distance factor
        float distanceFactor = 1f;
        if (variesWithDistance) {
            float distance = (receiverPosition - effectPosition).magnitude;
            distanceFactor = distance >= maxDistance ? 0f : distanceFalloff.Evaluate(distance / maxDistance);
        }

        lowSpeed = lowSpeedIntensity * distanceFactor * lowSpeedMotor.Evaluate(progress);
        highSpeed = highSpeedIntensity * distanceFactor * highSpeedMotor.Evaluate(progress);

        //check if we're finished with the effect
        if (progress >= 1f) {
            if (Type == EType.Oneshot) {
                return true;
            }
            progress = 0f;
        }
        return false;
    }

}
