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
            Debug.Log($"{gameObject.name}: Starting movement with {waypoints.Length} waypoints");
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
        
        // Debug: Show current state
        if (Time.frameCount % 60 == 0) // Every 60 frames (about once per second)
        {
            Debug.Log($"{gameObject.name}: Speed={agent.velocity.magnitude:F2}, Target={currentWaypointIndex}, Distance={agent.remainingDistance:F2}, PathStatus={agent.pathStatus}");
        }
    }

    void GoToNextWaypoint()
    {
        Debug.Log($"🔍 GOTO DEBUG: GoToNextWaypoint called on {gameObject.name}");
        
        if (waypoints == null || waypoints.Length == 0)
        {
            Debug.LogError($"❌ GOTO DEBUG: {gameObject.name}: No waypoints available! waypoints null: {waypoints == null}, length: {waypoints?.Length ?? 0}");
            return;
        }
        
        if (agent == null || !agent.isOnNavMesh)
        {
            Debug.LogError($"❌ GOTO DEBUG: {gameObject.name}: NavMeshAgent is null or not on NavMesh! Agent null: {agent == null}, On NavMesh: {agent?.isOnNavMesh}");
            return;
        }

        Debug.Log($"🔍 GOTO DEBUG: {gameObject.name} has {waypoints.Length} waypoints, current index: {currentWaypointIndex}");

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
        
        Debug.Log($"🔍 GOTO DEBUG: {gameObject.name} targeting waypoint {currentWaypointIndex}");
        
        if (waypoints[currentWaypointIndex] != null)
        {
            Vector3 targetPosition = waypoints[currentWaypointIndex].position;
            Debug.Log($"🔍 GOTO DEBUG: Target position: {targetPosition}");
            
            // Check if the target position is valid on NavMesh
            NavMeshHit hit;
            if (NavMesh.SamplePosition(targetPosition, out hit, 2f, NavMesh.AllAreas))
            {
                agent.SetDestination(hit.position);
                Debug.Log($"✅ GOTO SUCCESS: {gameObject.name} walking to waypoint {currentWaypointIndex} at {hit.position}");
            }
            else
            {
                Debug.LogError($"❌ GOTO ERROR: {gameObject.name}: Waypoint {currentWaypointIndex} at {targetPosition} is not on NavMesh!");
                // Try next waypoint
                if (waypoints.Length > 1)
                {
                    GoToNextWaypoint();
                }
            }
        }
        else
        {
            Debug.LogError($"❌ GOTO ERROR: {gameObject.name}: Waypoint {currentWaypointIndex} is null!");
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
    
    // Method to reinitialize pedestrian with new waypoints (used by spawner)
    public void InitializeWithWaypoints(Transform[] newWaypoints)
    {
        Debug.Log($"🔍 INIT DEBUG: InitializeWithWaypoints called on {gameObject.name} with {(newWaypoints?.Length ?? 0)} waypoints");
        
        if (newWaypoints != null && newWaypoints.Length > 0)
        {
            // Validate all waypoints are not null
            for (int i = 0; i < newWaypoints.Length; i++)
            {
                if (newWaypoints[i] == null)
                {
                    Debug.LogError($"❌ INIT DEBUG: Waypoint {i} in array is NULL!");
                    return;
                }
            }
            
            waypoints = newWaypoints;
            Debug.Log($"🔍 INIT DEBUG: Waypoints array assigned. Length: {waypoints.Length}");
            
            // Reset movement state
            currentWaypointIndex = 0;
            isWaiting = false;
            waitTimer = 0f;
            
            Debug.Log($"🔍 INIT DEBUG: Movement state reset. Agent null? {agent == null}, Agent on NavMesh? {agent?.isOnNavMesh}");
            
            // Start movement if we have an agent and are on NavMesh
            if (agent != null && agent.isOnNavMesh && waypoints.Length > 0)
            {
                Debug.Log($"🔍 INIT DEBUG: Starting movement to first waypoint");
                GoToNextWaypoint();
            }
            else
            {
                Debug.LogWarning($"⚠️ INIT DEBUG: Cannot start movement - Agent: {agent != null}, OnNavMesh: {agent?.isOnNavMesh}, Waypoints: {waypoints.Length}");
            }
        }
        else
        {
            Debug.LogError($"❌ INIT DEBUG: {gameObject.name}: InitializeWithWaypoints called with null or empty waypoints array");
        }
    }
}
