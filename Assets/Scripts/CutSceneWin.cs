using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CutSceneWin : MonoBehaviour
{
    [Header("Cutscene Lines")]
    [SerializeField] private string[] winLines;

    [Header("UI Elements")]
    [SerializeField] private Canvas cutSceneCanvas;
    [SerializeField] private TextMeshProUGUI cutSceneText;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Controls")]
    public KeyCode prevKey = KeyCode.A;
    public KeyCode nextKey = KeyCode.D;
    public KeyCode continueKey = KeyCode.E; // after winning, continue to level 2

    private int currentIndex = 0;

    void Start()
    {
        if (cutSceneCanvas != null)
            cutSceneCanvas.enabled = true;

        if (winLines == null || winLines.Length == 0)
        {
            Debug.LogError("CutSceneWin: winLines is empty! Assign text in the inspector.");
            winLines = new string[] { "No win lines assigned!" };
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
            currentIndex = Mathf.Min(winLines.Length - 1, currentIndex + 1);
            UpdateText();
        }

        if (Input.GetKeyDown(continueKey) && currentIndex == winLines.Length - 1)
        {
            LoadNextLevel();
        }
    }

    void UpdateText()
    {
        if (cutSceneText != null)
            cutSceneText.text = winLines[currentIndex];

        if (promptText != null)
        {
            if (currentIndex < winLines.Length - 1)
                promptText.text = "A: Previous | D: Next";
            else
                promptText.text = "Press E to continue";
        }
    }

    void LoadNextLevel()
    {
        if (cutSceneCanvas != null)
            cutSceneCanvas.enabled = false;

        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadScene(nextSceneIndex);
    }
}
