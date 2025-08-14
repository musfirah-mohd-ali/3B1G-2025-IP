using UnityEngine;

public class CarBehaviour : MonoBehaviour
{
    [Header("Car Physics")]
    public Rigidbody SphereRB;
    public float fwdSpeed = 50f;
    public float revSpeed = 50f;
    public float turnSpeed = 70f;
    public float brakeForce = 100f;
    public float frictionForce = 40f;
    
    [Header("First Person Mode")]
    public GameObject firstPersonControllerPrefab;
    public Camera carCamera;
    public Transform playerSpawnPoint;
    
    [Header("Traffic System")]
    public DeliveryManager deliveryManager; // Reference to penalty system
    
    // Input variables
    private float moveInput;
    private bool hasExited = false;
    private TrafficLightPoints currentTrafficLight; // Track current traffic light in range

    void Start()
    {
        // Detach the physics sphere from the car visual
        SphereRB.transform.parent = null;
        
        // Auto-find DeliveryManager if not assigned
        if (deliveryManager == null)
        {
            deliveryManager = FindObjectOfType<DeliveryManager>();
            if (deliveryManager == null)
            {
                Debug.LogError("[TRAFFIC SETUP] DeliveryManager not found! Traffic violation penalties will not work.");
            }
            else
            {
                Debug.Log($"[TRAFFIC SETUP] DeliveryManager found: {deliveryManager.name}. Current cash: ${deliveryManager.cash}, Penalty: ${deliveryManager.trafficViolationPenalty}");
            }
        }
        else
        {
            Debug.Log($"[TRAFFIC SETUP] DeliveryManager manually assigned: {deliveryManager.name}");
        }
        
        // Check if this car has a trigger collider
        Collider[] colliders = GetComponents<Collider>();
        bool hasTrigger = false;
        foreach (Collider col in colliders)
        {
            if (col.isTrigger)
            {
                hasTrigger = true;
                Debug.Log($"[TRAFFIC SETUP] Found trigger collider on {gameObject.name}: {col.GetType().Name}");
                break;
            }
        }
        
        if (!hasTrigger)
        {
            Debug.LogError($"[TRAFFIC SETUP] No trigger collider found on {gameObject.name}! Traffic violation detection will not work.");
        }
        
        // Option: Create a separate trigger specifically for traffic violations
        CreateTrafficViolationTrigger();
    }
    
    private void CreateTrafficViolationTrigger()
    {
        // Check if we already have a dedicated traffic trigger
        Transform existingTrigger = transform.Find("TrafficViolationTrigger");
        if (existingTrigger != null)
        {
            Debug.Log("[TRAFFIC SETUP] TrafficViolationTrigger already exists");
            return;
        }
        
        // Create a separate GameObject for traffic violation detection
        GameObject trafficTrigger = new GameObject("TrafficViolationTrigger");
        trafficTrigger.transform.SetParent(transform);
        trafficTrigger.transform.localPosition = Vector3.zero;
        
        // Add a larger trigger collider for traffic detection
        BoxCollider triggerCollider = trafficTrigger.AddComponent<BoxCollider>();
        triggerCollider.isTrigger = true;
        triggerCollider.size = new Vector3(2f, 2f, 2f); // Adjust size as needed
        
        // Add the traffic violation detector component
        TrafficViolationDetector detector = trafficTrigger.AddComponent<TrafficViolationDetector>();
        detector.carBehaviour = this;
        
        Debug.Log("[TRAFFIC SETUP] Created separate TrafficViolationTrigger");
    }

    void Update()
    {
        // Get input and handle car movement
        HandleCarMovement();
        
        // Check if player wants to exit the car
        if (Input.GetKeyDown(KeyCode.E) && !hasExited)
        {
            ExitCar();
        }
    }

    void FixedUpdate()
    {
        // Apply forces to the physics sphere
        SphereRB.AddForce(transform.forward * moveInput, ForceMode.Acceleration);
        ApplyFriction();
    }

