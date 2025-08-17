using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

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

    private int currentIndex = 0;

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

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadSceneAsync(nextSceneIndex);
    }
}
