using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class CutScene : MonoBehaviour
{
    [Header("Cutscene Lines")]
    [SerializeField] private string[] cutSceneLines;

    [Header("UI Elements")]
    [SerializeField] private Canvas cutSceneCanvas;
    [SerializeField] private TextMeshProUGUI cutSceneText;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Settings")]
    public KeyCode nextKey = KeyCode.E;

    void Start()
    {
        if (cutSceneCanvas != null)
            cutSceneCanvas.enabled = true;

        StartCoroutine(PlayCutScene());
    }

    private IEnumerator PlayCutScene()
    {
        for (int i = 0; i < cutSceneLines.Length; i++)
        {
            if (cutSceneText != null)
                cutSceneText.text = cutSceneLines[i];

            if (promptText != null)
            {
                promptText.text = (i < cutSceneLines.Length - 1) 
                    ? "Press E to continue" 
                    : "Press E to start game";
            }

            // Wait until player presses the key, frame-safe
            yield return new WaitUntil(() => Input.GetKeyDown(nextKey));
        }

        // Hide cutscene UI
        if (cutSceneCanvas != null)
            cutSceneCanvas.enabled = false;

        // Load next scene asynchronously to avoid freezing
        int nextSceneIndex = SceneManager.GetActiveScene().buildIndex + 1;
        SceneManager.LoadSceneAsync(nextSceneIndex);
    }
}
