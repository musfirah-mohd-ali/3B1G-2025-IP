using UnityEngine;


[System.Serializable]
[CreateAssetMenu(fileName = "LightingPreset", menuName = "ScriptableObjects/LightingPreset", order = 1)]
public class LightingPreset : ScriptableObject // Changed from MonoBehaviour to ScriptableObject
{
    public Gradient AmbientColor;
    public Gradient DirectionalColor;
    public Gradient FogColor;
}
