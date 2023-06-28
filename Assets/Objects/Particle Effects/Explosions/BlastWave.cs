using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BlastWave : MonoBehaviour
{
    public int pointsCount;
    public float maxRadius;
    public float speed;
    public float startWidth;

    public bool simulate;
    private float simCount = 0f;

    private LineRenderer lineRenderer;

    private void Awake() {
        lineRenderer = GetComponent<LineRenderer>();

        lineRenderer.positionCount = pointsCount + 1;
    }

    void Start() {
        StartCoroutine(Blast());
    }

    private IEnumerator Blast() {
        float currentRadius = 0f;

        while (currentRadius < maxRadius) {
            currentRadius += Time.deltaTime * speed;
            Draw(currentRadius);
            yield return null;
        }
        
        Destroy(gameObject);
    }

    private void Draw(float currentRadius) {
        float angleBetweenPoints = 360f / pointsCount;

        for (int i = 0; i <= pointsCount; i++) {
            float angle = i * angleBetweenPoints * Mathf.Deg2Rad;
            Vector3 direction = new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0f);
            Vector3 position = direction * currentRadius;

            lineRenderer.SetPosition(i, position);
        }

        lineRenderer.widthMultiplier = Mathf.Lerp(0f, startWidth, 1f - currentRadius / maxRadius);
    }

    private void Update() {
        if (simulate && simCount == 0) {
            StartCoroutine(Blast());
            simCount += 1;
        }

        if (simulate == false) {
            simCount = 0;
        }
    }
}
