using UnityEngine;

/// <summary>
/// Example script showing how to control PopupNotifications for different levels
/// Attach this to a GameObject and assign the PopupNotification component
/// </summary>
public class LevelInstructionController : MonoBehaviour
{
    [Header("Components")]
    public PopupNotification popupNotification;
    
    [Header("Level Control")]
    public int currentGameLevel = 0;
    
    void Start()
    {
        // Example: Show instructions for the current level
        if (popupNotification != null)
        {
            ShowInstructionsForCurrentLevel();
        }
    }
    
    void Update()
    {
        // Example keyboard controls for testing
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ShowLevelInstructions(0); // Show Level 1 instructions
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ShowLevelInstructions(1); // Show Level 2 instructions
        }
        else if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ShowLevelInstructions(2); // Show Level 3 instructions
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            ShowSpecificInstructionsExample(); // Show specific instructions
        }
    }
    
    /// <summary>
    /// Show all instructions for the current game level
    /// </summary>
    public void ShowInstructionsForCurrentLevel()
    {
        if (popupNotification != null)
        {
            popupNotification.ShowInstructionsForLevel(currentGameLevel);
        }
    }
    
    /// <summary>
    /// Show instructions for a specific level
    /// </summary>
    public void ShowLevelInstructions(int levelIndex)
    {
        if (popupNotification != null)
        {
            currentGameLevel = levelIndex;
            popupNotification.ShowInstructionsForLevel(levelIndex);
            Debug.Log($"Showing instructions for Level {levelIndex + 1}");
        }
    }
    
    /// <summary>
    /// Example: Show only specific instructions from current level
    /// For example, show only instructions 0, 2, and 4 from the current level
    /// </summary>
    public void ShowSpecificInstructionsExample()
    {
        if (popupNotification != null)
        {
            int[] specificInstructions = { 0, 2, 4 }; // Show 1st, 3rd, and 5th instructions
            popupNotification.ShowSpecificInstructions(currentGameLevel, specificInstructions);
            Debug.Log($"Showing specific instructions for Level {currentGameLevel + 1}");
        }
    }
    
    /// <summary>
    /// Set the level without showing instructions
    /// </summary>
    public void SetLevel(int levelIndex)
    {
        currentGameLevel = levelIndex;
        if (popupNotification != null)
        {
            popupNotification.SetCurrentLevel(levelIndex);
        }
    }
    
    /// <summary>
    /// Show tutorial instructions (useful for first-time players)
    /// </summary>
    public void ShowTutorialInstructions()
    {
        ShowLevelInstructions(0); // Assuming level 0 is tutorial
    }
    
    /// <summary>
    /// Example: Show different instructions based on game state
    /// </summary>
    public void ShowContextualInstructions(string gameState)
    {
        switch (gameState.ToLower())
        {
            case "tutorial":
                ShowLevelInstructions(0);
                break;
            case "level1":
                ShowLevelInstructions(1);
                break;
            case "level2":
                ShowLevelInstructions(2);
                break;
            case "boss_fight":
                // Show only combat-related instructions
                ShowSpecificInstructions(2, new int[] { 3, 4, 5 });
                break;
            default:
                Debug.LogWarning($"Unknown game state: {gameState}");
                break;
        }
    }
    
    /// <summary>
    /// Show specific instructions from a specific level
    /// </summary>
    public void ShowSpecificInstructions(int levelIndex, int[] instructionIndices)
    {
        if (popupNotification != null)
        {
            popupNotification.ShowSpecificInstructions(levelIndex, instructionIndices);
        }
    }
}
