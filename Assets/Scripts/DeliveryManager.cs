using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DeliveryManager : MonoBehaviour
{
    [Header("Delivery Settings")]
    public Transform[] deliveryTargets;
    public float deliveryRange = 5f;
    public string[] locationNames;
    
    [Header("Delivery Zone Settings")]
    public float deliveryTime = 3f; // Time to stay in zone
    public GameObject deliveryZonePrefab; // Prefab to spawn as delivery zone
    public float zoneScale = 1f; // Scale multiplier for the zone
    
    private bool hasPackage = false;
    private Transform currentTarget;
    private int currentTargetIndex = -1;
    private GameObject currentDeliveryZone;
    private bool playerInZone = false;
    private float deliveryTimer = 0f;

    void Update()
    {
        if (hasPackage && playerInZone)
        {
            UpdateDeliveryTimer();
        }
    }

    public void StartDelivery()
    {
        if (deliveryTargets == null || deliveryTargets.Length == 0)
        {
            Debug.LogError("No delivery targets assigned!");
            return;
        }
        
        hasPackage = true;
        SelectRandomTarget();
        CreateDeliveryZone();
        Debug.Log($"Package picked up! Deliver it to: {GetCurrentLocationName()}");
    }
    
    private void SelectRandomTarget()
    {
        currentTargetIndex = Random.Range(0, deliveryTargets.Length);
        currentTarget = deliveryTargets[currentTargetIndex];
    }
    
    private void CreateDeliveryZone()
    {
        if (currentTarget == null) return;
        
        GameObject zonePrefab = deliveryZonePrefab;
        
        // Fall back to primitive cylinder if no prefab assigned
        if (zonePrefab == null)
        {
            currentDeliveryZone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            currentDeliveryZone.name = $"DeliveryZone_{GetCurrentLocationName()}";
            
            // Setup collider as trigger
            Collider zoneCollider = currentDeliveryZone.GetComponent<Collider>();
            zoneCollider.isTrigger = true;
        }
        else
        {
            // Instantiate the custom prefab
            currentDeliveryZone = Instantiate(zonePrefab);
            currentDeliveryZone.name = $"DeliveryZone_{GetCurrentLocationName()}";
            
            // Ensure it has a trigger collider
            Collider zoneCollider = currentDeliveryZone.GetComponent<Collider>();
            if (zoneCollider == null)
            {
                // Add a trigger collider if the prefab doesn't have one
                zoneCollider = currentDeliveryZone.AddComponent<BoxCollider>();
            }
            zoneCollider.isTrigger = true;
        }
        
        // Position at target location
        currentDeliveryZone.transform.position = currentTarget.position;
        
        // Apply scale
        if (deliveryZonePrefab == null)
        {
            // Scale primitive cylinder based on delivery range
            currentDeliveryZone.transform.localScale = new Vector3(deliveryRange * 2 * zoneScale, 0.5f * zoneScale, deliveryRange * 2 * zoneScale);
        }
        else
        {
            // Scale custom prefab
            currentDeliveryZone.transform.localScale = Vector3.one * zoneScale;
        }
        
        // Add or find DeliveryZone component
        DeliveryZone zoneScript = currentDeliveryZone.GetComponent<DeliveryZone>();
        if (zoneScript == null)
        {
            zoneScript = currentDeliveryZone.AddComponent<DeliveryZone>();
        }
        zoneScript.deliveryManager = this;
        
        Debug.Log($"Delivery zone created at {GetCurrentLocationName()}");
    }
    
    private string GetCurrentLocationName()
    {
        if (locationNames != null && currentTargetIndex >= 0 && currentTargetIndex < locationNames.Length)
            return locationNames[currentTargetIndex];
        return $"Location {currentTargetIndex + 1}";
    }

    private void UpdateDeliveryTimer()
    {
        deliveryTimer += Time.deltaTime;
        
        float remainingTime = deliveryTime - deliveryTimer;
        if (remainingTime > 0)
        {
            Debug.Log($"Delivering... {remainingTime:F1}s remaining");
        }
        else
        {
            CompleteDelivery();
        }
    }

    public void OnPlayerEnterZone()
    {
        if (hasPackage)
        {
            playerInZone = true;
            deliveryTimer = 0f;
            Debug.Log($"Entered delivery zone! Stay for {deliveryTime} seconds to deliver.");
        }
    }

    public void OnPlayerExitZone()
    {
        if (hasPackage)
        {
            playerInZone = false;
            deliveryTimer = 0f;
            Debug.Log("Left delivery zone! Delivery cancelled.");
        }
    }

    private void CompleteDelivery()
    {
        Debug.Log($"Delivery completed at {GetCurrentLocationName()}!");
        
        hasPackage = false;
        playerInZone = false;
        deliveryTimer = 0f;
        currentTarget = null;
        currentTargetIndex = -1;
        
        // Destroy the delivery zone
        if (currentDeliveryZone != null)
        {
            Destroy(currentDeliveryZone);
            currentDeliveryZone = null;
        }
    }

    public Transform GetCurrentTarget() => currentTarget;
    public bool HasPackage() => hasPackage;
    
    public string GetDeliveryStatus()
    {
        if (!hasPackage) return "No package";
        if (currentTarget == null) return "No destination";
        if (playerInZone) return $"Delivering... {deliveryTime - deliveryTimer:F1}s";
        return $"Deliver to: {GetCurrentLocationName()}";
    }

    void OnDrawGizmosSelected()
    {
        if (deliveryTargets == null) return;
        
        for (int i = 0; i < deliveryTargets.Length; i++)
        {
            if (deliveryTargets[i] == null) continue;
            
            Gizmos.color = (deliveryTargets[i] == currentTarget) ? Color.red : Color.green;
            Gizmos.DrawWireSphere(deliveryTargets[i].position, deliveryRange);
            
            #if UNITY_EDITOR
            string label = (locationNames != null && i < locationNames.Length) ? 
                locationNames[i] : $"Location {i + 1}";
            Handles.Label(deliveryTargets[i].position + Vector3.up * 2f, label);
            #endif
        }
    }
}
