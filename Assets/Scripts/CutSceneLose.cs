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
    public KeyCode restartKey = KeyCode.E; // after losing, press E to restart or go back to menu

    private int currentIndex = 0;

    void Start()
    {
        if (cutSceneCanvas != null)
            cutSceneCanvas.enabled = true;

        if (loseLines == null || loseLines.Length == 0)
        {
            Debug.LogError("CutSceneLose: loseLines is empty! Assign text in the inspector.");
            loseLines = new string[] { "No lose lines assigned!" };
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
            currentIndex = Mathf.Min(loseLines.Length - 1, currentIndex + 1);
            UpdateText();
        }

        if (Input.GetKeyDown(restartKey) && currentIndex == loseLines.Length - 1)
        {
            ReloadGame();
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

    void ReloadGame()
    {
        if (cutSceneCanvas != null)
            cutSceneCanvas.enabled = false;

        // reload the first scene (0) or whatever you want
        SceneManager.LoadScene(0);
    }
}
