using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pulse : MonoBehaviour
{
    public float maxScaleTime;
    public float scaleTime;
    private float currentScale;
    private float targetScale;

    // Start is called before the first frame update
    void Start()
    {
        scaleTime = maxScaleTime;
        currentScale = 1f;
        targetScale = 1.5f;
    }

    // Update is called once per frame
    void Update()
    {
        if (scaleTime > 0) {
            scaleTime -= Time.deltaTime;
            float scale = Mathf.Lerp(targetScale, currentScale, scaleTime / maxScaleTime);
            transform.localScale = new Vector3(scale * 2, scale, 1f);
        }
        else {
            if (currentScale == 1f) {
                currentScale = 1.5f;
                targetScale = 1f;
            }
            else if (currentScale == 1.5f) {
                currentScale = 1f;
                targetScale = 1.5f;
            }
            scaleTime = maxScaleTime;
        }
    }
}
