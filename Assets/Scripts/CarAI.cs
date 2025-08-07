using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CarAI : MonoBehaviour
{
    [Header("Waypoint Navigation")]
    public Transform[] waypoints;
    
    [Header("Settings")]
    public float turnSpeed = 5f;
    public float rotationOffset = -90f; // Adjust if car faces wrong direction
    public float normalSpeed = 10f;
    public float waypointCheckInterval = 0.1f; // How often to check waypoints
    
    [Header("Collision Avoidance")]
    public float avoidanceDistance = 5f; // Distance to maintain from other cars
    public float slowSpeed = 1f; // Speed when avoiding collision
    public float avoidanceCheckInterval = 0.05f; // How often to check for collisions
    
    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private Coroutine navigationCoroutine;
    private Coroutine rotationCoroutine;
    private Coroutine avoidanceCoroutine;
    private GameObject nearbyCarObject;
    private bool isAvoiding = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.speed = normalSpeed;
        
        if (waypoints.Length > 0)
        {
            // Start coroutines for navigation and rotation
            navigationCoroutine = StartCoroutine(NavigationLoop());
            rotationCoroutine = StartCoroutine(RotationLoop());
            avoidanceCoroutine = StartCoroutine(AvoidanceLoop());
            GoToNextWaypoint();
        }
    }

    void OnDestroy()
    {
        // Clean up coroutines when object is destroyed
        if (navigationCoroutine != null)
            StopCoroutine(navigationCoroutine);
        if (rotationCoroutine != null)
            StopCoroutine(rotationCoroutine);
        if (avoidanceCoroutine != null)
            StopCoroutine(avoidanceCoroutine);
    }

    IEnumerator NavigationLoop()
    {
        while (true)
        {
            // Check if waypoint is reached
            if (!agent.pathPending && agent.remainingDistance < 1f)
            {
                yield return StartCoroutine(WaypointReachedDelay());
                GoToNextWaypoint();
            }
            
            yield return new WaitForSeconds(waypointCheckInterval);
        }
    }

    IEnumerator RotationLoop()
    {
        while (true)
        {
            RotateTowardsTarget();
            yield return null; // Wait one frame
        }
    }

    IEnumerator WaypointReachedDelay()
    {
        // Small delay when reaching waypoint for more realistic behavior
        yield return new WaitForSeconds(0.1f);
    }

    IEnumerator AvoidanceLoop()
    {
        while (true)
        {
            if (nearbyCarObject != null)
            {
                float distanceToNearby = Vector3.Distance(transform.position, nearbyCarObject.transform.position);
                
                if (distanceToNearby < avoidanceDistance)
                {
                    // Too close - maintain distance
                    MaintainDistance(nearbyCarObject);
                }
                else if (distanceToNearby > avoidanceDistance * 1.5f)
                {
                    // Far enough - resume normal behavior
                    ResumeNormalBehavior();
                }
            }
            else if (isAvoiding)
            {
                // No nearby car but still avoiding - resume normal behavior
                ResumeNormalBehavior();
            }
            
            yield return new WaitForSeconds(avoidanceCheckInterval);
        }
    }

    void MaintainDistance(GameObject otherCar)
    {
        if (!isAvoiding)
        {
            isAvoiding = true;
            Debug.Log("Maintaining distance from nearby car");
        }
        
        Vector3 directionAway = (transform.position - otherCar.transform.position).normalized;
        float currentDistance = Vector3.Distance(transform.position, otherCar.transform.position);
        
        if (currentDistance < avoidanceDistance)
        {
            // Too close - slow down or stop
            agent.speed = slowSpeed * 0.5f; // Even slower when too close
            
            // Optionally move slightly away
            Vector3 avoidancePosition = transform.position + directionAway * 2f;
            agent.SetDestination(avoidancePosition);
        }
        else
        {
            // At good distance - maintain slow speed
            agent.speed = slowSpeed;
        }
    }

    void ResumeNormalBehavior()
    {
        if (isAvoiding)
        {
            isAvoiding = false;
            agent.speed = normalSpeed;
            Debug.Log("Resuming normal navigation");
            
            // Resume normal waypoint navigation
            GoToNextWaypoint();
        }
    }

    void Update()
    {
        // Keep Update empty since we're using coroutines
        // This can be used for other non-coroutine logic if needed
    }

    void RotateTowardsTarget()
    {
        if (agent.destination == Vector3.zero) return;
        
        Vector3 direction = (agent.destination - transform.position).normalized;
        if (direction.magnitude < 0.1f) return;
        
        Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, rotationOffset, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        agent.SetDestination(waypoints[currentWaypointIndex].position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    // Trigger detection methods
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car") && other.gameObject != gameObject)
        {
            nearbyCarObject = other.gameObject;
            Debug.Log("Car entered detection zone: " + other.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car") && other.gameObject == nearbyCarObject)
        {
            nearbyCarObject = null;
            Debug.Log("Car left detection zone: " + other.name);
        }
    }

    // Public method to stop all coroutines (useful for pausing AI)
    public void StopAI()
    {
        if (navigationCoroutine != null)
        {
            StopCoroutine(navigationCoroutine);
            navigationCoroutine = null;
        }
        if (rotationCoroutine != null)
        {
            StopCoroutine(rotationCoroutine);
            rotationCoroutine = null;
        }
        if (avoidanceCoroutine != null)
        {
            StopCoroutine(avoidanceCoroutine);
            avoidanceCoroutine = null;
        }
    }

    // Public method to restart AI coroutines
    public void StartAI()
    {
        if (waypoints.Length > 0)
        {
            if (navigationCoroutine == null)
                navigationCoroutine = StartCoroutine(NavigationLoop());
            if (rotationCoroutine == null)
                rotationCoroutine = StartCoroutine(RotationLoop());
            if (avoidanceCoroutine == null)
                avoidanceCoroutine = StartCoroutine(AvoidanceLoop());
        }
    }
} 
