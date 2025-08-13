using UnityEngine;
using UnityEngine.AI;

public class PedestrianAI : MonoBehaviour
{
    [Header("Waypoint Navigation")]
    public Transform[] waypoints; // Manually assigned waypoint transforms (empty GameObjects)
    public bool autoFindWaypointsIfEmpty = true; // Fallback to find Waypoints component if no manual waypoints assigned
    
    [Header("Movement Settings")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6f;
    public float waypointReachDistance = 1f; // Distance to consider waypoint reached
    public float waitTimeAtWaypoint = 2f; // Time to wait at each waypoint
    
    [Header("Behavior Settings")]
    public bool randomWaypoints = false; // If true, chooses random waypoints instead of sequential
    public bool shouldWaitAtWaypoints = true; // If true, pauses at waypoints
    
    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;
    private float waitTimer = 0f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Validate NavMeshAgent component
        if (agent == null)
        {
            Debug.LogError($"{gameObject.name}: No NavMeshAgent component found! Please add NavMeshAgent to the pedestrian prefab.");
            return;
        }
        
        // Configure NavMeshAgent for pedestrian movement
        agent.updateRotation = true; // Let NavMeshAgent handle rotation
        agent.speed = walkSpeed;
        agent.stoppingDistance = 0.2f;
        agent.angularSpeed = 120f; // Slower turning for realistic pedestrian movement
        agent.acceleration = 8f; // Default acceleration
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
        
        // Check if on NavMesh
        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"{gameObject.name}: Pedestrian is not on NavMesh! Make sure the spawn position is on a NavMesh surface.");
            return;
        }
        
        // Get waypoints - either manually assigned or from Waypoints component
        SetupWaypoints();
        
        if (waypoints != null && waypoints.Length > 0)
        {
            Debug.Log($"{gameObject.name}: Starting movement with {waypoints.Length} waypoints");
            GoToNextWaypoint();
        }
        else
        {
            Debug.LogWarning("No waypoints found for " + gameObject.name + ". Make sure waypoints are manually assigned or Waypoints component exists in scene.");
        }
    }
    
    void SetupWaypoints()
    {
        // If waypoints are manually assigned, use them directly
        if (waypoints != null && waypoints.Length > 0)
        {
            Debug.Log($"{gameObject.name} using {waypoints.Length} manually assigned waypoints");
            return;
        }
        
        // Fallback to Waypoints component if auto-find is enabled and no manual waypoints
        if (autoFindWaypointsIfEmpty)
        {
            Waypoints waypointsManager = FindObjectOfType<Waypoints>();
            if (waypointsManager != null && waypointsManager.points != null && waypointsManager.points.Length > 0)
            {
                waypoints = waypointsManager.points;
                Debug.Log($"{gameObject.name} using {waypoints.Length} waypoints from Waypoints component as fallback");
            }
            else
            {
                Debug.LogError($"{gameObject.name}: No manual waypoints assigned and no Waypoints component found!");
            }
        }
        else
        {
            Debug.LogError($"{gameObject.name}: No waypoints assigned! Please assign waypoint transforms manually.");
        }
    }

    void Update()
    {
        // Early exit if no agent or not on NavMesh
        if (agent == null || !agent.isOnNavMesh)
            return;
            
        // Handle waiting at waypoints
        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;
            if (waitTimer <= 0f)
            {
                isWaiting = false;
                GoToNextWaypoint();
            }
            return;
        }
        
        // Check if we've reached the current waypoint
        if (!agent.pathPending && agent.remainingDistance < waypointReachDistance)
        {
            if (shouldWaitAtWaypoints)
            {
                StartWaiting();
            }
            else
            {
                GoToNextWaypoint();
            }
        }
        
        // Debug: Show current state
        if (Time.frameCount % 60 == 0) // Every 60 frames (about once per second)
        {
            Debug.Log($"{gameObject.name}: Speed={agent.velocity.magnitude:F2}, Target={currentWaypointIndex}, Distance={agent.remainingDistance:F2}, PathStatus={agent.pathStatus}");
        }
    }

    void GoToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogWarning($"{gameObject.name}: No waypoints available!");
            return;
        }
        
        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogError($"{gameObject.name}: NavMeshAgent is null or not on NavMesh!");
            return;
        }

        if (randomWaypoints)
        {
            // Choose a random waypoint (but not the current one)
            int newIndex;
            do
            {
                newIndex = Random.Range(0, waypoints.Length);
            } while (newIndex == currentWaypointIndex && waypoints.Length > 1);
            
            currentWaypointIndex = newIndex;
        }
        else
        {
            // Go to next waypoint in sequence
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        }
        
        if (waypoints[currentWaypointIndex] != null)
        {
            Vector3 targetPosition = waypoints[currentWaypointIndex].position;
            
            // Check if the target position is valid on NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                Debug.Log($"{gameObject.name} walking to waypoint {currentWaypointIndex} at {hit.position}");
            }
            else
            {
                Debug.LogError($"{gameObject.name}: Waypoint {currentWaypointIndex} at {targetPosition} is not on NavMesh!");
                // Try next waypoint
                if (waypoints.Length > 1)
                {
                    GoToNextWaypoint();
                }
            }
        }
        else
        {
            Debug.LogError($"{gameObject.name}: Waypoint {currentWaypointIndex} is null!");
        }
    }
    
    void StartWaiting()
    {
        isWaiting = true;
        waitTimer = waitTimeAtWaypoint;
        Debug.Log($"{gameObject.name} waiting at waypoint {currentWaypointIndex}");
    }
    
    // Public methods to control pedestrian behavior
    public void SetWalkSpeed()
    {
        agent.speed = walkSpeed;
    }
    
    public void SetRunSpeed()
    {
        agent.speed = runSpeed;
    }
    
    public void StopMovement()
    {
        agent.isStopped = true;
    }
    
    public void ResumeMovement()
    {
        agent.isStopped = false;
    }
}
