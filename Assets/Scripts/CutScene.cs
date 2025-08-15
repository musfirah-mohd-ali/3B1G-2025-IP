using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class CutScene : MonoBehaviour
{
    [Header("Cutscene Lines")]
    [SerializeField] private string[] normalIntroLines;
    [SerializeField] private string[] winLines;
    [SerializeField] private string[] loseLines;

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

    // Static flags to detect context
    private static bool firstTimeHere = true;
    private static bool cameFromWin = false;
    private static bool cameFromLose = false;

    // Public methods to be called from other scripts
    public static void SetWin() => cameFromWin = true;
    public static void SetLose() => cameFromLose = true;

    void Start()
    {
        if (cutSceneCanvas != null)
            cutSceneCanvas.enabled = true;

        // Decide which lines to use
        if (GameState.cameFromWin)
        {
            currentLines = winLines;
        }
        else if (GameState.cameFromLose)
        {
            currentLines = loseLines;
        }
        else
        {
            currentLines = normalIntroLines;
        }

        // Reset flags so next time scene loads fresh
        GameState.cameFromWin = false;
        GameState.cameFromLose = false;

        // Safety check
        if (currentLines == null || currentLines.Length == 0)
        {
            Debug.LogError("CutScene: currentLines is empty! Assign text in the inspector.");
            currentLines = new string[] { "No lines assigned!" };
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
