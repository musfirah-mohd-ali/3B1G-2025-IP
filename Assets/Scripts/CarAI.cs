using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CarAI : MonoBehaviour
{
    [Header("Settings")]
    public float normalSpeed = 10f;       // Speed when green
    public float slowSpeed = 3f;          // Speed when slowing (red/yellow)
    public float stopDistance = 5f;       // Distance from traffic light to start slowing
    public float rotationOffset = 0f;
    public float brakingSmooth = 5f;      // How fast the car slows down
    public float waypointSearchRadius = 50f; // Max distance to search for nearest waypoint
    public bool startFromNearestWaypoint = true; // Enable nearest waypoint detection
    
    [Header("Coroutine Settings")]
    public float updateInterval = 0.1f;    // How often to update (in seconds)
    public float rotationUpdateInterval = 0.05f; // How often to update rotation (reduced for snappier turning)
    public float trafficLightCheckInterval = 0.2f; // How often to check traffic lights
    
    [Header("Rotation Settings")]
    public float rotationSpeed = 15f; // How fast the car turns (higher = snappier)
    public bool useInstantRotation = false; // Enable for instant snappy rotation

    private NavMeshAgent agent;
    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    
    private TrafficLightPoints trafficLightInRange;
    
    // Simple collision avoidance
    private bool isObjectInTrigger = false;  // Flag to track if any car/pedestrian is in our trigger
    private int objectsInTriggerCount = 0;   // Count of cars/pedestrians currently in trigger
    
    // Coroutine references
    private Coroutine movementCoroutine;
    private Coroutine rotationCoroutine;
    private Coroutine trafficLightCoroutine;
    private Coroutine waypointNavigationCoroutine;

    void Start()
    {
        StartCoroutine(InitializeCarAI());
    }
    
    IEnumerator InitializeCarAI()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Reset collision avoidance state
        isObjectInTrigger = false;
        objectsInTriggerCount = 0;
        
        agent.updateRotation = false; // We handle rotation manually
        agent.speed = normalSpeed;
        agent.acceleration = 8f;
        agent.angularSpeed = 360f; // Increased from 120f for faster pathfinding turns
        agent.autoBraking = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        
        // Wait a frame to ensure everything is properly initialized
        yield return null;
        
        // Get waypoints from Waypoints manager
        yield return StartCoroutine(SetupWaypoints());
        
        // Start all the coroutines
        StartAllCoroutines();
    }
    
    IEnumerator SetupWaypoints()
    {
        Waypoints waypointsManager = FindObjectOfType<Waypoints>();
        
        // Wait until waypoints are found
        while (waypointsManager == null || waypointsManager.points == null || waypointsManager.points.Length == 0)
        {
            yield return new WaitForSeconds(0.5f); // Check every half second
            waypointsManager = FindObjectOfType<Waypoints>();
        }
        
        waypoints = waypointsManager.points;
        
        if (startFromNearestWaypoint)
        {
            yield return StartCoroutine(FindNearestWaypointCoroutine());
        }
        else
        {
            currentWaypointIndex = 0; // Start from first waypoint
        }
        
        yield return StartCoroutine(GoToNextWaypointCoroutine());
    }
    
    void StartAllCoroutines()
    {
        // Start the main coroutines
        rotationCoroutine = StartCoroutine(RotationCoroutine());
        trafficLightCoroutine = StartCoroutine(TrafficLightCoroutine());
        waypointNavigationCoroutine = StartCoroutine(WaypointNavigationCoroutine());
        
        Debug.Log($"CarAI on {gameObject.name}: All coroutines started");
    }
    
    void StopAllCoroutines()
    {
        if (rotationCoroutine != null) StopCoroutine(rotationCoroutine);
        if (trafficLightCoroutine != null) StopCoroutine(trafficLightCoroutine);
        if (waypointNavigationCoroutine != null) StopCoroutine(waypointNavigationCoroutine);
    }
    
    IEnumerator RotationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(rotationUpdateInterval);
            
            // Rotate towards destination
            if (agent != null && agent.isOnNavMesh && agent.hasPath && agent.velocity.sqrMagnitude > 0.01f)
            {
                Vector3 direction = agent.steeringTarget - transform.position;
                direction.y = 0; // Keep rotation only on Y-axis to prevent up/down tilting
                
                if (direction.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, rotationOffset, 0);
                    
                    if (useInstantRotation)
                    {
                        // Instant snappy rotation
                        transform.rotation = targetRotation;
                    }
                    else
                    {
                        // Smooth but fast rotation
                        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                    }
                }
            }
        }
    }
    
    IEnumerator WaypointNavigationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);
            
            // Safety checks
            if (agent == null || !agent.isOnNavMesh || waypoints == null || waypoints.Length == 0)
            {
                continue;
            }
            
            // Check if we've reached the current waypoint
            if (!agent.pathPending && agent.remainingDistance < 2f)
            {
                yield return StartCoroutine(GoToNextWaypointCoroutine());
            }
            // Check if agent is stuck (no movement for too long)
            else if (agent.velocity.sqrMagnitude < 0.1f && !agent.isStopped && agent.hasPath)
            {
                // Only recalculate if we've been stuck for a while
                yield return new WaitForSeconds(2f);
                if (agent.velocity.sqrMagnitude < 0.1f && !agent.isStopped)
                {
                    Debug.LogWarning($"CarAI {gameObject.name}: Agent appears stuck, recalculating path...");
                    yield return StartCoroutine(GoToNextWaypointCoroutine());
                }
            }
        }
    }
    
    IEnumerator TrafficLightCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(trafficLightCheckInterval);
            
            // Safety check for NavMeshAgent
            if (agent == null)
            {
                Debug.LogError($"CarAI {gameObject.name}: NavMeshAgent is null!");
                continue;
            }
            
            if (!agent.isOnNavMesh)
            {
                Debug.LogWarning($"CarAI {gameObject.name}: Agent is not on NavMesh! Position: {transform.position}");
                continue;
            }
            
            // Check for path issues
            if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                Debug.LogWarning($"CarAI {gameObject.name}: Invalid path detected, recalculating...");
                yield return StartCoroutine(GoToNextWaypointCoroutine());
                continue;
            }

            // Traffic lights have priority - cars must stop at red lights
            // Collision avoidance is secondary - only applies when traffic light allows movement
            
            bool shouldMove = false;
            string reason = "";
            
            if (trafficLightInRange != null)
            {
                if (trafficLightInRange.CanGo())
                {
                    // Traffic light is green - always move (ignore collisions)
                    shouldMove = true;
                    reason = "Traffic light GREEN - moving (ignoring collisions)";
                }
                else
                {
                    // Traffic light is red - always stop regardless of collision
                    shouldMove = false;
                    reason = "Traffic light RED - must stop";
                }
            }
            else
            {
                // No traffic light - only collision matters
                if (!isObjectInTrigger)
                {
                    shouldMove = true;
                    reason = "No traffic light and no objects in trigger";
                }
                else
                {
                    shouldMove = false;
                    reason = "No traffic light but objects in trigger - collision avoidance";
                }
            }

            // Apply movement decision with additional safety checks
            if (shouldMove && agent.isStopped)
            {
                // Ensure we have a valid destination before resuming
                if (agent.hasPath || agent.pathPending)
                {
                    agent.isStopped = false;
                    Debug.Log($"CarAI {gameObject.name}: MOVING - {reason}");
                }
                else
                {
                    Debug.LogWarning($"CarAI {gameObject.name}: Trying to move but no path available, recalculating...");
                    yield return StartCoroutine(GoToNextWaypointCoroutine());
                }
            }
            else if (!shouldMove && !agent.isStopped)
            {
                agent.isStopped = true;
                Debug.Log($"CarAI {gameObject.name}: STOPPING - {reason}");
            }
        }
    }
    
    IEnumerator GoToNextWaypointCoroutine()
    {
        if (waypoints == null || waypoints.Length == 0) 
        {
            Debug.LogError($"CarAI {gameObject.name}: No waypoints available!");
            yield break;
        }
        if (agent == null || !agent.isOnNavMesh) 
        {
            Debug.LogError($"CarAI {gameObject.name}: Agent invalid or not on NavMesh!");
            yield break;
        }

        // Ensure current waypoint index is valid
        if (currentWaypointIndex >= waypoints.Length)
        {
            Debug.Log($"CarAI {gameObject.name}: Reached end of waypoint path, destroying car...");
            Destroy(gameObject);
            yield break;
        }
        
        // Ensure the waypoint exists
        if (waypoints[currentWaypointIndex] == null)
        {
            Debug.LogError($"CarAI {gameObject.name}: Waypoint {currentWaypointIndex} is null!");
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                Debug.Log($"CarAI {gameObject.name}: No valid waypoints found, destroying car...");
                Destroy(gameObject);
                yield break;
            }
            yield break;
        }

        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        
        // Check if the destination is reachable
        NavMeshPath testPath = new NavMeshPath();
        if (!agent.CalculatePath(targetPosition, testPath) || testPath.status != NavMeshPathStatus.PathComplete)
        {
            Debug.LogWarning($"CarAI {gameObject.name}: Cannot reach waypoint {currentWaypointIndex}, trying next one...");
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length)
            {
                Debug.Log($"CarAI {gameObject.name}: No reachable waypoints remaining, destroying car...");
                Destroy(gameObject);
                yield break;
            }
            yield return StartCoroutine(GoToNextWaypointCoroutine());
            yield break;
        }
        
        bool destinationSet = agent.SetDestination(targetPosition);
        if (!destinationSet)
        {
            Debug.LogError($"CarAI {gameObject.name}: Failed to set destination to waypoint {currentWaypointIndex}");
        }
        else
        {
            Debug.Log($"CarAI {gameObject.name}: Moving to waypoint {currentWaypointIndex} at {targetPosition}");
        }
        
        // Move to next waypoint or destroy if this was the last one
        currentWaypointIndex++;
        if (currentWaypointIndex >= waypoints.Length)
        {
            Debug.Log($"CarAI {gameObject.name}: Reached final waypoint, destroying car...");
            // Wait a moment to ensure the car reaches the waypoint before destroying
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
            yield break;
        }
        
        // Wait a frame to ensure destination is set
        yield return null;
    }
    
    IEnumerator FindNearestWaypointCoroutine()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"CarAI on {gameObject.name}: No waypoints available for nearest search!");
            yield break;
        }
        
        float bestScore = float.MaxValue;
        int bestIndex = 0;
        Vector3 carPosition = transform.position;
        Vector3 carForward = transform.forward;
        
        // Process waypoints in batches to avoid frame drops
        int batchSize = 5;
        int processed = 0;
        
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            
            Vector3 waypointPosition = waypoints[i].position;
            float distance = Vector3.Distance(carPosition, waypointPosition);
            
            // Skip waypoints that are too far
            if (distance > waypointSearchRadius) continue;
            
            // Calculate direction score (prefer waypoints in front of the car)
            Vector3 directionToWaypoint = (waypointPosition - carPosition).normalized;
            float dotProduct = Vector3.Dot(carForward, directionToWaypoint);
            
            // Score combines distance and direction preference
            float directionBonus = (dotProduct > 0) ? 0.5f : 1.5f;
            float score = distance * directionBonus;
            
            // Check if path to waypoint is accessible via NavMesh (async)
            yield return StartCoroutine(CheckPathAccessibility(waypointPosition, (isAccessible) => {
                if (isAccessible)
                {
                    score *= 0.9f; // Apply accessibility bonus
                }
                
                if (score < bestScore)
                {
                    bestScore = score;
                    bestIndex = i;
                }
            }));
            
            processed++;
            
            // Yield every batch to prevent frame drops
            if (processed >= batchSize)
            {
                yield return null;
                processed = 0;
            }
        }
        
        // Set the current waypoint index to the best one found
        currentWaypointIndex = bestIndex;
        
        float actualDistance = Vector3.Distance(carPosition, waypoints[bestIndex].position);
        Debug.Log($"CarAI on {gameObject.name}: Selected waypoint {bestIndex} at distance {actualDistance:F2} (score: {bestScore:F2})");
        
        // If no suitable waypoint found, warn and use first waypoint
        if (bestScore == float.MaxValue)
        {
            Debug.LogWarning($"CarAI on {gameObject.name}: No suitable waypoint found within {waypointSearchRadius} units! Using first waypoint.");
            currentWaypointIndex = 0;
        }
    }
    
    IEnumerator CheckPathAccessibility(Vector3 targetPosition, System.Action<bool> callback)
    {
        NavMeshPath testPath = new NavMeshPath();
        bool isAccessible = false;
        
        // Perform path calculation over multiple frames if needed
        if (agent.CalculatePath(targetPosition, testPath))
        {
            yield return null; // Wait a frame for calculation
            isAccessible = testPath.status == NavMeshPathStatus.PathComplete;
        }
        
        callback?.Invoke(isAccessible);
    }

    void OnTriggerEnter(Collider other)
    {
        StartCoroutine(OnTriggerEnterCoroutine(other));
    }
    
    IEnumerator OnTriggerEnterCoroutine(Collider other)
    {
        Debug.Log($"CarAI {gameObject.name}: Trigger Enter with {other.name}");
        
        // Detect traffic light
        TrafficLightPoints tl = other.GetComponent<TrafficLightPoints>();
        if (tl != null)
        {
            trafficLightInRange = tl;
            Debug.Log($"CarAI {gameObject.name}: Traffic light detected - {tl.name}");
            yield return null; // Wait a frame for state to update
        }
        
        // Detect other AI cars for collision avoidance
        CarAI otherCarAI = other.GetComponent<CarAI>();
        if (otherCarAI != null && otherCarAI != this)
        {
            objectsInTriggerCount++;
            isObjectInTrigger = true;
            Debug.Log($"CarAI {gameObject.name}: Car {other.name} entered trigger (Objects in trigger: {objectsInTriggerCount})");
            // Note: Movement decision will be handled by TrafficLightCoroutine
        }
        
        // Detect pedestrians for collision avoidance
        if (other.CompareTag("Pedestrian"))
        {
            objectsInTriggerCount++;
            isObjectInTrigger = true;
            Debug.Log($"CarAI {gameObject.name}: Pedestrian {other.name} entered trigger (Objects in trigger: {objectsInTriggerCount})");
            // Note: Movement decision will be handled by TrafficLightCoroutine
        }
    }

    void OnTriggerExit(Collider other)
    {
        StartCoroutine(OnTriggerExitCoroutine(other));
    }
    
    IEnumerator OnTriggerExitCoroutine(Collider other)
    {
        // Handle traffic light exit
        TrafficLightPoints tl = other.GetComponent<TrafficLightPoints>();
        if (tl != null && tl == trafficLightInRange)
        {
            trafficLightInRange = null;
            yield return null; // Wait a frame for state to update
        }
        
        // Handle collision avoidance exit for cars
        CarAI otherCarAI = other.GetComponent<CarAI>();
        bool wasCarOrPedestrian = false;
        
        if (otherCarAI != null && otherCarAI != this)
        {
            objectsInTriggerCount--;
            wasCarOrPedestrian = true;
            Debug.Log($"CarAI {gameObject.name}: Car {other.name} exited trigger (Objects remaining: {objectsInTriggerCount})");
        }
        
        // Handle collision avoidance exit for pedestrians
        if (other.CompareTag("Pedestrian"))
        {
            objectsInTriggerCount--;
            wasCarOrPedestrian = true;
            Debug.Log($"CarAI {gameObject.name}: Pedestrian {other.name} exited trigger (Objects remaining: {objectsInTriggerCount})");
        }
        
        // Update state only once if a car or pedestrian exited
        if (wasCarOrPedestrian)
        {
            if (objectsInTriggerCount <= 0)
            {
                objectsInTriggerCount = 0; // Ensure it doesn't go negative
                isObjectInTrigger = false;
                Debug.Log($"CarAI {gameObject.name}: No objects in trigger - collision clear, checking movement");
                
                // Immediately check if we can resume movement
                if (trafficLightInRange == null || trafficLightInRange.CanGo())
                {
                    agent.isStopped = false;
                    Debug.Log($"CarAI {gameObject.name}: RESUMING movement immediately - trigger cleared");
                }
                else
                {
                    Debug.Log($"CarAI {gameObject.name}: Trigger cleared but traffic light is red - staying stopped");
                }
            }
        }
    }
    
    // Public method to manually rescan for nearest waypoint
    public void RescanNearestWaypoint()
    {
        StartCoroutine(RescanNearestWaypointCoroutine());
    }
    
    IEnumerator RescanNearestWaypointCoroutine()
    {
        if (waypoints != null && waypoints.Length > 0)
        {
            yield return StartCoroutine(FindNearestWaypointCoroutine());
            yield return StartCoroutine(GoToNextWaypointCoroutine());
        }
    }
    
    // Public method to pause/resume car AI
    public void PauseCarAI()
    {
        StopAllCoroutines();
        if (agent != null)
            agent.isStopped = true;
    }
    
    public void ResumeCarAI()
    {
        if (agent != null)
            agent.isStopped = false;
        StartAllCoroutines();
    }
    
    // Public method to get current waypoint info
    public int GetCurrentWaypointIndex()
    {
        return currentWaypointIndex;
    }
    
    public Transform GetCurrentWaypoint()
    {
        if (waypoints != null && currentWaypointIndex < waypoints.Length)
            return waypoints[currentWaypointIndex];
        return null;
    }
    // Public method to check collision avoidance status
    public bool IsObjectInTrigger()
    {
        return isObjectInTrigger;
    }
    
    public int GetObjectsInTriggerCount()
    {
        return objectsInTriggerCount;
    }
    
    void OnDestroy()
    {
        // Clean up coroutines when object is destroyed
        StopAllCoroutines();
    }
    
    void OnDisable()
    {
        // Stop coroutines when object is disabled
        StopAllCoroutines();
    }
    
    void OnEnable()
    {
        // Restart coroutines when object is re-enabled
        if (waypoints != null && waypoints.Length > 0)
        {
            StartAllCoroutines();
        }
    }
    
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Draw line to current target waypoint
        if (waypoints != null && currentWaypointIndex < waypoints.Length && waypoints[currentWaypointIndex] != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, waypoints[currentWaypointIndex].position);
            
            // Draw sphere at target waypoint
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(waypoints[currentWaypointIndex].position, 2f);
        }
        
        // Draw search radius
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, waypointSearchRadius);
        
        // Draw forward direction
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 5f);
        
        // Draw trigger visualization
        Gizmos.color = isObjectInTrigger ? Color.red : Color.cyan;
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null && triggerCollider.isTrigger)
        {
            // Draw the trigger bounds
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(triggerCollider.bounds.center - transform.position, triggerCollider.bounds.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
        
        // Draw coroutine status
        if (Application.isPlaying)
        {
            Vector3 labelPos = transform.position + Vector3.up * 3f;
            string status = "Coroutines: ";
            status += (rotationCoroutine != null) ? "R✓ " : "R✗ ";
            status += (trafficLightCoroutine != null) ? "T✓ " : "T✗ ";
            status += (waypointNavigationCoroutine != null) ? "W✓ " : "W✗ ";
            
            if (isObjectInTrigger)
            {
                status += $"\nObjects in trigger: {objectsInTriggerCount}";
            }
            
            UnityEditor.Handles.Label(labelPos, status);
        }
    }
#endif
}
