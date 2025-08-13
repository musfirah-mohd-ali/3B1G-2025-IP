using UnityEngine;

public class PedestrianSpawner : MonoBehaviour
{
    [Header("Pedestrian Settings")]
    public GameObject[] pedestrianPrefabs; // Multiple pedestrian prefabs to spawn randomly
    public Transform[] spawnPoints; // Different spawn locations on the map
    public Transform[] pedestrianWaypoints; // Waypoints for pedestrian navigation (empty GameObjects)
    
    [Header("Spawn Control")]
    public int minPedestrians = 5;         // Minimum number of pedestrians to spawn
    public int maxPedestrians = 15;        // Maximum number of pedestrians to spawn
    public int pedestriansPerSpawnPoint = 3; // Max pedestrians per spawn point
    public bool spawnRandomAmount = true; // If true, spawns random amount between min/max
    
    [Header("Advanced Settings")]
    public bool continuousSpawning = false; // Keep spawning over time
    public float spawnInterval = 10f;       // Time between spawns (if continuous)
    public int targetPedestrianCount = 10;  // Target number of pedestrians to maintain
    
    [Header("Spawn Behavior")]
    public float spawnRadius = 2f;          // Random spawn radius around spawn points
    public bool useWaypointsAsSpawns = true; // Use waypoints as additional spawn points if no spawn points set
    public string pedestrianTag = "Pedestrian"; // Tag for pedestrians (for counting)
    
    private int currentPedestrianCount = 0;

    void Start()
    {
        SpawnInitialPedestrians();
        
        if (continuousSpawning)
        {
            InvokeRepeating(nameof(MaintainPedestrianCount), spawnInterval, spawnInterval);
        }
    }

    void SpawnInitialPedestrians()
    {
        int pedestriansToSpawn = CalculateSpawnAmount();
        Debug.Log($"Spawning {pedestriansToSpawn} pedestrians initially");
        
        SpawnPedestriansAtDifferentPoints(pedestriansToSpawn);
    }

    int CalculateSpawnAmount()
    {
        if (spawnRandomAmount)
        {
            return Random.Range(minPedestrians, maxPedestrians + 1);
        }
        else
        {
            return Mathf.Clamp(targetPedestrianCount, minPedestrians, maxPedestrians);
        }
    }

    void SpawnPedestriansAtDifferentPoints(int pedestrianCount)
    {
        // Check if we have pedestrian prefabs
        if (pedestrianPrefabs == null || pedestrianPrefabs.Length == 0)
        {
            Debug.LogError("No pedestrian prefabs assigned to PedestrianSpawner!");
            return;
        }
        
        // Get spawn points
        Transform[] spawners = GetSpawnPoints();
        
        if (spawners.Length == 0)
        {
            Debug.LogError("No spawn points or waypoints found for pedestrians!");
            return;
        }

        // Track pedestrians per spawn point to avoid overcrowding
        int[] pedestriansAtSpawnPoint = new int[spawners.Length];

        for (int i = 0; i < pedestrianCount; i++)
        {
            // Find a spawn point that hasn't reached its limit
            int randomSpawnIndex = FindAvailableSpawnPoint(spawners, pedestriansAtSpawnPoint);
            
            if (randomSpawnIndex == -1)
            {
                Debug.Log($"All spawn points at capacity. Spawned {i} pedestrians instead of {pedestrianCount}");
                break;
            }
            
            Transform spawnPoint = spawners[randomSpawnIndex];
            pedestriansAtSpawnPoint[randomSpawnIndex]++;
            
            // Randomly select a pedestrian prefab
            GameObject selectedPedestrianPrefab = pedestrianPrefabs[Random.Range(0, pedestrianPrefabs.Length)];
            
            // Add random offset around spawn point
            Vector3 randomOffset = Random.insideUnitCircle * spawnRadius;
            Vector3 spawnPos = spawnPoint.position + new Vector3(randomOffset.x, 0, randomOffset.y);
            
            // Random rotation for variety (pedestrians can face any direction initially)
            Quaternion spawnRotation = Quaternion.Euler(0, Random.Range(0f, 360f), 0);
            
            // Spawn pedestrian
            GameObject pedestrian = Instantiate(selectedPedestrianPrefab, spawnPos, spawnRotation);
            
            // Set tag for counting
            if (!string.IsNullOrEmpty(pedestrianTag))
            {
                pedestrian.tag = pedestrianTag;
            }
            
            // Setup AI waypoints - assign manually configured waypoints
            PedestrianAI pedestrianAI = pedestrian.GetComponent<PedestrianAI>();
            if (pedestrianAI != null)
            {
                // Assign waypoints if available
                if (pedestrianWaypoints != null && pedestrianWaypoints.Length > 0)
                {
                    pedestrianAI.waypoints = pedestrianWaypoints;
                    pedestrianAI.autoFindWaypointsIfEmpty = false; // Use manual waypoints only
                    Debug.Log($"Assigned {pedestrianWaypoints.Length} manual waypoints to {pedestrian.name}");
                }
                else
                {
                    pedestrianAI.autoFindWaypointsIfEmpty = true; // Allow fallback to Waypoints component
                    Debug.Log($"No manual waypoints assigned to {pedestrian.name}, using auto-find");
                }
                
                // Optionally randomize some settings for variety
                RandomizePedestrianBehavior(pedestrianAI);
            }
            
            currentPedestrianCount++;
            Debug.Log($"Spawned {selectedPedestrianPrefab.name} pedestrian {i + 1}/{pedestrianCount} at spawn point {randomSpawnIndex}. Total pedestrians: {currentPedestrianCount}");
        }
    }

