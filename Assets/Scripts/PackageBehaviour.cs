using UnityEngine;

public class PackageBehaviour : MonoBehaviour
{
    public float pickupRange = 3f; // Range for the trigger sphere
    public KeyCode interactKey = KeyCode.E; // Key to press for interaction
    public DeliveryManager deliveryManager; // Assign in inspector
    
    private GameObject nearbyPackage = null; // Package currently in trigger range
    public Timer timer;

    void Awake()
    {
        // Create trigger collider for package detection
        SetupTrigger();

        // Auto-find DeliveryManager if not assigned
        if (deliveryManager == null)
        {
            deliveryManager = FindObjectOfType<DeliveryManager>();
            if (deliveryManager == null)
            {
                Debug.LogError("No DeliveryManager found in scene! Please add one.");
            }
        }
        if( timer == null)
        {
            timer = FindObjectOfType<Timer>();
            if (timer == null)
            {
                Debug.LogError("No Timer found in scene! Please add one.");
            }
        }
    }

    void Update()
    {
        // Check for interact key press
        if (Input.GetKeyDown(interactKey))
        {
            TryPickup();
        }
    }
    
    private void SetupTrigger()
    {
        // Add a sphere collider as trigger for package detection
        SphereCollider triggerCollider = gameObject.GetComponent<SphereCollider>();
        if (triggerCollider == null)
        {
            triggerCollider = gameObject.AddComponent<SphereCollider>();
        }
        
        triggerCollider.isTrigger = true;
        triggerCollider.radius = pickupRange;
    }
    
    private void OnTriggerEnter(Collider other)
    {
        // Check if we entered a package trigger
        if (other.CompareTag("Package"))
        {
            nearbyPackage = other.gameObject;
            Debug.Log("Press Interact to pick up package");
        }
    }
    
    private void OnTriggerExit(Collider other)
    {
        // Check if we left a package trigger
        if (other.CompareTag("Package") && other.gameObject == nearbyPackage)
        {
            nearbyPackage = null;
            Debug.Log("Moved away from package");
        }
    }

    void TryPickup()
    {
        // Check if there's a package nearby
        if (nearbyPackage != null)
        {
            // Start the delivery process
            deliveryManager.StartDelivery();

            if (timer != null)
            {
                timer.maxTime = 300f; // Set game time to 5 minutes for delivery
                timer.StartTimer();
            }

            // Remove the package object
            Destroy(nearbyPackage);
            nearbyPackage = null;
        }
        else
        {
            Debug.Log("No package nearby to pick up");
        }
    }
}
