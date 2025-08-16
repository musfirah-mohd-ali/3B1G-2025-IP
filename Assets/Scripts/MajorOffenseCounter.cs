using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class MajorOffenseCounter : MonoBehaviour
{
    [Header("Settings")]
    public int maxOffenses = 3;
    [SerializeField]
    public int badEndingSceneIndex = 3;  // Scene index in Build Settings
    
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
        
        if (currentOffenseCount == maxOffenses)
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