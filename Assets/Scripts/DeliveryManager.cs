using UnityEngine;
using UnityEngine.SceneManagement;
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

    [Header("Player Stats")]
    public int deliveredPackages = 0;
    public int cash = 0;
    public int cashPerDelivery = 50;
    
    [Header("Level Completion")]
    public int requiredDeliveries = 5;  // Number of deliveries needed to complete level
#if UNITY_EDITOR
    public SceneAsset rewardsScene;  // Drag rewards scene file here in inspector
#endif
    [SerializeField] private string rewardsSceneName;  // This stores the scene name
    
    [Header("Penalty System")]
    public int trafficViolationPenalty = 25;

    [Header("UI References")]
    public TextMeshProUGUI itemsText;
    public TextMeshProUGUI cashText;
    
    [Header("Delivery Marker UI")]
    public GameObject deliveryMarkerUI; // UI panel containing the marker elements
    public RectTransform deliveryArrow; // Arrow pointing to delivery location
    public TextMeshProUGUI distanceText; // Shows distance to delivery
    public TextMeshProUGUI locationNameText; // Shows delivery location name
    public Camera playerCamera; // Reference to player camera for calculations

    private bool hasPackage = false;
    private Transform currentTarget;
    private int currentTargetIndex = -1;
    private int previousTargetIndex = -1; // Track previous delivery location
    private GameObject currentDeliveryZone;
    private bool playerInZone = false;
    private float deliveryTimer = 0f;
    public PopupNotification PopupNotifications;

#if UNITY_EDITOR
    void OnValidate()
    {
        // Update the scene name when the SceneAsset changes
        if (rewardsScene != null)
        {
            rewardsSceneName = rewardsScene.name;
        }
    }
