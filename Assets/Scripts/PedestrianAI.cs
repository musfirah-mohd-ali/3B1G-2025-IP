using UnityEngine;
using UnityEngine.AI;

public class PedestrianAI : MonoBehaviour
{
    [Header("Waypoint Navigation")]
    [SerializeField] public Transform[] waypoints; // Manually assigned waypoint transforms (empty GameObjects) - public for spawner access
    
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
        
        // Get waypoints from manually assigned transforms
        if (waypoints != null && waypoints.Length > 0)
        {
            GoToNextWaypoint();
        }
        else
        {
            Debug.LogWarning($"{gameObject.name}: No waypoints assigned! Please assign waypoint transforms to the waypoints array.");
        }
    }
    
    void Update()
    {
        // Early exit if no agent or not on NavMesh
        if (agent == null || !agent.isOnNavMesh)
            return;
        
        // Early exit if no waypoints assigned
        if (waypoints == null || waypoints.Length == 0)
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
    }

    void GoToNextWaypoint()
    {
        
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError($"{gameObject.name}: No waypoints available!");
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
    
    // Method to reinitialize pedestrian with new waypoints (used by spawner)
    public void InitializeWithWaypoints(Transform[] newWaypoints)
    {
        if (newWaypoints != null && newWaypoints.Length > 0)
        {
            // Validate all waypoints are not null
            for (int i = 0; i < newWaypoints.Length; i++)
            {
                if (newWaypoints[i] == null)
                {
                    Debug.LogError($"{gameObject.name}: Waypoint {i} in array is NULL!");
                    return;
                }
            }
            
            waypoints = newWaypoints;
            
            // Reset movement state
            currentWaypointIndex = 0;
            isWaiting = false;
            waitTimer = 0f;
            
            // Start movement if we have an agent and are on NavMesh
            if (agent != null && agent.isOnNavMesh && waypoints.Length > 0)
            {
                GoToNextWaypoint();
            }
            else
            {
                Debug.LogWarning($"{gameObject.name}: Cannot start movement - check NavMesh and waypoints");
            }
        }
        else
        {
            Debug.LogError($"{gameObject.name}: InitializeWithWaypoints called with null or empty waypoints array");
        }
    }
}
