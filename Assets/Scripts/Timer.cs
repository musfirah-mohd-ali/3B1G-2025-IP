using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class Timer : MonoBehaviour
{
    [Header("UI Elements")]
    public GameObject timerUI;
    public TextMeshProUGUI timerText;

    [Header("Timer Settings")]
    [Tooltip("Set the duration of the timer in seconds")]
    public float maxTime = 30f;
    
    [Header("Time Up Scene")]
#if UNITY_EDITOR
    public SceneAsset timeUpScene;  // Drag scene file here in inspector
#endif
    [SerializeField] private string timeUpSceneName;  // This stores the scene name

    private float currentTime;
    private bool timerRunning;

#if UNITY_EDITOR
    void OnValidate()
    {
        // Update the scene name when the SceneAsset changes
        if (timeUpScene != null)
        {
            timeUpSceneName = timeUpScene.name;
        }
    }
#endif

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
                TimeUp();
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
        // Keep the timer UI visible to show final time
        // if (timerUI != null)
        //     timerUI.SetActive(false);
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

    private void TimeUp()
    {
        Debug.Log("Timer: Time is up! Transitioning to time up scene...");
        
        if (!string.IsNullOrEmpty(timeUpSceneName))
        {
            // Store that time ran out (for potential use in the next scene)
            PlayerPrefs.SetString("GameEndReason", "TimeUp");
            PlayerPrefs.SetFloat("FinalTime", 0f);
            PlayerPrefs.Save();
            
            Debug.Log($"Loading time up scene: {timeUpSceneName}");
            SceneManager.LoadScene(timeUpSceneName);
        }
        else
        {
            Debug.LogError("Time up scene not assigned! Please drag a scene file to the Time Up Scene field.");
            
            // Fallback: Trigger bad ending if no scene is assigned
            if (MajorOffenseCounter.Instance != null)
            {
                MajorOffenseCounter.Instance.LoadBadEnding();
            }
        }
    }

    // Method to get remaining time (useful for other scripts)
    public float GetRemainingTime()
    {
        return currentTime;
    }
    
    // Method to get remaining time as percentage (useful for progress bars)
    public float GetRemainingTimePercentage()
    {
        return currentTime / maxTime;
    }
    
    // Method to check if timer is running
    public bool IsTimerRunning()
    {
        return timerRunning;
    }
    
    // Method to add extra time (useful for power-ups or bonuses)
    public void AddTime(float extraTime)
    {
        currentTime += extraTime;
        currentTime = Mathf.Min(currentTime, maxTime); // Cap at maximum time
        UpdateUI();
        Debug.Log($"Timer: Added {extraTime} seconds. New time: {currentTime:F1}");
    }

    // REMOVED: OnPackageDelivered method - deliveries no longer stop the timer
}
