using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MajorOffenseCounter : MonoBehaviour
{
    [Header("Settings")]
    public int maxOffenses = 3;
    public int badEndingSceneIndex = 1;  // Scene index in Build Settings
    
    [Header("UI")]
    public TextMeshProUGUI offenseCounterText;
    
    private int currentOffenseCount = 0;
    public static MajorOffenseCounter Instance;

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        UpdateUI();
    }

    void OnCollisionEnter(Collision collision)
    {
        // Check if we hit an AI car or pedestrian
        if (collision.gameObject.CompareTag("Car") || collision.gameObject.CompareTag("Pedestrian"))
        {
            AddOffense();
        }
    }

    public void AddOffense()
    {
        currentOffenseCount++;
        Debug.Log($"Major Offense! Count: {currentOffenseCount}");
        
        UpdateUI();
        
        if (currentOffenseCount > maxOffenses)
        {
            LoadBadEnding();
        }
    }

    void UpdateUI()
    {
        if (offenseCounterText != null)
        {
            offenseCounterText.text = $"Major Offenses: {currentOffenseCount}/{maxOffenses}";
        }
    }

    void LoadBadEnding()
    {
        Debug.Log($"Loading bad ending scene (Index: {badEndingSceneIndex})...");
        SceneManager.LoadScene(badEndingSceneIndex);
    }
}