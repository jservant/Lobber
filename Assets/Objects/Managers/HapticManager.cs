using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class HapticManager : MonoBehaviour
{
    public static HapticManager Instance { get; private set; } = null;

    List<HapticEffect> activeEffects = new List<HapticEffect>();
    public static void PlayEffect(HapticEffect effect, Vector3 position) {
        Instance.PlayEffect_Internal(effect, position);
    }

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void PlayEffect_Internal(HapticEffect effect, Vector3 position) {
        var activeEffect = ScriptableObject.Instantiate(effect);
        activeEffect.Initialize(position);

        activeEffects.Add(activeEffect);
    }

    private void Update() {
        float lowSpeedMotor = 0f;
        float highSpeedMotor = 0f;

        for (int i = 0; i < activeEffects.Count; i++) {
            var effect = activeEffects[i];

            //tick the effect
            float lowSpeedComponent = 0f;
            float highSpeedComponent = 0f;

            if (effect.Tick(transform.position, out lowSpeedComponent, out highSpeedComponent)) {
                activeEffects.RemoveAt(i);
                --i;
            }

            lowSpeedMotor = Mathf.Clamp01(lowSpeedComponent + lowSpeedMotor);
            highSpeedMotor = Mathf.Clamp01(highSpeedComponent + highSpeedMotor);
        }

        if (Gamepad.current != null) Gamepad.current.SetMotorSpeeds(lowSpeedMotor, highSpeedMotor);
    }

    private void OnDestroy() {
        if (Gamepad.current != null) Gamepad.current.SetMotorSpeeds(0, 0);
    }
}
