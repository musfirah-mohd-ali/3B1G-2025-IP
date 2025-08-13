using UnityEngine;
using UnityEngine.AI;

public class CarAI : MonoBehaviour
{
    [Header("Settings")]
    public float normalSpeed = 10f;       // Speed when green
    public float slowSpeed = 3f;          // Speed when slowing (red/yellow)
    public float stopDistance = 5f;       // Distance from traffic light to start slowing
    public float rotationOffset = 0f;
    public float brakingSmooth = 5f;      // How fast the car slows down

    private NavMeshAgent agent;
    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    
    private TrafficLightPoints trafficLightInRange;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        agent.speed = normalSpeed;
        agent.acceleration = 8f;
        agent.autoBraking = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;

        // Get waypoints from Waypoints manager
        Waypoints waypointsManager = FindObjectOfType<Waypoints>();
        if (waypointsManager?.points != null && waypointsManager.points.Length > 0)
        {
            waypoints = waypointsManager.points;
            GoToNextWaypoint();
        }
    }

    void Update()
    {
        // Rotate towards destination
        if (agent.isOnNavMesh && agent.hasPath && agent.velocity.sqrMagnitude > 0.01f)
        {
            Vector3 direction = agent.steeringTarget - transform.position;
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, rotationOffset, 0);
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 5f);
            }
        }

        // Waypoint navigation
        if (agent.isOnNavMesh && !agent.pathPending && agent.remainingDistance < 2f)
            GoToNextWaypoint();

        // Traffic light handling
        HandleTrafficLight();
    }

    void GoToNextWaypoint()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        if (!agent.isOnNavMesh) return;

        agent.SetDestination(waypoints[currentWaypointIndex].position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }

    void HandleTrafficLight()
    {
        if (!agent.isOnNavMesh) return; // Skip if agent isn't on NavMesh yet

        if (trafficLightInRange != null)
        {
            if (trafficLightInRange.CanGo())
                agent.isStopped = false; // green
            else
                agent.isStopped = true;  // red/yellow
        }
        else
        {
            agent.isStopped = false;     // no light, move normally
        }
    }



    void OnTriggerEnter(Collider other)
    {
        Debug.Log("CarAI: Trigger Enter with " + other.name);
        // Detect traffic light
        TrafficLightPoints tl = other.GetComponent<TrafficLightPoints>();
        if (tl != null)
        {
            trafficLightInRange = tl;
        }
    }

    void OnTriggerExit(Collider other)
    {
        TrafficLightPoints tl = other.GetComponent<TrafficLightPoints>();
        if (tl != null && tl == trafficLightInRange)
        {
            trafficLightInRange = null;
        }
    }
}
