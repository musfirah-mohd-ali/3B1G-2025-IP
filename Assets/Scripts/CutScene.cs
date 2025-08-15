using UnityEngine;
using TMPro;
using System.Collections;
using UnityEngine.SceneManagement;

public class CutScene : MonoBehaviour
{
    [SerializeField] public string[] cutSceneLines;
    [SerializeField] public Canvas cutSceneCanvas;
    [SerializeField] public TextMeshProUGUI cutSceneText;
    [SerializeField] public TextMeshProUGUI promptText;
    public KeyCode nextKey = KeyCode.E;

    [SerializeField] private string nextScene = "Level1";

    void Start()
    {
        StartCoroutine(PlayCutScene());
    }

    IEnumerator PlayCutScene()
    {
        cutSceneCanvas.enabled = true;

        for (int i = 0; i < cutSceneLines.Length; i++)
        {
            cutSceneText.text = cutSceneLines[i];

            if (i < cutSceneLines.Length - 1)
                promptText.text = "Press E to continue";
            else
                promptText.text = "Press E to start game";

            yield return new WaitUntil(() => Input.GetKeyUp(nextKey));
            yield return new WaitUntil(() => Input.GetKeyDown(nextKey));
        }

        cutSceneCanvas.enabled = false;

        SceneManager.LoadScene(nextScene);
    }
}