    private void HandleCarMovement()
    {
        // Get input
        float rawInput = Input.GetAxisRaw("Vertical");
        moveInput = rawInput * (rawInput > 0 ? fwdSpeed : revSpeed);
        float turnInput = Input.GetAxisRaw("Horizontal");
        
        // Keep car visual aligned with physics sphere
        transform.position = SphereRB.transform.position;
        
        // Rotate car based on input
        transform.Rotate(0, turnInput * turnSpeed * Time.deltaTime, 0, Space.World);
        
        // Apply braking
        if (Input.GetKey(KeyCode.Space))
        {
            ApplyBraking();
        }
    }

    private void ApplyBraking()
    {
        Vector3 velocity = SphereRB.linearVelocity;
        
        if (velocity.magnitude > 0.1f)
        {
            SphereRB.AddForce(-velocity.normalized * brakeForce, ForceMode.Acceleration);
        }
        else
        {
            SphereRB.linearVelocity = Vector3.zero;
        }
    }

    private void ApplyFriction()
    {
        Vector3 velocity = SphereRB.linearVelocity;
        
        // Stop if moving very slowly with no input
        if (velocity.magnitude < 0.5f && Mathf.Abs(moveInput) < 0.1f)
        {
            SphereRB.linearVelocity = Vector3.zero;
            return;
        }
        
        // Apply friction force
        if (velocity.magnitude > 0.1f)
        {
            SphereRB.AddForce(-velocity.normalized * frictionForce * velocity.magnitude * 0.1f, ForceMode.Acceleration);
        }
    }
    
    private void ExitCar()
    {
        if (firstPersonControllerPrefab == null)
        {
            Debug.LogError("First Person Controller prefab not assigned!");
            return;
        }
        
        hasExited = true;
        
        // Spawn position
        Vector3 spawnPos = playerSpawnPoint != null ? 
            playerSpawnPoint.position : 
            transform.position + transform.right * 2f + Vector3.up * 0.5f;
        
        // Create first person controller
        GameObject fpsController = Instantiate(firstPersonControllerPrefab, spawnPos, Quaternion.identity);
        
        // Switch cameras
        SwitchToFirstPersonCamera(fpsController);
        
        this.enabled = false;
        Debug.Log("Switched to First Person Controller");
    }
    
    private void SwitchToFirstPersonCamera(GameObject fpsController)
    {
        // Disable car camera and its audio
        if (carCamera != null)
        {
            carCamera.enabled = false;
            AudioListener carAudio = carCamera.GetComponent<AudioListener>();
            if (carAudio != null) carAudio.enabled = false;
        }
        
        // Enable FPS camera and ensure it has audio
        Camera fpsCamera = fpsController.GetComponentInChildren<Camera>();
        if (fpsCamera != null)
        {
            fpsCamera.enabled = true;
            
            AudioListener fpsAudio = fpsCamera.GetComponent<AudioListener>();
            if (fpsAudio != null)
            {
                fpsAudio.enabled = true;
            }
            else
            {
                fpsCamera.gameObject.AddComponent<AudioListener>();
            }
        }
        else
        {
            Debug.LogError("No camera found in First Person Controller prefab!");
        }
    }
    
    public void EnterCar(GameObject fpsController)
    {
        if (fpsController != null)
        {
            // Switch back to car camera
            SwitchToCarCamera(fpsController);
            
            // Destroy the FPS controller
            Destroy(fpsController);
            
            // Reset car state
            hasExited = false;
            this.enabled = true;
            
            Debug.Log("Switched back to Car Controller");
        }
    }
    
    private void SwitchToCarCamera(GameObject fpsController)
    {
        // Disable FPS camera and its audio
        Camera fpsCamera = fpsController.GetComponentInChildren<Camera>();
        if (fpsCamera != null)
        {
            fpsCamera.enabled = false;
            AudioListener fpsAudio = fpsCamera.GetComponent<AudioListener>();
            if (fpsAudio != null) fpsAudio.enabled = false;
        }
        
        // Enable car camera and ensure it has audio
        if (carCamera != null)
        {
            carCamera.enabled = true;
            
            AudioListener carAudio = carCamera.GetComponent<AudioListener>();
            if (carAudio != null)
            {
                carAudio.enabled = true;
            }
            else
            {
                carCamera.gameObject.AddComponent<AudioListener>();
            }
        }
        else
        {
            Debug.LogError("Car camera not assigned!");
        }
    }
    
