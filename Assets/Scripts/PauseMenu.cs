using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public static bool GameIsPaused;

    [Header("Menus")]
    public GameObject pauseMenuUI;
    public GameObject optionsMenuUI;

    void Start()
    {
        GameIsPaused = false;
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (optionsMenuUI.activeSelf)
            {
                BackToPauseMenu();
            }
            else
            {
                TogglePause();
            }
        }
    }
    public void Resume()
    {
        GameIsPaused = false;
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(false);
        Time.timeScale = 1f;
    }


    public void TogglePause()
    {
        GameIsPaused = !GameIsPaused;
        pauseMenuUI.SetActive(GameIsPaused);
        optionsMenuUI.SetActive(false);
        Time.timeScale = GameIsPaused ? 0f : 1f;
    }

    public void OpenOptions()
    {
        pauseMenuUI.SetActive(false);
        optionsMenuUI.SetActive(true);
    }

    public void BackToPauseMenu()
    {
        optionsMenuUI.SetActive(false);
        pauseMenuUI.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
