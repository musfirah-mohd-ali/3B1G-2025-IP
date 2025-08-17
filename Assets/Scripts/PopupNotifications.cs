using UnityEngine;
using TMPro;
using System.Collections;

public class PopupNotification : MonoBehaviour
{
    public TextMeshProUGUI notificationText; // The text inside the panel
    public CanvasGroup panelCanvasGroup;     // Assign your panel’s CanvasGroup
    public float fadeDuration = 0.5f;
    public float displayTime = 3f;
    public LevelInstructions levelInstructions;

    private int currentStep = 0;
    private bool isShowing = false;

    void Start()
    {
        if (panelCanvasGroup != null)
            panelCanvasGroup.alpha = 0f;

        // Use new level system if available, otherwise fall back to legacy
        if (autoShowOnStart)
        {
            if (allLevelInstructions != null && allLevelInstructions.Length > 0)
            {
                ShowInstructionsForLevel(startingLevel);
            }
            else
            {
                ShowAllInstructions(); // Show all instructions in sequence (legacy mode)
            }
        }
    }

    public void ShowNextInstruction()
    {
        if (isShowing) return;
        if (levelInstructions == null || currentStep >= levelInstructions.instructions.Length) return;

        StartCoroutine(ShowInstructionCoroutine(levelInstructions.instructions[currentStep]));
        currentStep++;
    }

    public void ShowAllInstructions()
    {
        if (levelInstructions == null || levelInstructions.instructions.Length == 0) return;
        StartCoroutine(ShowAllInstructionsCoroutine());
    }

    private IEnumerator ShowAllInstructionsCoroutine()
    {
        currentStep = 0;
        
        for (int i = 0; i < levelInstructions.instructions.Length; i++)
        {
            yield return StartCoroutine(ShowInstructionCoroutine(levelInstructions.instructions[i]));
            
            // Add a small delay between instructions if there are more to show
            if (i < levelInstructions.instructions.Length - 1)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private IEnumerator ShowInstructionCoroutine(string message)
    {
        isShowing = true;
        notificationText.text = message;

        // Fade in panel and text
        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(0f, 1f, elapsed / fadeDuration);
            panelCanvasGroup.alpha = alpha;
            yield return null;
        }
        panelCanvasGroup.alpha = 1f;

        // Wait display time
        yield return new WaitForSeconds(displayTime);

        // Fade out
        elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
            panelCanvasGroup.alpha = alpha;
            yield return null;
        }
        panelCanvasGroup.alpha = 0f;

        isShowing = false;
    }

    public void SetLevel(LevelInstructions newLevel)
    {
        levelInstructions = newLevel;
        currentStep = 0;
    }

    // === LEVEL CONTROL FUNCTIONALITY ===
    
    [Header("Level Control")]
    public LevelInstructions[] allLevelInstructions; // Array of all level instructions
    public bool autoShowOnStart = true;              // Whether to show instructions automatically on Start
    public int startingLevel = 0;                    // Which level to start with
    
    private LevelInstructions currentLevelInstructions;
    private int currentLevel = 0;

    /// <summary>
    /// Show instructions for a specific level
    /// </summary>
    /// <param name="levelIndex">The level index (0-based)</param>
    public void ShowInstructionsForLevel(int levelIndex)
    {
        if (allLevelInstructions == null || levelIndex < 0 || levelIndex >= allLevelInstructions.Length)
        {
            Debug.LogWarning($"[PopupNotification] Invalid level index: {levelIndex}. Using legacy levelInstructions.");
            currentLevelInstructions = levelInstructions;
            currentLevel = 0;
        }
        else
        {
            currentLevel = levelIndex;
            currentLevelInstructions = allLevelInstructions[levelIndex];
        }
        
        if (currentLevelInstructions != null && currentLevelInstructions.instructions.Length > 0)
        {
            StartCoroutine(ShowAllInstructionsForLevelCoroutine());
        }
    }

    /// <summary>
    /// Show specific instructions from a level
    /// </summary>
    /// <param name="levelIndex">The level index</param>
    /// <param name="instructionIndices">Array of instruction indices to show</param>
    public void ShowSpecificInstructions(int levelIndex, int[] instructionIndices)
    {
        if (allLevelInstructions == null || levelIndex < 0 || levelIndex >= allLevelInstructions.Length)
        {
            Debug.LogWarning($"[PopupNotification] Invalid level index: {levelIndex}");
            return;
        }

        currentLevel = levelIndex;
        currentLevelInstructions = allLevelInstructions[levelIndex];
        
        if (instructionIndices == null || instructionIndices.Length == 0)
        {
            ShowInstructionsForLevel(levelIndex);
            return;
        }

        StartCoroutine(ShowSpecificInstructionsCoroutine(instructionIndices));
    }

    /// <summary>
    /// Set the current level without showing instructions
    /// </summary>
    public void SetCurrentLevel(int levelIndex)
    {
        if (allLevelInstructions != null && levelIndex >= 0 && levelIndex < allLevelInstructions.Length)
        {
            currentLevel = levelIndex;
            currentLevelInstructions = allLevelInstructions[levelIndex];
            currentStep = 0;
        }
    }

    /// <summary>
    /// Get the current level index
    /// </summary>
    public int GetCurrentLevel()
    {
        return currentLevel;
    }

    private IEnumerator ShowAllInstructionsForLevelCoroutine()
    {
        currentStep = 0;
        
        for (int i = 0; i < currentLevelInstructions.instructions.Length; i++)
        {
            yield return StartCoroutine(ShowInstructionCoroutine(currentLevelInstructions.instructions[i]));
            
            // Add a small delay between instructions if there are more to show
            if (i < currentLevelInstructions.instructions.Length - 1)
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    private IEnumerator ShowSpecificInstructionsCoroutine(int[] instructionIndices)
    {
        for (int i = 0; i < instructionIndices.Length; i++)
        {
            int index = instructionIndices[i];
            
            if (index >= 0 && index < currentLevelInstructions.instructions.Length)
            {
                yield return StartCoroutine(ShowInstructionCoroutine(currentLevelInstructions.instructions[index]));
                
                // Add delay between instructions if there are more to show
                if (i < instructionIndices.Length - 1)
                {
                    yield return new WaitForSeconds(0.5f);
                }
            }
            else
            {
                Debug.LogWarning($"[PopupNotification] Invalid instruction index: {index} for level {currentLevel}");
            }
        }
    }
}
