using UnityEngine;
using UnityEngine.AI;

public class CarAI : MonoBehaviour
{
    [Header("Waypoint Navigation")]
    public Transform[] waypoints;
    
    [Header("Settings")]
    public float turnSpeed = 5f;
    public float rotationOffset = -90f; // Adjust if car faces wrong direction
    
    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.updateRotation = false;
        
        if (waypoints.Length > 0)
            GoToNextWaypoint();
    }

    void Update()
    {
        RotateTowardsTarget();
        CheckWaypointReached();
    }

    void RotateTowardsTarget()
    {
        if (agent.destination == Vector3.zero) return;
        
        Vector3 direction = (agent.destination - transform.position).normalized;
        if (direction.magnitude < 0.1f) return;
        
        Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, rotationOffset, 0);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * turnSpeed);
    }

    void CheckWaypointReached()
    {
        if (!agent.pathPending && agent.remainingDistance < 1f)
            GoToNextWaypoint();
    }

    void GoToNextWaypoint()
    {
        if (waypoints.Length == 0) return;

        agent.SetDestination(waypoints[currentWaypointIndex].position);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
    }
}
