using UnityEngine;
using UnityEngine.AI;

public class CarAI : MonoBehaviour
{
    [Header("Waypoint Navigation")]
    public Waypoints waypointsManager; // Reference to the Waypoints component
    private GameObject[] waypointObjects; // Will be populated from Waypoints component
    
    [Header("Settings")]
    public float normalSpeed = 10f;
    public float slowSpeed = 3f; // Speed when near other cars
    public float waypointReachDistance = 2f; // Distance to consider waypoint reached
    public float rotationOffset = 0f; // Rotation offset in degrees (adjust if car faces wrong direction)
    
    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private bool isSlowingDown = false;
    private int carsInTrigger = 0; // Count of cars in trigger zone

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Configure NavMeshAgent to handle all movement physics
        agent.updateRotation = false; // We'll handle rotation manually for offset
        agent.updatePosition = true; // Let NavMeshAgent handle position
        agent.speed = normalSpeed;
        agent.acceleration = 8f; // Realistic acceleration
        agent.angularSpeed = 120f; // Realistic turning
        agent.stoppingDistance = 0.5f;
        agent.autoBraking = true; // Automatic deceleration
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        
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
        // Handle rotation towards destination with offset
        if (agent.hasPath && agent.velocity.magnitude > 0.1f)
        {
            Vector3 direction = agent.steeringTarget - transform.position;
            if (direction.magnitude > 0.1f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                targetRotation *= Quaternion.Euler(0, rotationOffset, 0); // Apply offset
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        
        // Check if we've reached the current waypoint
        if (!agent.pathPending && agent.remainingDistance < waypointReachDistance)
        {
            GoToNextWaypoint();
        }
    }

    void GoToNextWaypoint()
    {
        if (waypointObjects == null || waypointObjects.Length == 0) return;

        agent.SetDestination(waypointObjects[currentWaypointIndex].transform.position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypointObjects.Length;
    }

    // Trigger detection methods - slow down when cars enter trigger
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car") && other.gameObject != gameObject)
        {
            carsInTrigger++;
            
            if (!isSlowingDown)
            {
                isSlowingDown = true;
                agent.speed = slowSpeed;
                Debug.Log("Slowing down - car entered detection zone: " + other.name);
            }
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car") && other.gameObject != gameObject)
        {
            carsInTrigger--;
            
            // Only resume normal speed if no cars left in trigger
            if (carsInTrigger <= 0 && isSlowingDown)
            {
                carsInTrigger = 0; // Ensure it doesn't go negative
                isSlowingDown = false;
                agent.speed = normalSpeed;
                Debug.Log("Resuming normal speed - car left detection zone: " + other.name);
            }
        }
    }
} 
