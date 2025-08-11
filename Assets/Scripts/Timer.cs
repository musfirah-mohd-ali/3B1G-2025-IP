using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI timerText;
    // [SerializeField] private TextMeshProUGUI deliverTimerText;
    // // float elaspedTime;
    // [SerializeField] float remainingTime;
    public Slider timerSlider;
    // public TextMeshProUGUI timerText;
    public float gameTime;
    private bool stopTimer;

    void Start()
    {
        stopTimer = false;
        timerSlider.maxValue = gameTime;
        timerSlider.value = gameTime;

    }
    void Update()
    {
        // elaspedTime += Time.deltaTime;
        // int minutes = Mathf.FloorToInt(elaspedTime / 60f);
        // int seconds = Mathf.FloorToInt(elaspedTime % 60f);
        // timerText.text = "Time in game: " + string.Format("{0:00}:{1:00}", minutes, seconds);

        // remainingTime -= Time.deltaTime;
        // int remMinutes = Mathf.FloorToInt(remainingTime / 60f);
        // int remSeconds = Mathf.FloorToInt(remainingTime % 60f);
        // deliverTimerText.text = "Remaining time for Delivery: " + string.Format("{0:00}:{1:00}", remMinutes, remSeconds);
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
