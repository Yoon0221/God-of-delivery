using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class PreStart : MonoBehaviour
{
    public GameManager gameManager;
    public TextMeshProUGUI dayText;
    public GameObject canBuy, seeEnding;
    public Slider slider;
    private void Start() {
        gameManager = FindObjectOfType<GameManager>();
        int dayLeft = 1000;
        if (gameManager.buildingNum < 4)
            dayLeft = gameManager.buildDayLimits[gameManager.buildingNum] - gameManager.buildDay + 1;
        if (dayLeft == 1000)
            dayText.text = "제한 없음";
        else if (dayLeft < 1)
            dayText.text = "제한날짜초과";
        else if (dayLeft == 1)
            dayText.text = "마지막날";
        else
            dayText.text = "D - " + dayLeft.ToString();
        if (gameManager.buildingNum <= 3 && gameManager.Day != 0)
            slider.value = ((float)gameManager.buildDay / gameManager.buildDayLimits[gameManager.buildingNum]);
        canBuy.SetActive(false);
        if (gameManager.buildingNum <= 3 && gameManager.buildState <= 5) {
            if (gameManager.TotalCash >= gameManager.buildPrice[gameManager.buildState]) {
                canBuy.SetActive(true);
            }
        }
        if (gameManager.buildState == 6)
            seeEnding.SetActive(true);
        else
            seeEnding.SetActive(false);
    }
    public void Play() {
        gameManager.StartGamePlay();
    }
    public void ToTitle() {
        gameManager.UpdateGameState(GameState.Title);
    }
    public void Building() {
        if (gameManager.buildingNum == 0)
            SceneManager.LoadScene("Building1");
        if (gameManager.buildingNum == 1)
            SceneManager.LoadScene("Building2");
        if (gameManager.buildingNum == 2)
            SceneManager.LoadScene("Building3");
        if (gameManager.buildingNum == 3)
            SceneManager.LoadScene("Building4");
    }
    public void Ending() {
        SceneManager.LoadScene("Ending");
    }

    public void gotowhere()
    {
        SceneManager.LoadScene("Howtoplay");
    }
}
