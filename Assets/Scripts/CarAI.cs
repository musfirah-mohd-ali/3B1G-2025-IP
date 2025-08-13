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
    public float rotationUpdateInterval = 0.1f; // Increased from 0.05f for stability
    public float trafficLightCheckInterval = 0.2f; // How often to check traffic lights
    
    [Header("Collision Avoidance")]
    public float safeFollowingDistance = 8f;    // Minimum distance to maintain from car in front (used for trigger size reference)

    private NavMeshAgent agent;
    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    
    private TrafficLightPoints trafficLightInRange;
    private bool isAvoidingCollision = false;  // Flag to track collision avoidance state
    private GameObject carInFront = null;      // Reference to the car we're following
    
    // Position stabilization
    private float groundY; // Store the initial ground Y position
    
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
        
        // Store initial ground position
        groundY = transform.position.y;
        
        agent.updateRotation = false;
        agent.updateUpAxis = false; // Prevent vertical position changes
        agent.speed = normalSpeed;
        agent.acceleration = 6f; // Reduced from 8f for smoother movement
        agent.angularSpeed = 120f; // Limit rotation speed
        agent.autoBraking = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance; // Reduced for performance
        agent.baseOffset = 0f; // Ensure agent stays at ground level

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
            
            // Stabilize Y position to prevent glitching
            if (Mathf.Abs(transform.position.y - groundY) > 0.1f)
            {
                Vector3 stabilizedPosition = transform.position;
                stabilizedPosition.y = groundY;
                transform.position = stabilizedPosition;
            }
            
            // Rotate towards destination
            if (agent != null && agent.isOnNavMesh && agent.hasPath && agent.velocity.sqrMagnitude > 0.01f)
            {
                Vector3 direction = agent.steeringTarget - transform.position;
                direction.y = 0; // Keep rotation only on Y-axis to prevent up/down tilting
                
                if (direction.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, rotationOffset, 0);
                    // Use interval-based interpolation instead of deltaTime
                    float rotationSpeed = 3f; // Reduced from 5f for smoother movement
                    transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotationSpeed * rotationUpdateInterval);
                }
            }
        }
    }
    
    IEnumerator WaypointNavigationCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(updateInterval);
            
            // Check if we've reached the current waypoint
            if (agent != null && agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance < 2f)
            {
                yield return StartCoroutine(GoToNextWaypointCoroutine());
            }
        }
    }
    
    IEnumerator TrafficLightCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(trafficLightCheckInterval);
            
            if (agent == null || !agent.isOnNavMesh) continue;

            if (trafficLightInRange != null)
            {
                if (trafficLightInRange.CanGo())
                {
                    // Only resume if not avoiding collision
                    if (!isAvoidingCollision)
                        agent.isStopped = false; // green and no car ahead
                }
                else
                {
                    agent.isStopped = true;  // red/yellow - always stop
                }
            }
            else
            {
                // No traffic light - only move if not avoiding collision
                if (!isAvoidingCollision)
                    agent.isStopped = false;
            }
        }
    }
    
    IEnumerator GoToNextWaypointCoroutine()
    {
        if (waypoints == null || waypoints.Length == 0) yield break;
        if (agent == null || !agent.isOnNavMesh) yield break;

        agent.SetDestination(waypoints[currentWaypointIndex].position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        
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
        Debug.Log("CarAI: Trigger Enter with " + other.name);
        
        // Detect traffic light
        TrafficLightPoints tl = other.GetComponent<TrafficLightPoints>();
        if (tl != null)
        {
            trafficLightInRange = tl;
            yield return null; // Wait a frame for state to update
        }
        
        // Detect other AI cars for collision avoidance
        CarAI otherCarAI = other.GetComponent<CarAI>();
        if (otherCarAI != null && otherCarAI != this)
        {
            // Check if the other car is in front of us
            Vector3 directionToOther = (other.transform.position - transform.position).normalized;
            float dotProduct = Vector3.Dot(transform.forward, directionToOther);
            
            // If the other car is roughly in front of us (dot product > 0.5)
            if (dotProduct > 0.5f)
            {
                if (!isAvoidingCollision)
                {
                    isAvoidingCollision = true;
                    carInFront = other.gameObject;
                    agent.isStopped = true;
                    Debug.Log($"CarAI on {gameObject.name}: Stopping to avoid collision with {carInFront.name} (trigger-based)");
                }
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        StartCoroutine(OnTriggerExitCoroutine(other));
    }
    
    IEnumerator OnTriggerExitCoroutine(Collider other)
    {
        TrafficLightPoints tl = other.GetComponent<TrafficLightPoints>();
        if (tl != null && tl == trafficLightInRange)
        {
            trafficLightInRange = null;
            yield return null; // Wait a frame for state to update
        }
        
        // Handle collision avoidance exit
        CarAI otherCarAI = other.GetComponent<CarAI>();
        if (otherCarAI != null && otherCarAI != this && carInFront == other.gameObject)
        {
            if (isAvoidingCollision)
            {
                isAvoidingCollision = false;
                carInFront = null;
                
                // Only resume if not stopped by traffic light
                if (trafficLightInRange == null || trafficLightInRange.CanGo())
                {
                    agent.isStopped = false;
                    Debug.Log($"CarAI on {gameObject.name}: Resuming movement, car exited trigger zone");
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
    
    // Public methods to get collision avoidance status
    public bool IsAvoidingCollision()
    {
        return isAvoidingCollision;
    }
    
    public GameObject GetCarInFront()
    {
        return carInFront;
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
        
        // Draw trigger zone for collision detection (assumes trigger extends forward from car)
        Gizmos.color = isAvoidingCollision ? Color.red : Color.cyan;
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider != null && triggerCollider.isTrigger)
        {
            // Draw the trigger bounds
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(triggerCollider.bounds.center - transform.position, triggerCollider.bounds.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
        else
        {
            // Fallback: draw estimated trigger zone based on safe following distance
            Vector3 triggerCenter = transform.position + transform.forward * (safeFollowingDistance * 0.5f);
            Vector3 triggerSize = new Vector3(2f, 1f, safeFollowingDistance);
            Gizmos.DrawWireCube(triggerCenter, triggerSize);
        }
        
        // Draw safe following distance reference
        Gizmos.color = new Color(1f, 0.5f, 0f); // Orange color
        Gizmos.DrawWireSphere(transform.position + transform.forward * safeFollowingDistance, 1f);
        
        // Draw coroutine status
        if (Application.isPlaying)
        {
            Vector3 labelPos = transform.position + Vector3.up * 3f;
            string status = "Coroutines: ";
            status += (rotationCoroutine != null) ? "R✓ " : "R✗ ";
            status += (trafficLightCoroutine != null) ? "T✓ " : "T✗ ";
            status += (waypointNavigationCoroutine != null) ? "W✓ " : "W✗ ";
            
            if (isAvoidingCollision && carInFront != null)
            {
                status += $"\nAvoiding: {carInFront.name} (Trigger)";
            }
            
            UnityEditor.Handles.Label(labelPos, status);
        }
    }
#endif
}
