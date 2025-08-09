using UnityEngine;
using UnityEngine.AI;

public class CarAI : MonoBehaviour
{
    [Header("Waypoint Navigation")]
    public GameObject[] waypointObjects;
    
    [Header("Settings")]
    public float normalSpeed = 10f;
    public float slowSpeed = 3f; // Speed when near other cars
    public float waypointReachDistance = 2f; // Distance to consider waypoint reached
    
    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private bool isSlowingDown = false;
    private int carsInTrigger = 0; // Count of cars in trigger zone

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        // Configure NavMeshAgent to handle all movement and rotation
        agent.updateRotation = true; // Let NavMeshAgent handle rotation
        agent.speed = normalSpeed;
        agent.stoppingDistance = 0.5f;
        
        if (waypointObjects.Length > 0)
        {
            GoToNextWaypoint();
        }
    }

    void Update()
    {
        // Check if we've reached the current waypoint
        if (!agent.pathPending && agent.remainingDistance < waypointReachDistance)
        {
            GoToNextWaypoint();
        }
    }

    void GoToNextWaypoint()
    {
        if (waypointObjects.Length == 0) return;

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
