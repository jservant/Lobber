using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public GameObject[] areas;
    public List<GameObject> currentTargets = new List<GameObject>();
    private int areasCompleted = 0;
    public bool targetsExist;

    // Start is called before the first frame update
    void Start()
    {
        targetsExist = false;
        UpdateAreas();
    }

    // Update is called once per frame
    void Update() {
        targetsExist = CheckTargets();
        if (!targetsExist && (areasCompleted != 5)) {
            areasCompleted += 1;
            UpdateAreas();
        }
    }

    void UpdateAreas() {
            for (int i = 0; i < areas.Length; i++) {
                if (i <= areasCompleted) {
                    areas[i].SetActive(true);
                }
                else areas[i].SetActive(false);
            }

            foreach (Transform child in areas[areasCompleted].transform) {
                if (child.gameObject.name == "Sandbag") currentTargets.Add(child.gameObject);
            }
    }

    private bool CheckTargets() {
        for (int n = 0; n < currentTargets.Count; n++) {
            if (currentTargets[n] != null) return true;
        }
        return false;
    }
}
