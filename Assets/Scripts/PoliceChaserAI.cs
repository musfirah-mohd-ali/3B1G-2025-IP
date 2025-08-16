using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class PoliceChaserAI : MonoBehaviour
{
    [Header("Movement & Speed")]
    public float chaseSpeed = 8f;
    public float stoppingDistance = 2f;
    
    [Header("Rotation")]
    public float rotationOffset = 0f;
    public float rotationSpeed = 15f;
    public bool useInstantRotation = false;
    
    [Header("Collision Settings")]
    public float collisionDistance = 2f;
    
    [Header("Performance")]
    public float updateInterval = 0.05f; // More frequent updates for better chasing
    public float rotationUpdateInterval = 0.02f;
    
    [Header("UI Reference")]
    public PackageLossUI packageLossUI;

    // Core components
    private Transform player;
    private NavMeshAgent agent;
    private DeliveryManager deliveryManager;
    private bool chasing = false;
    
    // Coroutine management
    private Coroutine[] chasingCoroutines = new Coroutine[3];

    void Start() => StartCoroutine(Initialize());

    IEnumerator Initialize()
    {
        agent = GetComponent<NavMeshAgent>();
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        deliveryManager = FindObjectOfType<DeliveryManager>();
        
        if (!ValidateComponents()) yield break;
        
        SetupAgent();
        yield return null; // Wait one frame
        StartChase();
    }

    bool ValidateComponents()
    {
        if (agent == null)
        {
            Debug.LogError("PoliceChaserAI: No NavMeshAgent component found!");
            return false;
        }
        
        if (player == null)
        {
            Debug.LogWarning("PoliceChaserAI: No Player found!");
            return false;
        }
        
        if (deliveryManager == null)
        {
            Debug.LogWarning("PoliceChaserAI: No DeliveryManager found!");
            return false;
        }
        
        if (!agent.isOnNavMesh)
        {
            Debug.LogError("PoliceChaserAI: Not placed on NavMesh!");
            return false;
        }
        
        return true;
    }

    void SetupAgent()
    {
        agent.updateRotation = false; // We'll handle rotation manually for offset
        agent.speed = chaseSpeed;
        agent.stoppingDistance = 0.5f; // Get very close before stopping
        agent.acceleration = 20f; // High acceleration for aggressive chase
        agent.angularSpeed = 720f; // Very fast turning
        agent.autoBraking = false; // Never slow down when approaching target
        agent.obstacleAvoidanceType = ObstacleAvoidanceType.MedQualityObstacleAvoidance;
        agent.radius = 0.5f; // Smaller radius for tighter navigation
        agent.height = 2f; // Standard height
    }

    public void StartChase()
    {
        if (!chasing && player != null && agent?.isOnNavMesh == true)
        {
            chasing = true;
            
            // Force agent settings on chase start
            agent.speed = chaseSpeed;
            agent.isStopped = false;
            agent.enabled = true;
            
            StopAllCoroutines();
            
            chasingCoroutines[0] = StartCoroutine(ChaseLoop());
            chasingCoroutines[1] = StartCoroutine(RotationLoop());
            chasingCoroutines[2] = StartCoroutine(CollisionLoop());
            
            Debug.Log($"PoliceChaserAI: Started chasing at speed {agent.speed}");
        }
    }

    public void StopChase()
    {
        chasing = false;
        if (agent != null) agent.isStopped = true;
        StopChasingCoroutines();
    }

    IEnumerator ChaseLoop()
    {
        var wait = new WaitForSeconds(updateInterval);
        while (chasing && player != null && agent?.isOnNavMesh == true)
        {
            // Continuously update destination to player's position with prediction
            if (!agent.isStopped)
            {
                // Predict where player will be based on their velocity
                Vector3 predictedPosition = GetPredictedPlayerPosition();
                agent.SetDestination(predictedPosition);
                
                // Ensure agent speed matches chase speed
                agent.speed = chaseSpeed;
                
                // Force agent to not stop or slow down
                agent.isStopped = false;
            }
            yield return wait;
        }
    }

    Vector3 GetPredictedPlayerPosition()
    {
        // Get player's movement velocity
        Rigidbody playerRb = player.GetComponent<Rigidbody>();
        if (playerRb != null)
        {
            // Predict where player will be in 0.5 seconds
            Vector3 predictedPos = player.position + (playerRb.linearVelocity * 0.5f);
            return predictedPos;
        }
        
        // Fallback to current position
        return player.position;
    }

    IEnumerator RotationLoop()
    {
        var wait = new WaitForSeconds(rotationUpdateInterval);
        while (chasing && player != null && agent?.isOnNavMesh == true)
        {
            // Only rotate if agent is moving and has a path
            if (agent.hasPath && agent.velocity.sqrMagnitude > 0.1f)
            {
                Vector3 direction = agent.steeringTarget - transform.position;
                direction.y = 0; // Keep rotation only on Y-axis
                
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

    IEnumerator CollisionLoop()
    {
        var wait = new WaitForSeconds(updateInterval);
        while (chasing && player != null && deliveryManager != null && agent?.isOnNavMesh == true)
        {
            // Check both NavMesh distance and direct distance
            float directDistance = Vector3.Distance(transform.position, player.position);
            bool isCloseEnough = directDistance <= collisionDistance;
            
            // Also check if we've reached close to the player
            bool reachedPlayer = !agent.pathPending && agent.remainingDistance < collisionDistance;
            
            if ((isCloseEnough || reachedPlayer) && deliveryManager.HasPackage())
            {
                HandlePlayerCaught();
                yield break;
            }
            yield return wait;
        }
    }

    void HandlePlayerCaught()
    {
        Debug.Log("Police caught player with package!");
        
        StopChase();
        
        // Make player lose package
        if (deliveryManager != null)
        {
            deliveryManager.LosePackage();
            Debug.Log("Player's package confiscated! Find another package to continue deliveries.");
        }
        
        // Show package loss UI notification
        ShowPackageLossUI();
        
        // Police disappears after successful arrest
        StartCoroutine(ReturnToBase());
    }

    void ShowPackageLossUI() => (packageLossUI ?? FindObjectOfType<PackageLossUI>())?.ShowUI();

    IEnumerator ReturnToBase()
    {
        yield return new WaitForSeconds(2f);
        Destroy(gameObject);
    }

    void StopChasingCoroutines()
    {
        for (int i = 0; i < chasingCoroutines.Length; i++)
        {
            if (chasingCoroutines[i] != null)
            {
                StopCoroutine(chasingCoroutines[i]);
                chasingCoroutines[i] = null;
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && chasing && deliveryManager?.HasPackage() == true)
            HandlePlayerCaught();
    }

    void OnDestroy() => StopChasingCoroutines();
    void OnDisable() => StopChasingCoroutines();
}
