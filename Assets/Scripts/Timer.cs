using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public Slider timerSlider;
    public float gameTime;
    private bool stopTimer;
    private float startTime;

    void Start()
    {
        stopTimer = false;
        timerSlider.maxValue = gameTime;
        timerSlider.value = gameTime;
        startTime = Time.time;
    }

    void Update()
    {
        float elapsed = Time.time - startTime;
        float time = Mathf.Clamp(gameTime - elapsed, 0, gameTime);
        int minutes = Mathf.FloorToInt(time / 60f);
        int seconds = Mathf.FloorToInt(time % 60f);
        string textTime = string.Format("{0:00}:{1:00}", minutes, seconds);

        if (time <= 0)
        {
            stopTimer = true;
        }
        if (!stopTimer)
        {
            timerText.text = textTime;
            timerSlider.value = time;
        }
    }
}