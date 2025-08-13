using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    public GameObject timerUI;
    public TextMeshProUGUI timerText;
    public float maxTime = 30f;

    private float currentTime;
    private bool timerRunning;

    void Start()
    {
        // Hide timer UI at start
        timerUI.SetActive(false);
    }

    void Update()
    {
        if (timerRunning)
        {
            currentTime -= Time.deltaTime;

            if (currentTime <= 0)
            {
                currentTime = 0;
                StopTimer();
            }

            UpdateUI();
        }
    }

    public void StartTimer()
    {
        currentTime = maxTime;
        timerRunning = true;

        // Show the whole timer UI (image + text)
        timerUI.SetActive(true);
    }

    public void StopTimer()
    {
        timerRunning = false;

        // Hide the whole timer UI
        timerUI.SetActive(false);
    }

    private void UpdateUI()
    {
        int minutes = Mathf.FloorToInt(currentTime / 60);
        int seconds = Mathf.FloorToInt(currentTime % 60);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void OnPackageDelivered()
    {
        StopTimer();
    }
}
