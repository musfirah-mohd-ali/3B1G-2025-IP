using UnityEngine;
using UnityEngine.SceneManagement;

public static class SceneTracker
{
    public static string lastSceneName = "";
    
    public static void RecordScene()
    {
        lastSceneName = SceneManager.GetActiveScene().name;
        Debug.Log($"SceneTracker: Recorded scene '{lastSceneName}'");
    }
}
