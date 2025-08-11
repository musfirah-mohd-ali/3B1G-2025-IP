using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    private TextMeshProUGUI timerText;
    public Slider timerSlider;
    public float gameTime;
    private bool stopTimer;

    void Start()
    {
        stopTimer = false;
        timerSlider.maxValue = gameTime;
        timerSlider.value = gameTime;

    }
    void Update()
    {;
        float time = gameTime - Time.time;
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        string textTime = string.Format("{0:00}:{1:00}", minutes, seconds);
        if (time <= 0)
        {
            stopTimer = true;
        }
        if (stopTimer == false)
        {
            timerText.text = textTime;
            timerSlider.value = time;
        }
    }
}
