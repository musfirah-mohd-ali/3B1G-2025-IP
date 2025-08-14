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

        ShowNextInstruction(); // Show the first instruction automatically
    }

    public void ShowNextInstruction()
    {
        if (isShowing) return;
        if (levelInstructions == null || currentStep >= levelInstructions.instructions.Length) return;

        StartCoroutine(ShowInstructionCoroutine(levelInstructions.instructions[currentStep]));
        currentStep++;
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
}
