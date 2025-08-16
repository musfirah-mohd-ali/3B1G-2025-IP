using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CutSceneLose : MonoBehaviour
{
    [Header("Cutscene Lines")]
    [SerializeField] private string[] loseLines;

    [Header("UI Elements")]
    [SerializeField] private Canvas cutSceneCanvas;
    [SerializeField] private TextMeshProUGUI cutSceneText;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Controls")]
    public KeyCode prevKey = KeyCode.A;
    public KeyCode nextKey = KeyCode.D;
    public KeyCode restartKey = KeyCode.E;

    private int currentIndex = 0;

    void Start()
    {
        if (cutSceneCanvas != null)
            cutSceneCanvas.enabled = true;

        if (loseLines == null || loseLines.Length == 0)
            loseLines = new string[] { "No lose lines assigned!" };

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
            currentIndex = Mathf.Min(loseLines.Length - 1, currentIndex + 1);
            UpdateText();
        }

        if (Input.GetKeyDown(restartKey) && currentIndex == loseLines.Length - 1)
        {
            ReloadPreviousLevel();
        }
    }

    void UpdateText()
    {
        if (cutSceneText != null)
            cutSceneText.text = loseLines[currentIndex];

        if (promptText != null)
        {
            if (currentIndex < loseLines.Length - 1)
                promptText.text = "A: Previous | D: Next";
            else
                promptText.text = "Press E to restart";
        }
    }

    void ReloadPreviousLevel()
    {
        if (cutSceneCanvas != null)
            cutSceneCanvas.enabled = false;

        if (!string.IsNullOrEmpty(SceneTracker.lastSceneName))
        {
            Debug.Log($"Reloading previous scene: {SceneTracker.lastSceneName}");
            SceneManager.LoadScene(SceneTracker.lastSceneName);
        }
        else
        {
            Debug.LogWarning("SceneTracker.lastSceneName is empty! Reloading default scene.");
            SceneManager.LoadScene("MainMenu");
        }
    }
}
