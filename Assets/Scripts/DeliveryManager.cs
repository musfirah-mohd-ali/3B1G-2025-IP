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
    
    private bool hasPackage = false;
    private Transform currentTarget;
    private int currentTargetIndex = -1;

    void Update()
    {
        if (hasPackage && currentTarget != null)
            CheckDeliveryCompletion();
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
        Debug.Log($"Package picked up! Deliver it to: {GetCurrentLocationName()}");
    }
    
    private void SelectRandomTarget()
    {
        currentTargetIndex = Random.Range(0, deliveryTargets.Length);
        currentTarget = deliveryTargets[currentTargetIndex];
    }
    
    private string GetCurrentLocationName()
    {
        if (locationNames != null && currentTargetIndex >= 0 && currentTargetIndex < locationNames.Length)
            return locationNames[currentTargetIndex];
        return $"Location {currentTargetIndex + 1}";
    }

    private void CheckDeliveryCompletion()
    {
        Camera playerCamera = Camera.main;
        if (playerCamera == null) return;

        float distance = Vector3.Distance(playerCamera.transform.position, currentTarget.position);
        if (distance <= deliveryRange)
            CompleteDelivery();
    }

    private void CompleteDelivery()
    {
        Debug.Log($"Delivery completed at {GetCurrentLocationName()}!");
        hasPackage = false;
        currentTarget = null;
        currentTargetIndex = -1;
    }

    public Transform GetCurrentTarget() => currentTarget;
    public bool HasPackage() => hasPackage;
    
    public string GetDeliveryStatus()
    {
        if (!hasPackage) return "No package";
        if (currentTarget == null) return "No destination";
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