#endif

    void Start()
    {
        // Initialize UI with current values from inspector
        UpdateUI();
        
        // Auto-find player camera if not assigned
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
            if (playerCamera == null)
            {
                playerCamera = FindObjectOfType<Camera>();
            }
        }
        
        // Hide delivery marker initially
        if (deliveryMarkerUI != null)
        {
            deliveryMarkerUI.SetActive(false);
        }
    }

    void Update()
    {
        if (hasPackage && playerInZone)
        {
            UpdateDeliveryTimer();
        }
        
        // Update delivery marker if player has package
        if (hasPackage && currentTarget != null)
        {
            UpdateDeliveryMarker();
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
        ShowDeliveryMarker();
        Debug.Log($"Package picked up! Deliver it to: {GetCurrentLocationName()}");

        // Timer management removed - level timer runs independently
    }
    
    private void SelectRandomTarget()
    {
        // If there's only one target, we have no choice
        if (deliveryTargets.Length <= 1)
        {
            currentTargetIndex = 0;
            currentTarget = deliveryTargets[currentTargetIndex];
            return;
        }
        
        int newTargetIndex;
        int maxAttempts = 20; // Prevent infinite loops
        int attempts = 0;
        
        do
        {
            newTargetIndex = Random.Range(0, deliveryTargets.Length);
            attempts++;
        } 
        while (newTargetIndex == previousTargetIndex && attempts < maxAttempts);
        
        // Update previous target index for next delivery
        previousTargetIndex = currentTargetIndex;
        
        // Set new target
        currentTargetIndex = newTargetIndex;
        currentTarget = deliveryTargets[currentTargetIndex];
        
        Debug.Log($"Selected delivery target {currentTargetIndex} (previous was {previousTargetIndex})");
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
    
    private void ShowDeliveryMarker()
    {
        if (deliveryMarkerUI != null)
        {
            deliveryMarkerUI.SetActive(true);
            
            // Update location name text
            if (locationNameText != null)
            {
                locationNameText.text = GetCurrentLocationName();
            }
        }
    }
    
    private void HideDeliveryMarker()
    {
        if (deliveryMarkerUI != null)
        {
            deliveryMarkerUI.SetActive(false);
        }
    }
    
    private void UpdateDeliveryMarker()
    {
        if (playerCamera == null || currentTarget == null || deliveryMarkerUI == null)
            return;
            
        // Calculate distance to target
        Vector3 playerPosition = playerCamera.transform.position;
        Vector3 targetPosition = currentTarget.position;
        float distance = Vector3.Distance(playerPosition, targetPosition);
        
        // Update distance text
        if (distanceText != null)
        {
            if (distance < 1000f)
            {
                distanceText.text = $"{distance:F0}m";
            }
            else
            {
                distanceText.text = $"{distance / 1000f:F1}km";
            }
        }
        
        // Update arrow direction
        if (deliveryArrow != null)
        {
            // Get direction to target
            Vector3 direction = (targetPosition - playerPosition).normalized;
            
            // Convert to screen space direction
            Vector3 forward = playerCamera.transform.forward;
            Vector3 right = playerCamera.transform.right;
            
            // Project direction onto camera's forward and right vectors
            float forwardDot = Vector3.Dot(direction, forward);
            float rightDot = Vector3.Dot(direction, right);
            
            // Calculate angle for arrow rotation
            float angle = Mathf.Atan2(rightDot, forwardDot) * Mathf.Rad2Deg;
            
            // Apply rotation to arrow
            deliveryArrow.rotation = Quaternion.Euler(0, 0, angle - 90f); // -90 to correct for arrow pointing up by default
        }
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

        // Timer management removed - level timer runs independently

        // Update stats
        deliveredPackages++;
        cash += cashPerDelivery;

        // Update UI
        UpdateUI();
        
        // Check if level is completed
        CheckLevelCompletion();
        
        // Hide delivery marker
        HideDeliveryMarker();

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
        
        // Show next instruction after delivery (with null check)
        if (PopupNotifications != null)
        {
            PopupNotifications.ShowNextInstruction();
        }
        else
        {
            Debug.LogWarning("PopupNotifications is not assigned in DeliveryManager!");
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

    public void ApplyTrafficViolationPenalty()
    {
        Debug.Log($"[PENALTY] ApplyTrafficViolationPenalty called!");
        
        int previousCash = cash;
        cash -= trafficViolationPenalty;
        
        Debug.Log($"[PENALTY] Cash calculation: {previousCash} - {trafficViolationPenalty} = {cash}");
        
        // Ensure cash doesn't go below 0
        if (cash < 0) cash = 0;
        
        // Update UI
        UpdateUI();
        
        Debug.Log($"[PENALTY] Cash UI updated to: CASH: ${cash}");
            
        Debug.Log($"[PENALTY] Traffic violation! Penalty applied: ${trafficViolationPenalty}. Previous cash: ${previousCash}, Current cash: ${cash}");
    }

    private void UpdateUI()
    {
        // Update items text
        if (itemsText != null)
        {
            itemsText.text = $"ITEMS: {deliveredPackages}";
            Debug.Log($"[UI UPDATE] Items text updated to: {itemsText.text}");
        }
        else
        {
            Debug.LogWarning("[UI UPDATE] Items text component is null!");
        }
        
        // Update cash text
        if (cashText != null)
        {
            cashText.text = $"CASH: ${cash}";
            Debug.Log($"[UI UPDATE] Cash text updated to: {cashText.text}");
        }
        else
        {
            Debug.LogWarning("[UI UPDATE] Cash text component is null!");
        }
        
        // Force canvas to update
        Canvas.ForceUpdateCanvases();
    }
    
    private void CheckLevelCompletion()
    {
        Debug.Log($"Delivery completed! Progress: {deliveredPackages}/{requiredDeliveries}");
        
        if (deliveredPackages >= requiredDeliveries)
        {
            Debug.Log("Level completed! All required deliveries finished within time limit!");
            LoadRewardsScene();
        }
    }
    
    private void LoadRewardsScene()
    {
        if (!string.IsNullOrEmpty(rewardsSceneName))
        {
            // Store that this is a good ending (level completed successfully)
            PlayerPrefs.SetString("EndingType", MajorOffenseCounter.EndingType.Good.ToString());
            PlayerPrefs.SetInt("OffenseCount", MajorOffenseCounter.Instance?.GetOffenseCount() ?? 0);
            PlayerPrefs.SetInt("DeliveriesCompleted", deliveredPackages);
            PlayerPrefs.SetInt("FinalCash", cash);
            
            // Get remaining time from Timer if available
            Timer levelTimer = FindObjectOfType<Timer>();
            if (levelTimer != null)
            {
                PlayerPrefs.SetFloat("RemainingTime", levelTimer.GetRemainingTime());
            }
            
            PlayerPrefs.Save();
            
            Debug.Log($"Loading rewards scene: {rewardsSceneName}");
            SceneManager.LoadScene(rewardsSceneName);
        }
        else
        {
            Debug.LogError("Rewards scene not assigned! Please drag a scene file to the Rewards Scene field.");
            
            // Fallback: Show completion message
            Debug.Log("Level completed but no rewards scene assigned!");
        }
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