    Transform[] GetSpawnPoints()
    {
        // Use assigned spawn points if available
        if (spawnPoints != null && spawnPoints.Length > 0)
        {
            return spawnPoints;
        }
        
        // Fallback to waypoints if enabled and no spawn points set
        if (useWaypointsAsSpawns)
        {
            Waypoints waypoints = FindObjectOfType<Waypoints>();
            if (waypoints?.points != null && waypoints.points.Length > 0)
            {
                Debug.Log("Using waypoints as pedestrian spawn points");
                return waypoints.points;
            }
        }
        
        // Return empty array if nothing found
        return new Transform[0];
    }

    int FindAvailableSpawnPoint(Transform[] spawners, int[] pedestriansAtSpawnPoint)
    {
        // Try to find a spawn point that's not at capacity
        for (int attempts = 0; attempts < spawners.Length * 2; attempts++)
        {
            int randomIndex = Random.Range(0, spawners.Length);
            if (pedestriansAtSpawnPoint[randomIndex] < pedestriansPerSpawnPoint)
            {
                return randomIndex;
            }
        }
        
        // If all spawn points are at capacity, return -1
        return -1;
    }

    void RandomizePedestrianBehavior(PedestrianAI pedestrianAI)
    {
        // Add some variety to pedestrian behavior
        
        // Randomize walk speed slightly
        float baseWalkSpeed = pedestrianAI.walkSpeed;
        pedestrianAI.walkSpeed = baseWalkSpeed * Random.Range(0.8f, 1.2f);
        
        // Randomize wait time at waypoints
        pedestrianAI.waitTimeAtWaypoint = Random.Range(1f, 4f);
        
        // 30% chance to use random waypoints instead of sequential
        if (Random.Range(0f, 1f) < 0.3f)
        {
            pedestrianAI.randomWaypoints = true;
        }
        
        // 20% chance to not wait at waypoints (always moving)
        if (Random.Range(0f, 1f) < 0.2f)
        {
            pedestrianAI.shouldWaitAtWaypoints = false;
        }
    }

    void MaintainPedestrianCount()
    {
        // Count existing pedestrians (remove destroyed ones)
        GameObject[] existingPedestrians = GameObject.FindGameObjectsWithTag(pedestrianTag);
        currentPedestrianCount = existingPedestrians.Length;
        
        // Spawn more pedestrians if below target
        if (currentPedestrianCount < targetPedestrianCount)
        {
            int pedestriansNeeded = targetPedestrianCount - currentPedestrianCount;
            Debug.Log($"Maintaining pedestrian count: {currentPedestrianCount}/{targetPedestrianCount}. Spawning {pedestriansNeeded} more pedestrians.");
            SpawnPedestriansAtDifferentPoints(pedestriansNeeded);
        }
    }

    // Public methods for runtime control
    public void SpawnMorePedestrians(int amount)
    {
        SpawnPedestriansAtDifferentPoints(amount);
    }

    public void SetTargetPedestrianCount(int newTarget)
    {
        targetPedestrianCount = Mathf.Clamp(newTarget, minPedestrians, maxPedestrians);
    }

    public int GetCurrentPedestrianCount()
    {
        return currentPedestrianCount;
    }

    public void ClearAllPedestrians()
    {
        GameObject[] existingPedestrians = GameObject.FindGameObjectsWithTag(pedestrianTag);
        foreach (GameObject pedestrian in existingPedestrians)
        {
            if (pedestrian != null)
            {
                DestroyImmediate(pedestrian);
            }
        }
        currentPedestrianCount = 0;
        Debug.Log("Cleared all spawned pedestrians");
    }

    // Gizmos for visualizing spawn points in Scene view
    void OnDrawGizmosSelected()
    {
        if (spawnPoints != null)
        {
            Gizmos.color = Color.blue;
            foreach (Transform spawnPoint in spawnPoints)
            {
                if (spawnPoint != null)
                {
                    Gizmos.DrawWireSphere(spawnPoint.position, spawnRadius);
                    Gizmos.DrawWireSphere(spawnPoint.position, 0.5f);
                }
            }
        }
    }
}
