using UnityEngine;
using System.Collections;

public class TrafficLight : MonoBehaviour
{
    public enum LightState { Green, Yellow, Red }
    public LightState currentLight { get; private set; }

    // Mesh renderers for each lamp
    public MeshRenderer redLight;
    public MeshRenderer yellowLight;
    public MeshRenderer greenLight;

    // Materials
    public Material redUnlit;
    public Material redLit;
    public Material yellowUnlit;
    public Material yellowLit;
    public Material greenUnlit;
    public Material greenLit;

    // Durations
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
            // GREEN phase
            currentLight = LightState.Green;
            SetLights(false, false, true);
            yield return new WaitForSeconds(greenDuration);

            // YELLOW phase
            currentLight = LightState.Yellow;
            SetLights(false, true, false);
            yield return new WaitForSeconds(yellowDuration);

            // RED phase
            currentLight = LightState.Red;
            SetLights(true, false, false);
            yield return new WaitForSeconds(redDuration);
        }
    }

    private void SetLights(bool redOn, bool yellowOn, bool greenOn)
    {
        redLight.material = redOn ? redLit : redUnlit;
        yellowLight.material = yellowOn ? yellowLit : yellowUnlit;
        greenLight.material = greenOn ? greenLit : greenUnlit;
    }
}
