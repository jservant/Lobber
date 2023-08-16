using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SpecialSign : MonoBehaviour
{
    public GameObject[] inputDisplay;
    public GameObject[] _inputDisplay;
    public GameObject[] keyboardButtons;
    private GameManager gameMan;

    public Sprite[] iconsC;
    public Image iconImage;
    private int spriteIndex;

    private float displayTime = 0;

    // Start is called before the first frame update
    void Start()
    {
        gameMan = transform.Find("/GameManager").GetComponent<GameManager>();

        spriteIndex = 0;
        iconImage.sprite = iconsC[spriteIndex];
    }

    // Update is called once per frame
    void Update()
    {
        if (displayTime > 0) {
            displayTime -= Time.deltaTime;
        }
        else {
            ChangeIcons();
        }

        if (gameMan.gamePadConnected) {
            inputDisplay[0].SetActive(true);
            inputDisplay[1].SetActive(false);

            _inputDisplay[0].SetActive(true);
            _inputDisplay[1].SetActive(false);
        }
        else {
            inputDisplay[1].SetActive(true);
            inputDisplay[0].SetActive(false);

            _inputDisplay[1].SetActive(true);
            _inputDisplay[0].SetActive(false);
        }
    }
    void ChangeIcons() {
        keyboardButtons[spriteIndex].SetActive(false);
        spriteIndex += 1;
        if (spriteIndex > 3) spriteIndex = 0;
        iconImage.sprite = iconsC[spriteIndex];
        displayTime = 1f;

        keyboardButtons[spriteIndex].SetActive(true);
    }
}

  
