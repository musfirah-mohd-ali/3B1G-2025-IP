using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CutScene : MonoBehaviour
{
    [Header("Good Ending Dialogue")]
    [SerializeField] private string[] goodEndingLines;
    
    [Header("Bad Ending Dialogue")]
    [SerializeField] private string[] badEndingLines;
    
    [Header("Default Dialogue (if no ending type is set)")]
    [SerializeField] private string[] defaultLines;

    [Header("UI Elements")]
    [SerializeField] private Canvas cutSceneCanvas;
    [SerializeField] private TextMeshProUGUI cutSceneText;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Controls")]
    public KeyCode prevKey = KeyCode.A;
    public KeyCode nextKey = KeyCode.D;
    public KeyCode startGameKey = KeyCode.E;

    private int currentIndex = 0;
    private string[] currentLines;

    void Start()
    {
        if (cutSceneCanvas != null)
            cutSceneCanvas.enabled = true;

        // Determine which dialogue to use based on ending type
        DetermineDialogue();

        currentIndex = 0;
        UpdateText();
    }
    
    void DetermineDialogue()
    {
        // Check if we have ending information from MajorOffenseCounter
        string endingTypeString = PlayerPrefs.GetString("EndingType", "");
        
        if (!string.IsNullOrEmpty(endingTypeString))
        {
            if (endingTypeString == MajorOffenseCounter.EndingType.Good.ToString())
            {
                currentLines = goodEndingLines;
                int offenseCount = PlayerPrefs.GetInt("OffenseCount", 0);
                Debug.Log($"Loading GOOD ending dialogue. Final offense count: {offenseCount}");
            }
            else if (endingTypeString == MajorOffenseCounter.EndingType.Bad.ToString())
            {
                currentLines = badEndingLines;
                int offenseCount = PlayerPrefs.GetInt("OffenseCount", 0);
                Debug.Log($"Loading BAD ending dialogue. Final offense count: {offenseCount}");
            }
        }
        else
        {
            // No ending type set, use default dialogue
            currentLines = defaultLines;
            Debug.Log("No ending type found, using default dialogue");
        }

        // Safety check
        if (currentLines == null || currentLines.Length == 0)
        {
            Debug.LogError("CutScene: No dialogue lines assigned! Please assign dialogue in the inspector.");
            currentLines = new string[] { "No dialogue assigned!" };
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(prevKey))
        {
            currentIndex = Mathf.Max(0, currentIndex - 1);
            UpdateText();
        }

        if (Input.GetKeyDown(nextKey))
        {
            currentIndex = Mathf.Min(currentLines.Length - 1, currentIndex + 1);
            UpdateText();
        }

        // Only allow E to proceed if at the last line
        if (Input.GetKeyDown(startGameKey) && currentIndex == currentLines.Length - 1)
        {
            LoadNextScene();
        }
    }

    void UpdateText()
    {
        if (cutSceneText != null)
            cutSceneText.text = currentLines[currentIndex];

        if (promptText != null)
        {
            if (currentIndex < currentLines.Length - 1)
                promptText.text = "A: Previous | D: Next";
            else
                promptText.text = "Press E to continue";
        }
    }

    void LoadNextScene()
    {
        if (cutSceneCanvas != null)
            cutSceneCanvas.enabled = false;

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadSceneAsync(nextSceneIndex);
    }
}
