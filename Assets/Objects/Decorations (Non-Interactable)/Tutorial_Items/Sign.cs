using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : MonoBehaviour
{
    public GameObject[] inputDisplay;
    private GameManager gameMan;
    public Material[] signMat;

    public bool isHubSign;
    public bool isHardModeSign;


    // Start is called before the first frame update
    void Start()
    {
        gameMan = transform.Find("/GameManager").GetComponent<GameManager>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!isHubSign) {
            if (gameMan.gamePadConnected) {
                inputDisplay[0].SetActive(true);
                inputDisplay[1].SetActive(false);
            }
            else {
                inputDisplay[1].SetActive(true);
                inputDisplay[0].SetActive(false);
            }
        }

        if (isHardModeSign) {
            if (Initializer.save.versionLatest.hardModeUnlocked) {
                inputDisplay[0].SetActive(true);
                transform.parent.GetComponent<MeshRenderer>().material = signMat[1];
            }
            else {
                inputDisplay[0].SetActive(false);
                transform.parent.GetComponent<MeshRenderer>().material = signMat[0];
            }
        }
    }
}
