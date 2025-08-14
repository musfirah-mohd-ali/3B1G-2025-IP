using UnityEngine;
using TMPro;
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
    public float deliveryTime = 3f; 
    public GameObject deliveryZonePrefab;
    public float zoneScale = 1f;

    [Header("Timer Reference")]
    public Timer deliveryTimerUI;

    [Header("Player Stats")]
    public int deliveredPackages = 0;
    public int cash = 0;
    public int cashPerDelivery = 50;
    
    [Header("Penalty System")]
    public int trafficViolationPenalty = 25;

    [Header("UI References")]
    public TextMeshProUGUI itemsText;
    public TextMeshProUGUI cashText;

    private bool hasPackage = false;
    private Transform currentTarget;
    private int currentTargetIndex = -1;
    private GameObject currentDeliveryZone;
    private bool playerInZone = false;
    private float deliveryTimer = 0f;
    public PopupNotification PopupNotifications;

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

        if (deliveryTimerUI != null)
        {
            deliveryTimerUI.StartTimer();
        }
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

        if (zonePrefab == null)
        {
            currentDeliveryZone = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            currentDeliveryZone.name = $"DeliveryZone_{GetCurrentLocationName()}";
            Collider zoneCollider = currentDeliveryZone.GetComponent<Collider>();
            zoneCollider.isTrigger = true;
        }
        else
        {
            currentDeliveryZone = Instantiate(zonePrefab);
            currentDeliveryZone.name = $"DeliveryZone_{GetCurrentLocationName()}";
            Collider zoneCollider = currentDeliveryZone.GetComponent<Collider>();
            if (zoneCollider == null)
            {
                zoneCollider = currentDeliveryZone.AddComponent<BoxCollider>();
            }
            zoneCollider.isTrigger = true;
        }

        currentDeliveryZone.transform.position = currentTarget.position;

        if (deliveryZonePrefab == null)
        {
            currentDeliveryZone.transform.localScale = new Vector3(deliveryRange * 2 * zoneScale, 0.5f * zoneScale, deliveryRange * 2 * zoneScale);
        }
        else
        {
            currentDeliveryZone.transform.localScale = Vector3.one * zoneScale;
        }

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

        // Stop timer
        if (deliveryTimerUI != null)
            deliveryTimerUI.StopTimer();

        // Update stats
        deliveredPackages++;
        cash += cashPerDelivery;

        // Update UI
        if (itemsText != null)
            itemsText.text = $"ITEMS: {deliveredPackages}";
        if (cashText != null)
            cashText.text = $"CASH: ${cash}";

        // Reset delivery state
        hasPackage = false;
        playerInZone = false;
        deliveryTimer = 0f;
        currentTarget = null;
        currentTargetIndex = -1;

        if (currentDeliveryZone != null)
        {
            Destroy(currentDeliveryZone);
            currentDeliveryZone = null;
        }
        PopupNotifications.ShowNextInstruction(); // Show next instruction after delivery
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

    public void ApplyTrafficViolationPenalty()
    {
        cash -= trafficViolationPenalty;
        // Ensure cash doesn't go below 0
        if (cash < 0) cash = 0;
        
        // Update UI
        if (cashText != null)
            cashText.text = $"CASH: ${cash}";
            
        Debug.Log($"Traffic violation! Penalty applied: ${trafficViolationPenalty}. Current cash: ${cash}");
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
                string label = (locationNames != null && i < locationNames.Length) ? locationNames[i] : $"Location {i + 1}";
                Handles.Label(deliveryTargets[i].position + Vector3.up * 2f, label);
    #endif
            }
        }
    }
