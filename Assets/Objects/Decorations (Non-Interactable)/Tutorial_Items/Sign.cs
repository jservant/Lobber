using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sign : MonoBehaviour
{
    public GameObject[] inputDisplay;
    private GameManager gameMan;

    public bool isHubSign;

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
    }
}
