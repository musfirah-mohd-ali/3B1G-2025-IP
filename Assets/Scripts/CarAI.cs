using UnityEngine;
using UnityEngine.AI;

public class CarAI : MonoBehaviour
{
    [Header("Waypoint Navigation")]
    public Transform[] waypoints;
    
    [Header("Settings")]
    public float turnSpeed = 5f;
    public float rotationOffset = -90f; // Adjust if car faces wrong direction
    
    [Header("Physics")]
    public float frictionForce = 50f;    // Increased from 15f
    public float rollingResistance = 20f; // Increased from 5f
    public float maxSpeed = 20f;
    public float stopThreshold = 2f;     // Distance to start slowing down
    
    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private Rigidbody rb;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        rb = GetComponent<Rigidbody>();
        
        agent.updateRotation = false;
        agent.speed = maxSpeed;
        
        if (waypoints.Length > 0)
            GoToNextWaypoint();
    }

    void Update()
    {
        RotateTowardsTarget();
        CheckWaypointReached();
    }

    void FixedUpdate()
    {
        ApplyFriction();
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

    void ApplyFriction()
    {
        if (rb == null) return;
        
        Vector3 velocity = rb.linearVelocity;
        float currentSpeed = velocity.magnitude;
        
        // Strong braking when approaching waypoint
        if (agent.remainingDistance < stopThreshold && currentSpeed > 1f)
        {
            Vector3 strongBrake = -velocity.normalized * (frictionForce * 3f);
            rb.AddForce(strongBrake, ForceMode.Acceleration);
        }
        
        // Apply rolling resistance (constant deceleration)
        if (currentSpeed > 0.1f)
        {
            Vector3 rollingForce = -velocity.normalized * rollingResistance;
            rb.AddForce(rollingForce, ForceMode.Acceleration);
        }
        
        // Apply velocity-based friction (increases with speed)
        if (currentSpeed > 0.1f)
        {
            Vector3 frictionDirection = -velocity.normalized;
            float frictionMagnitude = frictionForce * currentSpeed * 0.2f; // Increased multiplier
            Vector3 totalFriction = frictionDirection * frictionMagnitude;
            rb.AddForce(totalFriction, ForceMode.Acceleration);
        }
        
        // Limit max speed
        if (currentSpeed > maxSpeed)
        {
            rb.linearVelocity = velocity.normalized * maxSpeed;
        }
        
        // Complete stop when very close to waypoint
        if (currentSpeed < 1f && agent.remainingDistance < 0.5f)
        {
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
    }
}
