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
    
    [Header("Traffic Violation Detection")]
    public bool enableTrafficViolations = true;
    [Space]
    [Header("AI Traffic Control")]
    public bool enableAITrafficControl = true;
    [Space]
    [Header("Trigger Zone Size")]
    public float triggerWidth = 5f;   // X-axis
    public float triggerLength = 5f;  // Z-axis (forward/backward)
    public float triggerHeight = 3f;  // Y-axis (up/down)
    [Space]
    [Header("Trigger Zone Position")]
    public Vector3 triggerOffset = Vector3.zero; // Offset from traffic light position
    
    private DeliveryManager deliveryManager;
    private bool carInZone = false;
    private CarBehaviour detectedCar = null;
    private BoxCollider triggerCollider;
    
    // AI car tracking
    private System.Collections.Generic.List<GameObject> aiCarsInZone = new System.Collections.Generic.List<GameObject>();

    private void Start()
    {
        // Find DeliveryManager for penalties
        deliveryManager = FindObjectOfType<DeliveryManager>();
        if (deliveryManager == null)
        {
            Debug.LogWarning($"[TRAFFIC LIGHT] {name}: No DeliveryManager found - violations disabled");
            enableTrafficViolations = false;
        }
        
        // Setup trigger collider for car detection
        SetupTriggerZone();
        
        StartCoroutine(TrafficLightCycle());
    }
    
    private void SetupTriggerZone()
    {
        if (!enableTrafficViolations) return;
        
        // Get or add BoxCollider
        triggerCollider = GetComponent<BoxCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<BoxCollider>();
            Debug.Log($"[TRAFFIC LIGHT] {name}: Added new trigger zone");
        }
        
        // Configure the trigger
        triggerCollider.isTrigger = true;
        UpdateTriggerSize();
        
        Debug.Log($"[TRAFFIC LIGHT] {name}: Trigger zone configured - Width: {triggerWidth}, Length: {triggerLength}, Height: {triggerHeight}");
    }
    
    private void UpdateTriggerSize()
    {
        if (triggerCollider != null)
        {
            triggerCollider.size = new Vector3(triggerWidth, triggerHeight, triggerLength);
            triggerCollider.center = triggerOffset;
        }
    }
    
    // Update trigger size when values change in inspector
    void OnValidate()
    {
        // Ensure positive values
        triggerWidth = Mathf.Max(0.1f, triggerWidth);
        triggerLength = Mathf.Max(0.1f, triggerLength);
        triggerHeight = Mathf.Max(0.1f, triggerHeight);
        
        // Update trigger size if we're in play mode
        if (Application.isPlaying && triggerCollider != null)
        {
            UpdateTriggerSize();
        }
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
    
    // Car detection triggers
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[TRIGGER DEBUG] {name}: Something entered trigger - Name: '{other.name}', Tag: '{other.tag}'");
        
        // Handle player car for penalty system
        if (enableTrafficViolations)
        {
            // Try to find CarBehaviour on the collider, its parent, or its root
            CarBehaviour playerCar = other.GetComponent<CarBehaviour>();
            if (playerCar == null)
            {
                playerCar = other.GetComponentInParent<CarBehaviour>();
            }
            if (playerCar == null)
            {
                playerCar = other.transform.root.GetComponent<CarBehaviour>();
            }
            
            Debug.Log($"[TRIGGER DEBUG] {name}: CarBehaviour found: {playerCar != null}");
            if (playerCar != null)
            {
                Debug.Log($"[TRIGGER DEBUG] {name}: CarBehaviour found on object: '{playerCar.gameObject.name}'");
            }
            
            // Check for player car - try multiple tag possibilities
            if (playerCar != null && (other.CompareTag("Player") || other.CompareTag("Car") || other.name.Contains("Car") || 
                playerCar.gameObject.CompareTag("Player") || playerCar.gameObject.CompareTag("Car")))
            {
                carInZone = true;
                detectedCar = playerCar;
                Debug.Log($"[TRAFFIC LIGHT] {name}: Player car entered zone - Light: {currentLight}");
            }
            else if (playerCar != null)
            {
                Debug.LogWarning($"[TRIGGER DEBUG] {name}: Found CarBehaviour on '{playerCar.gameObject.name}' but tags didn't match. Collider tag: '{other.tag}', CarBehaviour object tag: '{playerCar.gameObject.tag}'");
            }
        }
        
        // Handle AI cars for traffic control (excluding player car)
        if (enableAITrafficControl)
        {
            // Check for AI cars with "Car" tag (but not the player car)
            CarBehaviour carComponent = other.GetComponent<CarBehaviour>();
            if (carComponent == null)
            {
                carComponent = other.GetComponentInParent<CarBehaviour>();
            }
            if (carComponent == null)
            {
                carComponent = other.transform.root.GetComponent<CarBehaviour>();
            }
            
            if (other.CompareTag("Car") && carComponent == null) // AI cars shouldn't have CarBehaviour
            {
                if (!aiCarsInZone.Contains(other.gameObject))
                {
                    aiCarsInZone.Add(other.gameObject);
                    Debug.Log($"[AI TRAFFIC] {name}: AI car '{other.name}' entered zone - Light: {currentLight}");
                    
                    // Notify AI car about traffic light state
                    NotifyAICar(other.gameObject, currentLight);
                }
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[TRIGGER DEBUG] {name}: Something exited trigger - Name: '{other.name}', Tag: '{other.tag}'");
        
        // Handle player car for penalty system
        if (enableTrafficViolations)
        {
            // Try to find CarBehaviour on the collider, its parent, or its root
            CarBehaviour playerCar = other.GetComponent<CarBehaviour>();
            if (playerCar == null)
            {
                playerCar = other.GetComponentInParent<CarBehaviour>();
            }
            if (playerCar == null)
            {
                playerCar = other.transform.root.GetComponent<CarBehaviour>();
            }
            
            // Check if this is the player car that was detected earlier
            if (playerCar != null && playerCar == detectedCar && 
                (other.CompareTag("Player") || other.CompareTag("Car") || other.name.Contains("Car") ||
                playerCar.gameObject.CompareTag("Player") || playerCar.gameObject.CompareTag("Car")))
            {
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
                detectedCar = null;
            }
        }
        
        // Handle AI cars leaving (excluding player car)
        if (enableAITrafficControl)
        {
            CarBehaviour carComponent = other.GetComponent<CarBehaviour>();
            if (carComponent == null)
            {
                carComponent = other.GetComponentInParent<CarBehaviour>();
            }
            if (carComponent == null)
            {
                carComponent = other.transform.root.GetComponent<CarBehaviour>();
            }
            
            if (other.CompareTag("Car") && carComponent == null) // AI cars shouldn't have CarBehaviour
            {
                if (aiCarsInZone.Contains(other.gameObject))
                {
                    aiCarsInZone.Remove(other.gameObject);
                    Debug.Log($"[AI TRAFFIC] {name}: AI car '{other.name}' exited zone");
                    
                    // Notify AI car it can resume normal behavior
                    NotifyAICarExit(other.gameObject);
                }
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
        int penaltyAmount = deliveryManager.trafficViolationPenalty;
        
        // Apply the penalty
        deliveryManager.ApplyTrafficViolationPenalty();
        
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
    
    // Visual gizmos for the editor
    void OnDrawGizmos()
    {
        if (!enableTrafficViolations) return;
        
        Vector3 triggerSize = new Vector3(triggerWidth, triggerHeight, triggerLength);
        Vector3 triggerPosition = transform.position + transform.TransformDirection(triggerOffset);
        
        // Draw violation detection range
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(triggerPosition, triggerSize);
        
        // Draw a semi-transparent cube to show the detection area
        Gizmos.color = new Color(1f, 1f, 0f, 0.2f); // Yellow with transparency
        Gizmos.DrawCube(triggerPosition, triggerSize);
        
        // Draw line from traffic light to trigger center if offset
        if (triggerOffset != Vector3.zero)
        {
            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, triggerPosition);
            Gizmos.DrawWireSphere(triggerPosition, 0.2f);
        }
    }
    
    void OnDrawGizmosSelected()
    {
        if (!enableTrafficViolations) return;
        
        Vector3 triggerSize = new Vector3(triggerWidth, triggerHeight, triggerLength);
        Vector3 triggerPosition = transform.position + transform.TransformDirection(triggerOffset);
        
        // Draw a more prominent visualization when selected
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(triggerPosition, triggerSize);
        
        // Draw detection range with current light color
        switch (currentLight)
        {
            case LightState.Green:
                Gizmos.color = new Color(0f, 1f, 0f, 0.3f);
                break;
            case LightState.Yellow:
                Gizmos.color = new Color(1f, 1f, 0f, 0.3f);
                break;
            case LightState.Red:
                Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
                break;
        }
        Gizmos.DrawCube(triggerPosition, triggerSize);
        
        // Draw offset indicators when selected
        if (triggerOffset != Vector3.zero)
        {
            // Draw connection line
            Gizmos.color = Color.white;
            Gizmos.DrawLine(transform.position, triggerPosition);
            
            // Draw offset vector components
            Gizmos.color = Color.red;
            if (triggerOffset.x != 0) Gizmos.DrawRay(transform.position, transform.right * triggerOffset.x);
            Gizmos.color = Color.green;
            if (triggerOffset.y != 0) Gizmos.DrawRay(transform.position, transform.up * triggerOffset.y);
            Gizmos.color = Color.blue;
            if (triggerOffset.z != 0) Gizmos.DrawRay(transform.position, transform.forward * triggerOffset.z);
        }
        
        // Draw car detection status
        if (carInZone && detectedCar != null)
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(triggerPosition, detectedCar.transform.position);
            Gizmos.DrawWireSphere(detectedCar.transform.position, 1f);
        }
    }

}
