using UnityEngine;
using TMPro;

public class Timer : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject timerUI;
    public TextMeshProUGUI timerText;

    [Header("Timer Settings")]
    [Tooltip("Set the duration of the timer in seconds")]
    public float maxTime = 30f;

    private float currentTime;
    private bool timerRunning;

    void Start()
    {
        StartTimer();
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

    
        if (timerUI != null)
            timerUI.SetActive(true);

        UpdateUI();
    }

    public void StopTimer()
    {
        timerRunning = false;


        if (timerUI != null)
            timerUI.SetActive(false);
    }

    private void UpdateUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(currentTime / 60);
            int seconds = Mathf.FloorToInt(currentTime % 60);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }

    public void OnPackageDelivered()
    {
        StopTimer();
    }
}
