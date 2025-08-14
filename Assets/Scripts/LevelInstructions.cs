using UnityEngine;

[CreateAssetMenu(fileName = "LevelInstructions", menuName = "Scriptable Objects/LevelInstructions")]
public class LevelInstructions : ScriptableObject
{
    [TextArea]
    public string[] instructions; // Each instruction for this level
}
