using UnityEngine;
using UnityEngine.AI;

public class PedestrianAI : MonoBehaviour
{
    [Header("Waypoint Navigation")]
    public Waypoints waypointsManager; // Reference to the Waypoints component
    private GameObject[] waypointObjects; // Will be populated from Waypoints component
    
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
        
        // Configure NavMeshAgent for pedestrian movement
        agent.updateRotation = true; // Let NavMeshAgent handle rotation
        agent.speed = walkSpeed;
        agent.stoppingDistance = 0.2f;
        agent.angularSpeed = 120f; // Slower turning for realistic pedestrian movement
        
        // Get waypoints from Waypoints component
        SetupWaypoints();
        
        if (waypointObjects != null && waypointObjects.Length > 0)
        {
            GoToNextWaypoint();
        }
        else
        {
            Debug.LogWarning("No waypoints found for " + gameObject.name + ". Make sure Waypoints component is assigned and has waypoints.");
        }
    }
    
    void SetupWaypoints()
    {
        // If no waypoints manager is assigned, try to find one in the scene
        if (waypointsManager == null)
        {
            waypointsManager = FindObjectOfType<Waypoints>();
        }
        
        if (waypointsManager != null && waypointsManager.points != null && waypointsManager.points.Length > 0)
        {
            // Convert Transform array to GameObject array
            waypointObjects = new GameObject[waypointsManager.points.Length];
            for (int i = 0; i < waypointsManager.points.Length; i++)
            {
                if (waypointsManager.points[i] != null)
                {
                    waypointObjects[i] = waypointsManager.points[i].gameObject;
                }
            }
            Debug.Log($"{gameObject.name} found {waypointObjects.Length} waypoints from Waypoints component");
        }
        else
        {
            Debug.LogError("Waypoints component not found or has no waypoints assigned!");
        }
    }

    void Update()
    {
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
        if (waypointObjects == null || waypointObjects.Length == 0) return;

        if (randomWaypoints)
        {
            // Choose a random waypoint (but not the current one)
            int newIndex;
            do
            {
                newIndex = Random.Range(0, waypointObjects.Length);
            } while (newIndex == currentWaypointIndex && waypointObjects.Length > 1);
            
            currentWaypointIndex = newIndex;
        }
        else
        {
            // Go to next waypoint in sequence
            currentWaypointIndex = (currentWaypointIndex + 1) % waypointObjects.Length;
        }
        
        if (waypointObjects[currentWaypointIndex] != null)
        {
            agent.SetDestination(waypointObjects[currentWaypointIndex].transform.position);
            Debug.Log($"{gameObject.name} walking to waypoint {currentWaypointIndex}");
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
