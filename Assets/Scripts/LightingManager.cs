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

    // Rain timing variables
    [SerializeField] private float minRainDuration = 1f; // in hours
    [SerializeField] private float maxRainDuration = 5f; // in hours
    private float rainStartTime;
    private float rainEndTime;

    private void Start()
    {
        TimeOfDay = startTimeOfDay;

        // Pick a random start time and duration for rain
        rainStartTime = Random.Range(0f, 24f - maxRainDuration);
        float rainDuration = Random.Range(minRainDuration, maxRainDuration);
        rainEndTime = rainStartTime + rainDuration;
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

            // Rain at random time
            if (rainVFX != null)
            {
                var childParticles = rainVFX.GetComponentsInChildren<ParticleSystem>();
                foreach (var ps in childParticles)
                {
                    if (TimeOfDay >= rainStartTime && TimeOfDay < rainEndTime)
                    {
                        if (!ps.isPlaying) ps.Play();
                    }
                    else
                    {
                        if (ps.isPlaying) ps.Stop();
                    }
                }
            }

            UpdateLighting(TimeOfDay / 24f);

            // Move rain above player
            if (rainVFX != null && playerTransform != null)
            {
                Vector3 rainOffset = new Vector3(0, 1.2f, 0);
                rainVFX.transform.position = playerTransform.position + rainOffset;
            }
        }
        else
        {
            UpdateLighting(TimeOfDay / 24f);
        }
    }

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