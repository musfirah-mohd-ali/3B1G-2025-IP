using UnityEngine;
using UnityEngine.SceneManagement;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class MainMenu : MonoBehaviour
{
    [Header("UI Menus")]
    public GameObject mainMenu;
    public GameObject optionsMenu;
    
    [Header("Game Scene Settings")]
#if UNITY_EDITOR
    public SceneAsset gameScene; // Drag game scene file here in inspector
#endif
    [SerializeField] private string gameSceneName; // This stores the scene name
    
    [Header("Fallback Settings")]
    public bool useBuildIndexFallback = true; // Use old method if scene name is empty
    public int gameSceneBuildIndex = 1; // Build index of the game scene

#if UNITY_EDITOR
    void OnValidate()
    {
        // Update the scene name when the SceneAsset changes
        if (gameScene != null)
        {
            gameSceneName = gameScene.name;
        }
    }
#endif

    public void PlayGame()
    {
        // Try to load by scene name first
        if (!string.IsNullOrEmpty(gameSceneName))
        {
            Debug.Log($"Loading game scene: {gameSceneName}");
            SceneManager.LoadScene(gameSceneName);
        }
        // Fallback to build index method
        else if (useBuildIndexFallback)
        {
            Debug.Log($"Loading game scene by build index: {gameSceneBuildIndex}");
            SceneManager.LoadScene(gameSceneBuildIndex);
        }
        // Final fallback to original method
        else
        {
            Debug.Log("Using original build index method (current - 1)");
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex - 1);
        }
    }

    public void OpenOptions()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void BackToMainMenu()
    {
        optionsMenu.SetActive(false);
        mainMenu.SetActive(true);
    }

    public void QuitGame()
    {
        Debug.Log("QUIT!");
        Application.Quit();
    }
}
