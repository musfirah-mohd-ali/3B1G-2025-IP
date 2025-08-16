using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MajorOffenseCounter : MonoBehaviour
{
    [Header("Settings")]
    public int maxOffenses = 3;
    
    [Header("Cutscene Scene (for different endings)")]
#if UNITY_EDITOR
    public SceneAsset cutsceneScene;  // Drag scene file here in inspector
#endif
    [SerializeField] private string cutsceneSceneName;  // This stores the scene name

    [Header("UI")]
    public TextMeshProUGUI offenseCounterText;

    private int currentOffenseCount = 0;
    public static MajorOffenseCounter Instance;

    // Ending types that the cutscene can check
    public enum EndingType
    {
        Good,    // Player completed successfully
        Bad      // Player hit too many cars/pedestrians
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        // Update the scene name when the SceneAsset changes
        if (cutsceneScene != null)
        {
            cutsceneSceneName = cutsceneScene.name;
        }
    }
#endif

    void Awake()
    {
        Instance = this;
        SceneTracker.RecordScene(); // Remember the scene player is in
    }

    void Start()
    {
        UpdateUI();
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Hit: {other.name}, Tag: {other.tag}");
        if (other.CompareTag("Pedestrian") || other.CompareTag("CarAI"))
        {
            AddOffense();
        }
    }

    public void AddOffense()
    {
        currentOffenseCount++;
        Debug.Log($"Major Offense! Count: {currentOffenseCount}");
        
        UpdateUI();
        
        if (currentOffenseCount >= maxOffenses)
        {
            LoadBadEnding();
        }
    }

    void UpdateUI()
    {
        if (offenseCounterText != null)
            offenseCounterText.text = $"Major Offenses: {currentOffenseCount}/{maxOffenses}";
    }

    public void LoadBadEnding()
    {
        if (!string.IsNullOrEmpty(cutsceneSceneName))
        {
            // Store that this is a bad ending
            PlayerPrefs.SetString("EndingType", EndingType.Bad.ToString());
            PlayerPrefs.SetInt("OffenseCount", currentOffenseCount);
            PlayerPrefs.Save();
            
            Debug.Log($"Loading cutscene with BAD ending: {cutsceneSceneName}");
            SceneManager.LoadScene(cutsceneSceneName);
        }
        else
        {
            Debug.LogError("Cutscene scene not assigned! Please drag a scene file to the Cutscene Scene field.");
        }
    }
    
    // Method for good ending (call this when player wins)
    public void LoadGoodEnding()
    {
        if (!string.IsNullOrEmpty(cutsceneSceneName))
        {
            // Store that this is a good ending
            PlayerPrefs.SetString("EndingType", EndingType.Good.ToString());
            PlayerPrefs.SetInt("OffenseCount", currentOffenseCount);
            PlayerPrefs.Save();
            
            Debug.Log($"Loading cutscene with GOOD ending: {cutsceneSceneName}");
            SceneManager.LoadScene(cutsceneSceneName);
        }
        else
        {
            Debug.LogError("Cutscene scene not assigned! Please drag a scene file to the Cutscene Scene field.");
        }
    }
    
    // Helper method to get the current ending type (useful for other scripts)
    public static EndingType GetCurrentEndingType()
    {
        string endingTypeString = PlayerPrefs.GetString("EndingType", EndingType.Good.ToString());
        return (EndingType)System.Enum.Parse(typeof(EndingType), endingTypeString);
    }
    
    // Helper method to get the offense count from the previous scene
    public static int GetPreviousOffenseCount()
    {
        return PlayerPrefs.GetInt("OffenseCount", 0);
    }
}
