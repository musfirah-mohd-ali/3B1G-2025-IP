using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class CarAI : MonoBehaviour
{
    [Header("Movement & Speed")]
    public float normalSpeed = 10f;
    public float slowSpeed = 3f;
    public float stopDistance = 5f;
    public float brakingSmooth = 5f;
    
    [Header("Rotation")]
    public float rotationOffset = 0f;
    public float rotationSpeed = 15f;
    public bool useInstantRotation = false;
    
    [Header("Waypoint Settings")]
    public float waypointSearchRadius = 50f;
    public bool startFromNearestWaypoint = true;
    
    [Header("Performance")]
    public float updateInterval = 0.1f;
    public float rotationUpdateInterval = 0.05f;
    public float trafficLightCheckInterval = 0.2f;

    // Core components
    private NavMeshAgent agent;
    private Transform[] waypoints;
    private int currentWaypointIndex = 0;
    private TrafficLightPoints trafficLightInRange;
    
    // Collision tracking
    private bool isObjectInTrigger = false;
    private int objectsInTriggerCount = 0;
    
    // Coroutine management
    private Coroutine[] coroutines = new Coroutine[3];

    void Start() => StartCoroutine(Initialize());
    
    IEnumerator Initialize()
    {
        agent = GetComponent<NavMeshAgent>();
        SetupAgent();
        
        yield return SetupWaypoints();
        StartAllCoroutines();
    }

    void SetupAgent()
    {
        agent.updateRotation = false;
        agent.speed = normalSpeed;
        agent.acceleration = 8f;
        agent.angularSpeed = 360f;
        agent.autoBraking = true;
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        
        isObjectInTrigger = false;
        objectsInTriggerCount = 0;
    }
    
    IEnumerator SetupWaypoints()
    {
        Waypoints waypointsManager = FindObjectOfType<Waypoints>();
        
        while (waypointsManager?.points == null || waypointsManager.points.Length == 0)
        {
            yield return new WaitForSeconds(0.5f);
            waypointsManager = FindObjectOfType<Waypoints>();
        }
        
        waypoints = waypointsManager.points;
        
        if (startFromNearestWaypoint)
            yield return FindNearestWaypoint();
        else
            currentWaypointIndex = 0;
        
        yield return GoToNextWaypoint();
    }
    
    void StartAllCoroutines()
    {
        StopAllCoroutines();
        coroutines[0] = StartCoroutine(RotationLoop());
        coroutines[1] = StartCoroutine(TrafficLightLoop());
        coroutines[2] = StartCoroutine(WaypointNavigationLoop());
    }
    
    void StopAllCoroutines()
    {
        for (int i = 0; i < coroutines.Length; i++)
        {
            if (coroutines[i] != null)
            {
                StopCoroutine(coroutines[i]);
                coroutines[i] = null;
            }
        }
    }
    
    IEnumerator RotationLoop()
    {
        var wait = new WaitForSeconds(rotationUpdateInterval);
        while (true)
        {
            if (agent?.isOnNavMesh == true && agent.hasPath && agent.velocity.sqrMagnitude > 0.01f)
            {
                Vector3 direction = agent.steeringTarget - transform.position;
                direction.y = 0;
                
                if (direction.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction) * Quaternion.Euler(0, rotationOffset, 0);
                    transform.rotation = useInstantRotation ? targetRotation : 
                        Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * rotationSpeed);
                }
            }
            yield return wait;
        }
    }

    IEnumerator WaypointNavigationLoop()
    {
        var wait = new WaitForSeconds(updateInterval);
        while (true)
        {
            if (agent?.isOnNavMesh == true && waypoints?.Length > 0)
            {
                if (!agent.pathPending && agent.remainingDistance < 2f)
                {
                    yield return GoToNextWaypoint();
                }
                else if (agent.velocity.sqrMagnitude < 0.1f && !agent.isStopped && agent.hasPath)
                {
                    yield return new WaitForSeconds(2f);
                    if (agent.velocity.sqrMagnitude < 0.1f && !agent.isStopped)
                        yield return GoToNextWaypoint();
                }
            }
            yield return wait;
        }
    }
    
    IEnumerator TrafficLightLoop()
    {
        var wait = new WaitForSeconds(trafficLightCheckInterval);
        while (true)
        {
            if (agent?.isOnNavMesh != true) { yield return wait; continue; }
            
            if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
            {
                yield return GoToNextWaypoint();
                continue;
            }

            bool shouldMove = DetermineMovement();
            HandleMovementDecision(shouldMove);
            
            yield return wait;
        }
    }

    bool DetermineMovement()
    {
        if (trafficLightInRange != null)
            return trafficLightInRange.CanGo();
        
        return !isObjectInTrigger;
    }

    void HandleMovementDecision(bool shouldMove)
    {
        if (shouldMove && agent.isStopped)
        {
            if (agent.hasPath || agent.pathPending)
                agent.isStopped = false;
            else
                StartCoroutine(GoToNextWaypoint());
        }
        else if (!shouldMove && !agent.isStopped)
        {
            agent.isStopped = true;
        }
    }
    
    IEnumerator GoToNextWaypoint()
    {
        if (waypoints?.Length == 0 || agent?.isOnNavMesh != true) yield break;

        if (currentWaypointIndex >= waypoints.Length)
        {
            Destroy(gameObject);
            yield break;
        }
        
        if (waypoints[currentWaypointIndex] == null)
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length) Destroy(gameObject);
            yield break;
        }

        Vector3 targetPosition = waypoints[currentWaypointIndex].position;
        
        if (!IsPathValid(targetPosition))
        {
            currentWaypointIndex++;
            if (currentWaypointIndex >= waypoints.Length) Destroy(gameObject);
            else yield return GoToNextWaypoint();
            yield break;
        }
        
        agent.SetDestination(targetPosition);
        currentWaypointIndex++;
        
        if (currentWaypointIndex >= waypoints.Length)
        {
            yield return new WaitForSeconds(1f);
            Destroy(gameObject);
        }
    }

    bool IsPathValid(Vector3 targetPosition)
    {
        NavMeshPath testPath = new NavMeshPath();
        return agent.CalculatePath(targetPosition, testPath) && testPath.status == NavMeshPathStatus.PathComplete;
    }
    
    IEnumerator FindNearestWaypoint()
    {
        if (waypoints?.Length == 0) yield break;
        
        float bestScore = float.MaxValue;
        int bestIndex = 0;
        Vector3 carPosition = transform.position;
        Vector3 carForward = transform.forward;
        
        for (int i = 0; i < waypoints.Length; i++)
        {
            if (waypoints[i] == null) continue;
            
            Vector3 waypointPosition = waypoints[i].position;
            float distance = Vector3.Distance(carPosition, waypointPosition);
            
            if (distance > waypointSearchRadius) continue;
            
            Vector3 directionToWaypoint = (waypointPosition - carPosition).normalized;
            float dotProduct = Vector3.Dot(carForward, directionToWaypoint);
            float directionBonus = (dotProduct > 0) ? 0.5f : 1.5f;
            float score = distance * directionBonus;
            
            if (IsPathValid(waypointPosition)) score *= 0.9f;
            
            if (score < bestScore)
            {
                bestScore = score;
                bestIndex = i;
            }
            
            if (i % 5 == 0) yield return null; // Yield every 5 iterations
        }
        
        currentWaypointIndex = bestScore == float.MaxValue ? 0 : bestIndex;
    }

    void OnTriggerEnter(Collider other) => StartCoroutine(HandleTriggerEnter(other));
    void OnTriggerExit(Collider other) => StartCoroutine(HandleTriggerExit(other));
    
    IEnumerator HandleTriggerEnter(Collider other)
    {
        TrafficLightPoints tl = other.GetComponent<TrafficLightPoints>();
        if (tl != null)
        {
            trafficLightInRange = tl;
            yield return null;
        }
        
        if (IsCarOrPedestrian(other))
        {
            objectsInTriggerCount++;
            isObjectInTrigger = true;
        }
    }
    
    IEnumerator HandleTriggerExit(Collider other)
    {
        TrafficLightPoints tl = other.GetComponent<TrafficLightPoints>();
        if (tl != null && tl == trafficLightInRange)
        {
            trafficLightInRange = null;
            yield return null;
        }
        
        if (IsCarOrPedestrian(other))
        {
            objectsInTriggerCount = Mathf.Max(0, objectsInTriggerCount - 1);
            isObjectInTrigger = objectsInTriggerCount > 0;
            
            if (!isObjectInTrigger && (trafficLightInRange == null || trafficLightInRange.CanGo()))
                agent.isStopped = false;
        }
    }

    bool IsCarOrPedestrian(Collider other) => 
        (other.GetComponent<CarAI>() != null && other.GetComponent<CarAI>() != this) || other.CompareTag("Pedestrian");
    
    // Public API
    public void RescanNearestWaypoint() => StartCoroutine(RescanWaypoint());
    public void PauseCarAI() { StopAllCoroutines(); if (agent != null) agent.isStopped = true; }
    public void ResumeCarAI() { if (agent != null) agent.isStopped = false; StartAllCoroutines(); }
    public int GetCurrentWaypointIndex() => currentWaypointIndex;
    public Transform GetCurrentWaypoint() => waypoints != null && currentWaypointIndex < waypoints.Length ? waypoints[currentWaypointIndex] : null;
    public bool IsObjectInTrigger() => isObjectInTrigger;
    public int GetObjectsInTriggerCount() => objectsInTriggerCount;
    
    IEnumerator RescanWaypoint()
    {
        if (waypoints?.Length > 0)
        {
            yield return FindNearestWaypoint();
            yield return GoToNextWaypoint();
        }
    }

    // Lifecycle
    void OnDestroy() => StopAllCoroutines();
    void OnDisable() => StopAllCoroutines();
    void OnEnable() { if (waypoints?.Length > 0) StartAllCoroutines(); }
    
#if UNITY_EDITOR
    void OnDrawGizmosSelected()
    {
        // Current waypoint visualization
        if (waypoints?[currentWaypointIndex] != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, waypoints[currentWaypointIndex].position);
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(waypoints[currentWaypointIndex].position, 2f);
        }
        
        // Search radius and direction
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, waypointSearchRadius);
        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * 5f);
        
        // Trigger visualization
        Collider triggerCollider = GetComponent<Collider>();
        if (triggerCollider?.isTrigger == true)
        {
            Gizmos.color = isObjectInTrigger ? Color.red : Color.cyan;
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawWireCube(triggerCollider.bounds.center - transform.position, triggerCollider.bounds.size);
            Gizmos.matrix = Matrix4x4.identity;
        }
        
        // Runtime status
        if (Application.isPlaying)
        {
            string status = $"Coroutines: R{(coroutines[0] != null ? "✓" : "✗")} T{(coroutines[1] != null ? "✓" : "✗")} W{(coroutines[2] != null ? "✓" : "✗")}";
            if (isObjectInTrigger) status += $"\nObjects: {objectsInTriggerCount}";
            UnityEditor.Handles.Label(transform.position + Vector3.up * 3f, status);
        }
    }
#endif
}
