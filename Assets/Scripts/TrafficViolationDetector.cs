using UnityEngine;

public class TrafficViolationDetector : MonoBehaviour
{
    [HideInInspector]
    public CarBehaviour carBehaviour; // Reference to the car that owns this detector
    
    private TrafficLightPoints currentTrafficLight; // Track current traffic light in range

    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[TRAFFIC DETECTOR] Traffic detector triggered by: {other.name} (Tag: {other.tag})");
        
        // Only handle traffic lights
        TrafficLightPoints trafficLight = other.GetComponent<TrafficLightPoints>();
        if (trafficLight != null)
        {
            currentTrafficLight = trafficLight;
            Debug.Log($"[TRAFFIC DETECTOR] Entered traffic light zone: {other.name} - Light is {trafficLight.currentLight}");
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[TRAFFIC DETECTOR] Traffic detector exiting: {other.name} (Tag: {other.tag})");
        
        // Only handle traffic lights
        TrafficLightPoints trafficLight = other.GetComponent<TrafficLightPoints>();
        if (trafficLight != null && trafficLight == currentTrafficLight)
        {
            Debug.Log($"[TRAFFIC DETECTOR] Exiting traffic light zone. Light state: {trafficLight.currentLight}");
            
            // Check if player ran a red light
            if (trafficLight.currentLight == TrafficLightPoints.LightState.Red)
            {
                Debug.Log("[VIOLATION DETECTED] TRAFFIC VIOLATION! Player ran a red light!");
                
                // Apply penalty through the car's DeliveryManager
                if (carBehaviour != null && carBehaviour.deliveryManager != null)
                {
                    Debug.Log($"[PENALTY APPLY] Calling ApplyTrafficViolationPenalty. Current cash: ${carBehaviour.deliveryManager.cash}");
                    carBehaviour.deliveryManager.ApplyTrafficViolationPenalty();
                    Debug.Log($"[PENALTY RESULT] Penalty applied. New cash: ${carBehaviour.deliveryManager.cash}");
                }
                else
                {
                    Debug.LogError("[ERROR] CarBehaviour or DeliveryManager reference not set!");
                }
            }
            else
            {
                Debug.Log($"[NO VIOLATION] Light was {trafficLight.currentLight} - no penalty applied");
            }
            
            currentTrafficLight = null;
            Debug.Log("[TRAFFIC DETECTOR] Traffic light reference cleared");
        }
    }
}
