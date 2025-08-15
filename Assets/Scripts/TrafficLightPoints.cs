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
    
    private DeliveryManager deliveryManager;
    private bool carInZone = false;
    private BoxCollider triggerCollider;
    
    // AI car tracking
    private System.Collections.Generic.List<GameObject> aiCarsInZone = new System.Collections.Generic.List<GameObject>();

    private void Start()
    {
        // Find DeliveryManager for penalties
        deliveryManager = FindObjectOfType<DeliveryManager>();
        if (deliveryManager == null)
        {
            Debug.LogWarning($"[TRAFFIC LIGHT] {name}: No DeliveryManager found - traffic violations may not work properly!");
        }
        
        // Get existing trigger collider (must be manually added in Unity)
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            Debug.LogWarning($"[TRAFFIC LIGHT] {name}: No BoxCollider found! Please add a BoxCollider and set it as a trigger manually.");
        }
        else
        {
            Debug.Log($"[TRAFFIC LIGHT] {name}: Using existing trigger collider");
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
    
    public bool CanGo()
    {
        return currentLight == LightState.Green;
    }
    
    // Unified car detection trigger - handles both player and AI cars
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[TRIGGER DEBUG] {name}: Something entered trigger - Name: '{other.name}', Tag: '{other.tag}'");
        
        if (other.CompareTag("Player"))
        {
            // This is a PLAYER CAR (has "Player" tag)
            carInZone = true;
            Debug.Log($"[TRAFFIC LIGHT] {name}: Player car entered zone - Light: {currentLight}");
        }
        else if (other.CompareTag("Car"))
        {
            // This is an AI CAR (has "Car" tag)
            if (enableAITrafficControl && !aiCarsInZone.Contains(other.gameObject))
            {
                aiCarsInZone.Add(other.gameObject);
                Debug.Log($"[AI TRAFFIC] {name}: AI car '{other.name}' entered zone - Light: {currentLight}");
                
                // Notify AI car about traffic light state
                NotifyAICar(other.gameObject, currentLight);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[TRIGGER DEBUG] {name}: Something exited trigger - Name: '{other.name}', Tag: '{other.tag}'");
        
        if (other.CompareTag("Player") && carInZone)
        {
            // This is the PLAYER CAR exiting
            Debug.Log($"[TRAFFIC LIGHT] {name}: Player car exited zone - Light: {currentLight}");
            
            // Check for red light violation (ONLY for player)
            if (currentLight == LightState.Red)
            {
                ApplyTrafficViolationPenalty();
            }
            else
            {
                Debug.Log($"[TRAFFIC LIGHT] {name}: No violation - light was {currentLight}");
            }
            
            carInZone = false;
        }
        else if (other.CompareTag("Car"))
        {
            // This is an AI CAR exiting
            if (enableAITrafficControl && aiCarsInZone.Contains(other.gameObject))
            {
                aiCarsInZone.Remove(other.gameObject);
                Debug.Log($"[AI TRAFFIC] {name}: AI car '{other.name}' exited zone");
                
                // Notify AI car it can resume normal behavior
                NotifyAICarExit(other.gameObject);
            }
        }
    }
    
    // Method to notify AI cars about traffic light state
    private void NotifyAICar(GameObject aiCar, LightState lightState)
    {
        // Example methods - you can customize based on your AI car implementation
        
        // Method 1: If AI cars have a specific component
        var aiController = aiCar.GetComponent<MonoBehaviour>(); // Replace with your AI car script name
        if (aiController != null)
        {
            // You can use reflection or direct method calls here
            // aiController.SendMessage("OnTrafficLightChanged", lightState, SendMessageOptions.DontRequireReceiver);
        }
        
        // Method 2: Using Unity's messaging system
        aiCar.SendMessage("OnTrafficLightEnter", lightState, SendMessageOptions.DontRequireReceiver);
        
        Debug.Log($"[AI TRAFFIC] Notified {aiCar.name} about {lightState} light");
    }
    
    private void NotifyAICarExit(GameObject aiCar)
    {
        // Notify AI car that it has exited the traffic light zone
        aiCar.SendMessage("OnTrafficLightExit", SendMessageOptions.DontRequireReceiver);
        
        Debug.Log($"[AI TRAFFIC] Notified {aiCar.name} about exiting traffic zone");
    }
    
    // Public method for AI cars to check current light state
    public LightState GetCurrentLightState()
    {
        return currentLight;
    }
    
    // Public method to get all AI cars currently in zone
    public System.Collections.Generic.List<GameObject> GetAICarsInZone()
    {
        return new System.Collections.Generic.List<GameObject>(aiCarsInZone);
    }
    
    private void ApplyTrafficViolationPenalty()
    {
        if (deliveryManager == null)
        {
            Debug.LogError($"[TRAFFIC LIGHT] {name}: Cannot apply penalty - no DeliveryManager!");
            return;
        }
        
        Debug.Log($"[VIOLATION] {name}: RED LIGHT VIOLATION! Applying penalty...");
        
        int previousCash = deliveryManager.cash;
        
        // Apply the custom penalty amount
        deliveryManager.cash = Mathf.Max(0, deliveryManager.cash - penaltyAmount);
        
        // Update UI
        if (deliveryManager.cashText != null)
        {
            deliveryManager.cashText.text = $"CASH: ${deliveryManager.cash}";
        }
        
        // Verify UI was updated
        int newCash = deliveryManager.cash;
        bool uiUpdated = VerifyUIUpdate(previousCash, newCash, penaltyAmount);
        
        Debug.Log($"[VIOLATION] {name}: Penalty applied! Cash: ${previousCash} → ${newCash} (-${penaltyAmount})");
        Debug.Log($"[UI UPDATE] UI update {(uiUpdated ? "SUCCESSFUL" : "FAILED")}");
        
        // Force UI update if needed
        if (!uiUpdated)
        {
            ForceUIUpdate();
        }
    }
    
    private bool VerifyUIUpdate(int previousCash, int newCash, int penalty)
    {
        // Check if cash was properly deducted
        int expectedCash = Mathf.Max(0, previousCash - penalty);
        bool cashCorrect = (newCash == expectedCash);
        
        // Check if UI text was updated
        bool uiTextCorrect = false;
        if (deliveryManager.cashText != null)
        {
            string expectedText = $"CASH: ${newCash}";
            uiTextCorrect = (deliveryManager.cashText.text == expectedText);
            Debug.Log($"[UI CHECK] Expected: '{expectedText}', Actual: '{deliveryManager.cashText.text}'");
        }
        else
        {
            Debug.LogWarning("[UI CHECK] cashText is null - cannot verify UI update");
        }
        
        return cashCorrect && (deliveryManager.cashText == null || uiTextCorrect);
    }
    
    private void ForceUIUpdate()
    {
        Debug.Log($"[UI FORCE] {name}: Forcing UI update...");
        
        if (deliveryManager.cashText != null)
        {
            deliveryManager.cashText.text = $"CASH: ${deliveryManager.cash}";
            Debug.Log($"[UI FORCE] UI text manually updated to: {deliveryManager.cashText.text}");
        }
        
        // Also update items if available
        if (deliveryManager.itemsText != null)
        {
            deliveryManager.itemsText.text = $"ITEMS: {deliveryManager.deliveredPackages}";
        }
        
        // Force canvas update
        Canvas.ForceUpdateCanvases();
        Debug.Log($"[UI FORCE] {name}: Force update completed");
    }
}
