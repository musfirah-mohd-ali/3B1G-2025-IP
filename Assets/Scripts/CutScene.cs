using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class CutScene : MonoBehaviour
{
    [Header("Cutscene Dialogue")]
    [SerializeField] private string[] cutsceneLines;

    [Header("UI Elements")]
    [SerializeField] private Canvas cutSceneCanvas;
    [SerializeField] private TextMeshProUGUI cutSceneText;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Controls")]
    public KeyCode prevKey = KeyCode.A;
    public KeyCode nextKey = KeyCode.D;
    public KeyCode startGameKey = KeyCode.E;
    
    [Header("Next Scene Settings")]
#if UNITY_EDITOR
    public SceneAsset nextScene; // Drag next scene file here in inspector
#endif
    [SerializeField] private string nextSceneName; // This stores the scene name
    
    [Header("Fallback Settings")]
    public bool useBuildIndexFallback = true; // Use old method if scene name is empty
    public int nextSceneBuildIndex = -1; // Build index of next scene (-1 = auto-calculate)

    private int currentIndex = 0;

#if UNITY_EDITOR
    void OnValidate()
    {
        // Update the scene name when the SceneAsset changes
        if (nextScene != null)
        {
            nextSceneName = nextScene.name;
        }
    }
#endif

    void Start()
    {
        if (cutSceneCanvas != null)
            cutSceneCanvas.enabled = true;

        // Safety check for dialogue lines
        if (cutsceneLines == null || cutsceneLines.Length == 0)
        {
            Debug.LogError("CutScene: No dialogue lines assigned! Please assign dialogue in the inspector.");
            cutsceneLines = new string[] { "No dialogue assigned!" };
        }

        currentIndex = 0;
        UpdateText();
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
            currentIndex = Mathf.Min(cutsceneLines.Length - 1, currentIndex + 1);
            UpdateText();
        }

        // Only allow E to proceed if at the last line
        if (Input.GetKeyDown(startGameKey) && currentIndex == cutsceneLines.Length - 1)
        {
            LoadNextScene();
        }
    }

    void UpdateText()
    {
        if (cutSceneText != null)
            cutSceneText.text = cutsceneLines[currentIndex];

        if (promptText != null)
        {
            if (currentIndex < cutsceneLines.Length - 1)
                promptText.text = "A: Previous | D: Next";
            else
                promptText.text = "Press E to continue";
        }
    }

    void LoadNextScene()
    {
        if (cutSceneCanvas != null)
            cutSceneCanvas.enabled = false;

        // Try to load by scene name first
        if (!string.IsNullOrEmpty(nextSceneName))
        {
            Debug.Log($"Loading next scene: {nextSceneName}");
            SceneManager.LoadSceneAsync(nextSceneName);
        }
        // Fallback to build index method
        else if (useBuildIndexFallback)
        {
            int targetIndex = nextSceneBuildIndex;
            
            // If build index is -1, auto-calculate (original behavior)
            if (targetIndex == -1)
            {
                targetIndex = SceneManager.GetActiveScene().buildIndex + 1;
            }
            
            Debug.Log($"Loading next scene by build index: {targetIndex}");
            SceneManager.LoadSceneAsync(targetIndex);
        }
        // Final fallback to original method
        else
        {
            Debug.Log("Using original build index method (current + 1)");
            int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
            SceneManager.LoadSceneAsync(nextSceneIndex);
        }
    }
}