    // Traffic light trigger detection
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[TRIGGER ENTER] Player car triggered by object: {other.name} (Tag: {other.tag})");
        
        // Handle traffic light detection
        TrafficLightPoints trafficLight = other.GetComponent<TrafficLightPoints>();
        if (trafficLight != null)
        {
            currentTrafficLight = trafficLight;
            Debug.Log($"[TRAFFIC ENTER] Player car entered traffic light zone: {other.name} - Light is {trafficLight.currentLight}");
            return; // Exit early to avoid processing other logic
        }
        
        // Handle player re-entry (check for first-person controller or player tag)
        if (other.CompareTag("Player") || other.name.Contains("FirstPerson") || other.GetComponent<CharacterController>() != null)
        {
            Debug.Log($"[CAR REENTRY] Player detected near car: {other.name}");
            // Add your car re-entry logic here if needed
            return; // Exit early to avoid processing as traffic light
        }
        
        Debug.Log($"[TRIGGER ENTER] Object {other.name} - no specific handler");
    }
    
    void OnTriggerExit(Collider other)
    {
        Debug.Log($"[TRIGGER EXIT] Player car exiting trigger: {other.name} (Tag: {other.tag})");
        
        // Handle traffic light exit
        TrafficLightPoints trafficLight = other.GetComponent<TrafficLightPoints>();
        if (trafficLight != null)
        {
            Debug.Log($"[TRAFFIC EXIT] Found TrafficLightPoints on {other.name}. Current light: {currentTrafficLight?.name ?? "none"}, This light: {trafficLight.name}");
            
            if (trafficLight == currentTrafficLight)
            {
                Debug.Log($"[TRAFFIC EXIT] Exiting the same traffic light we entered. Light state: {trafficLight.currentLight}");
                
                // Check if player ran a red light
                if (trafficLight.currentLight == TrafficLightPoints.LightState.Red)
                {
                    Debug.Log("[VIOLATION DETECTED] TRAFFIC VIOLATION! Player ran a red light!");
                    
                    // Apply penalty through DeliveryManager
                    if (deliveryManager != null)
                    {
                        Debug.Log($"[PENALTY APPLY] Calling ApplyTrafficViolationPenalty. Current cash: ${deliveryManager.cash}");
                        deliveryManager.ApplyTrafficViolationPenalty();
                        Debug.Log($"[PENALTY RESULT] Penalty applied. New cash: ${deliveryManager.cash}");
                    }
                    else
                    {
                        Debug.LogError("[ERROR] DeliveryManager reference not set in CarBehaviour!");
                    }
                }
                else
                {
                    Debug.Log($"[NO VIOLATION] Light was {trafficLight.currentLight} - no penalty applied");
                }
                
                currentTrafficLight = null;
                Debug.Log("[TRAFFIC EXIT] Player car exited traffic light zone - reference cleared");
            }
            else
            {
                Debug.Log($"[TRAFFIC EXIT] Different traffic light - ignoring (entered: {currentTrafficLight?.name ?? "none"}, exiting: {trafficLight.name})");
            }
            return; // Exit early to avoid processing other logic
        }
        
        // Handle player re-entry exit
        if (other.CompareTag("Player") || other.name.Contains("FirstPerson") || other.GetComponent<CharacterController>() != null)
        {
            Debug.Log($"[CAR REENTRY] Player left car area: {other.name}");
            // Add your car re-entry exit logic here if needed
            return; // Exit early
        }
        
        Debug.Log($"[TRIGGER EXIT] Object {other.name} - no specific handler");
    }
}
