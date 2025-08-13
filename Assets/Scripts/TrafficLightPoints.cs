using UnityEngine;
using System.Collections;

public class TrafficLightPoints : MonoBehaviour
{
    public enum LightState { Green, Yellow, Red }
    public LightState currentLight { get; private set; }

    [Header("Lights")]
    public Light redLight;
    public Light yellowLight;
    public Light greenLight;

    [Header("Durations (seconds)")]
    public float greenDuration = 5f;
    public float yellowDuration = 2f;
    public float redDuration = 5f;

    private void Start()
    {
        StartCoroutine(TrafficLightCycle());
    }

    IEnumerator TrafficLightCycle()
    {
        while (true)
        {
            currentLight = LightState.Green;
            SetLights(false, false, true);
            yield return new WaitForSeconds(greenDuration);

            currentLight = LightState.Yellow;
            SetLights(false, true, false);
            yield return new WaitForSeconds(yellowDuration);

            currentLight = LightState.Red;
            SetLights(true, false, false);
            yield return new WaitForSeconds(redDuration);
        }
    }

    void SetLights(bool redOn, bool yellowOn, bool greenOn)
    {
        redLight.enabled = redOn;
        yellowLight.enabled = yellowOn;
        greenLight.enabled = greenOn;
    }
}
