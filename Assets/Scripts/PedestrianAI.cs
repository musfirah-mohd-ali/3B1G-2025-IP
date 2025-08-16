using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class PedestrianAI : MonoBehaviour
{
    [Header("Movement & Speed")]
    public float walkSpeed = 3.5f;
    public float runSpeed = 6f;
    public float waypointReachDistance = 1f;
    public float waitTimeAtWaypoint = 2f;
    
    [Header("Behavior Settings")]
    public bool randomWaypoints = false;
    public bool shouldWaitAtWaypoints = true;
    
    [Header("Performance")]
    public float updateInterval = 0.1f;
    
    [Header("Waypoints")]
    [SerializeField] public Transform[] waypoints; // Public for spawner access

    // Core components
    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    
    // Coroutine management
    private Coroutine navigationCoroutine;

    void Start() => StartCoroutine(Initialize());

    IEnumerator Initialize()
    {
        agent = GetComponent<NavMeshAgent>();
        
        if (!ValidateSetup()) yield break;
        
        SetupAgent();
        
        if (waypoints?.Length > 0)
            StartNavigation();
        else
            Debug.LogWarning($"{name}: No waypoints assigned!");
    }

    bool ValidateSetup()
    {
        if (agent == null)
        {
            Debug.LogError($"{name}: No NavMeshAgent component found!");
            return false;
        }
        
        if (!agent.isOnNavMesh)
        {
            Debug.LogError($"{name}: Not on NavMesh!");
            return false;
        }
        
        return true;
    }

    void SetupAgent()
    {
        agent.updateRotation = true;
        agent.speed = walkSpeed;
        agent.stoppingDistance = 0.2f;
        agent.angularSpeed = 120f;
        agent.acceleration = 8f;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
    }
    
    void StartNavigation()
    {
        if (navigationCoroutine != null) StopCoroutine(navigationCoroutine);
        navigationCoroutine = StartCoroutine(NavigationLoop());
    }

    void StopNavigation()
    {
        if (navigationCoroutine != null)
        {
            StopCoroutine(navigationCoroutine);
            navigationCoroutine = null;
        }
    }

    IEnumerator NavigationLoop()
    {
        var wait = new WaitForSeconds(updateInterval);
        
        while (agent?.isOnNavMesh == true && waypoints?.Length > 0)
        {
            if (!agent.pathPending && agent.remainingDistance < waypointReachDistance)
            {
                if (shouldWaitAtWaypoints)
                    yield return new WaitForSeconds(waitTimeAtWaypoint);
                
                yield return GoToNextWaypoint();
            }
            
            yield return wait;
        }
    }

    IEnumerator GoToNextWaypoint()
    {
        if (waypoints?.Length == 0 || agent?.isOnNavMesh != true) yield break;

        currentWaypointIndex = GetNextWaypointIndex();
        
        if (waypoints[currentWaypointIndex] == null)
        {
            Debug.LogError($"{name}: Waypoint {currentWaypointIndex} is null!");
            yield break;
        }

        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        
        if (IsValidNavMeshPosition(targetPosition, out Vector3 validPosition))
        {
            agent.SetDestination(validPosition);
        }
        else
        {
            Debug.LogError($"{name}: Waypoint {currentWaypointIndex} not on NavMesh!");
            if (waypoints.Length > 1) yield return GoToNextWaypoint();
        }
    }

    int GetNextWaypointIndex()
    {
        if (randomWaypoints && waypoints.Length > 1)
        {
            int newIndex;
            do { newIndex = Random.Range(0, waypoints.Length); }
            while (newIndex == currentWaypointIndex);
            return newIndex;
        }
        
        return (currentWaypointIndex + 1) % waypoints.Length;
    }

    bool IsValidNavMeshPosition(Vector3 position, out Vector3 validPosition)
    {
        return NavMesh.SamplePosition(position, out NavMeshHit hit, 2f, NavMesh.AllAreas) 
            ? (validPosition = hit.position) != Vector3.zero : (validPosition = Vector3.zero) == Vector3.zero;
    }
    
    // Public API
    public void SetWalkSpeed() => agent.speed = walkSpeed;
    public void SetRunSpeed() => agent.speed = runSpeed;
    public void StopMovement() => agent.isStopped = true;
    public void ResumeMovement() => agent.isStopped = false;
    
    public void InitializeWithWaypoints(Transform[] newWaypoints)
    {
        if (newWaypoints?.Length == 0)
        {
            Debug.LogError($"{name}: Invalid waypoints array!");
            return;
        }
        
        if (!ValidateWaypoints(newWaypoints)) return;
        
        StopNavigation();
        waypoints = newWaypoints;
        currentWaypointIndex = 0;
        
        if (agent?.isOnNavMesh == true)
            StartNavigation();
        else
            Debug.LogWarning($"{name}: Cannot start - check NavMesh!");
    }

    bool ValidateWaypoints(Transform[] waypointsToValidate)
    {
        for (int i = 0; i < waypointsToValidate.Length; i++)
        {
            if (waypointsToValidate[i] == null)
            {
                Debug.LogError($"{name}: Waypoint {i} is NULL!");
                return false;
            }
        }
        return true;
    }

    // Lifecycle
    void OnDestroy() => StopNavigation();
    void OnDisable() => StopNavigation();
    void OnEnable() { if (waypoints?.Length > 0 && agent?.isOnNavMesh == true) StartNavigation(); }
}
