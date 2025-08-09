using UnityEngine;
using UnityEngine.AI;

public class CarAI : MonoBehaviour
{
    [Header("Settings")]
    public float normalSpeed = 10f;
    public float slowSpeed = 3f;
    public float rotationOffset = 0f;
    
    private NavMeshAgent agent;
    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private bool isSlowingDown = false;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.speed = normalSpeed;
        agent.acceleration = 8f;
        agent.autoBraking = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
        
        // Get waypoints
        Waypoints waypointsManager = FindObjectOfType<Waypoints>();
        if (waypointsManager?.points != null && waypointsManager.points.Length > 0)
        {
            waypoints = waypointsManager.points;
            GoToNextWaypoint();
        }
    }

    void Update()
    {
        // Rotate towards destination - only if agent is active and on NavMesh
        if (agent.isOnNavMesh && agent.hasPath && agent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 direction = agent.steeringTarget - transform.position;
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, rotationOffset, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }
        
        // Check waypoint reached - only if agent is properly initialized
        if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance < 2f)
            GoToNextWaypoint();
    }

    void GoToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        if (!agent.isOnNavMesh) return; // Safety check
        
        agent.SetDestination(waypoints[currentWaypointIndex].position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Car") && !isSlowingDown)
        {
            isSlowingDown = true;
            agent.speed = slowSpeed;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Car") && isSlowingDown)
        {
            isSlowingDown = false;
            agent.speed = normalSpeed;
        }
    }
} 
