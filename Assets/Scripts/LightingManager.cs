using UnityEngine;

[ExecuteAlways]
public class LightingManager : MonoBehaviour
{
    [SerializeField] private Light DirectionalLight;
    [SerializeField] private LightingPreset Preset;

    [SerializeField, Range(0, 24)] private float TimeOfDay;
    [SerializeField, Range(0, 24)] private float startTimeOfDay = 6f;
    [SerializeField] private float dayLengthInSeconds = 120f;
    [SerializeField] private ParticleSystem rainVFX;
    [SerializeField] private Transform playerTransform; // Assign your player in the Inspector

    private void Start()
    {
        TimeOfDay = startTimeOfDay;
    }

    private void Update()
    {
        if (Preset == null)
        {
            return;
        }

        if (Application.isPlaying)
        {
            if (dayLengthInSeconds > 0)
            {
                TimeOfDay += (24f / dayLengthInSeconds) * Time.deltaTime;
                TimeOfDay %= 24f;
            }

            // Rain triggers between 9 and 12
            if (rainVFX != null)
            {
                if (TimeOfDay >= 9f && TimeOfDay < 12f)
                {
                    if (!rainVFX.isPlaying)
                        rainVFX.Play();
                    Debug.Log("rain");
                }
                else
                {
                    if (rainVFX.isPlaying)
                        rainVFX.Stop();
                }
            }

            UpdateLighting(TimeOfDay / 24f);

            // Move rain above player
            if (rainVFX != null && playerTransform != null)
            {
                Vector3 rainOffset = new Vector3(0, 2f, 0); // Adjust Y as needed
                rainVFX.transform.position = playerTransform.position + rainOffset;
            }
        }
        else
        {
            UpdateLighting(TimeOfDay / 24f);
        }
    }

    // private void LateUpdate()
    // {
    //     // Make rain follow the player
    //     if (rainVFX != null && playerTransform != null)
    //     {
    //         Vector3 rainOffset = new Vector3(0, 10f, 0); // Adjust Y as needed
    //         rainVFX.transform.position = playerTransform.position + rainOffset;
    //     }
    // }

    private void UpdateLighting(float timePercent)
    {
        RenderSettings.ambientLight = Preset.AmbientColor.Evaluate(timePercent);
        RenderSettings.fogColor = Preset.FogColor.Evaluate(timePercent);

        if (DirectionalLight != null)
        {
            DirectionalLight.color = Preset.DirectionalColor.Evaluate(timePercent);
            DirectionalLight.transform.rotation = Quaternion.Euler(new Vector3((timePercent * 360f) - 90f, 170f, 0f));
        }
    }

    private void OnValidate()
    {
        if (DirectionalLight != null)
        {
            return;
        }
        if (RenderSettings.sun != null)
        {
            DirectionalLight = RenderSettings.sun;
        }
        else
        {
            Light[] lights = GameObject.FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    DirectionalLight = light;
                    return;
                }
            }
        }
    }
}