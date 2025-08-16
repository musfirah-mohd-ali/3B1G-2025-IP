using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class PackageLossUI : MonoBehaviour
{
    [Header("UI References")]
    public TextMeshProUGUI titleText;
    public TextMeshProUGUI messageText;
    public CanvasGroup canvasGroup; // For fade effect
    
    [Header("Fade Settings")]
    public float displayDuration = 3f;
    public float fadeInDuration = 0.5f;
    public float fadeOutDuration = 1f;

    void Start()
    {
        // Set default text if not set in inspector
        if (titleText != null && string.IsNullOrEmpty(titleText.text))
        {
            titleText.text = "PACKAGE CONFISCATED!";
        }
        
        if (messageText != null && string.IsNullOrEmpty(messageText.text))
        {
            messageText.text = "The police caught you with a package while running a red light!\nYour package has been confiscated.";
        }
        
        // Ensure CanvasGroup exists for fade effect
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
            if (canvasGroup == null)
            {
                canvasGroup = gameObject.AddComponent<CanvasGroup>();
            }
        }
        
        // Start hidden
        gameObject.SetActive(false);
    }

    public void ShowUI()
    {
        gameObject.SetActive(true);
        StartCoroutine(ShowWithFadeEffect());
    }

    public void HideUI()
    {
        gameObject.SetActive(false);
    }

    IEnumerator ShowWithFadeEffect()
    {
        // Start with alpha 0 for fade-in effect
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
        
        // Fade in
        yield return StartCoroutine(FadeIn());
        
        // Display for specified duration
        yield return new WaitForSeconds(displayDuration);
        
        // Fade out
        yield return StartCoroutine(FadeOut());
        
        // Hide UI after fade out
        HideUI();
    }

    IEnumerator FadeIn()
    {
        if (canvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeInDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(0f, 1f, elapsedTime / fadeInDuration);
            yield return null;
        }
        canvasGroup.alpha = 1f;
    }

    IEnumerator FadeOut()
    {
        if (canvasGroup == null) yield break;
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeOutDuration)
        {
            elapsedTime += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsedTime / fadeOutDuration);
            yield return null;
        }
        canvasGroup.alpha = 0f;
    }

    public void SetMessage(string title, string message)
    {
        if (titleText != null)
            titleText.text = title;
        
        if (messageText != null)
            messageText.text = message;
    }
}
