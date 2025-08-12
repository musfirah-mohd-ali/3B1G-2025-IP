using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public Slider timerSlider;

    public float maxTime = 30f; // time in seconds
    private float currentTime;
    private bool timerRunning = false;

    void Start()
    {
        // Hide the slider and timer text at the start
        timerSlider.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
    }

    void Update()
    {
        if (timerRunning)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                StopTimer(); // Hides slider when time runs out
            }

            UpdateUI();
        }
    }

    public void StartTimer()
    {
        currentTime = maxTime;
        timerRunning = true;

        // Show slider and text when starting
        timerSlider.gameObject.SetActive(true);
        timerText.gameObject.SetActive(true);
    }

    public void StopTimer()
    {
        timerRunning = false;

        // Hide slider and text
        timerSlider.gameObject.SetActive(false);
        timerText.gameObject.SetActive(false);
    }

    void UpdateUI()
    {
        timerSlider.value = currentTime / maxTime;

        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);

        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }


    // Call this when package is delivered
    public void OnPackageDelivered()
    {
        StopTimer(); // stops and hides timer immediately
    }
}
