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
    
    [Header("Traffic Violation Penalty")]
    public int penaltyAmount = 50;  // Customizable penalty amount

    [Header("AI Traffic Control")]
    public bool enableAITrafficControl = true;

    [Header("Police Spawner")]
    public PoliceSpawner policeSpawner; // Assign in Inspector

    private DeliveryManager deliveryManager;
    private bool carInZone = false;
    private BoxCollider triggerCollider;
    private System.Collections.Generic.List<GameObject> aiCarsInZone = new System.Collections.Generic.List<GameObject>();

    private void Start()
    {
        deliveryManager = FindObjectOfType<DeliveryManager>();
        triggerCollider = GetComponent<BoxCollider>();

        if (policeSpawner == null)
        {
            Debug.LogWarning($"[TRAFFIC LIGHT] {name}: PoliceSpawner not assigned! No police will spawn on violations.");
        }

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

    public bool CanGo() => currentLight == LightState.Green;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            carInZone = true;
        else if (other.CompareTag("Car") && enableAITrafficControl && !aiCarsInZone.Contains(other.gameObject))
            aiCarsInZone.Add(other.gameObject);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && carInZone)
        {
            if (currentLight == LightState.Red)
            {
                ApplyTrafficViolationPenalty();
                SpawnPoliceChaser(); // Spawn police on red light violation
            }
            carInZone = false;
        }
        else if (other.CompareTag("Car") && aiCarsInZone.Contains(other.gameObject))
            aiCarsInZone.Remove(other.gameObject);
    }

    private void ApplyTrafficViolationPenalty()
    {
        if (deliveryManager != null)
        {
            deliveryManager.cash = Mathf.Max(0, deliveryManager.cash - penaltyAmount);
            if (deliveryManager.cashText != null)
                deliveryManager.cashText.text = $"CASH: ${deliveryManager.cash}";
        }
    }

    private void SpawnPoliceChaser()
    {
        if (policeSpawner != null)
        {
            Debug.Log($"[TRAFFIC LIGHT] {name}: Spawning police chaser due to red light violation!");
            policeSpawner.SpawnPolice(); // Make sure PoliceSpawner.SpawnPolice() handles the police AI
        }
    }
}
