using UnityEngine;

public class TrafficSpawner : MonoBehaviour
{
    [Header("Car Settings")]
    public GameObject carPrefab;
    public Transform[] spawnPoints; // Different spawn locations on the map
    public Transform[] waypoints;   // Waypoint path for AI cars
    
    [Header("Spawn Control")]
    public int minCars = 3;         // Minimum number of cars to spawn
    public int maxCars = 10;        // Maximum number of cars to spawn
    public int carsPerSpawnPoint = 2; // Max cars per spawn point
    public bool spawnRandomAmount = true; // If true, spawns random amount between min/max
    
    [Header("Advanced Settings")]
    public bool continuousSpawning = false; // Keep spawning over time
    public float spawnInterval = 5f;        // Time between spawns (if continuous)
    public int targetCarCount = 8;          // Target number of cars to maintain
    
    private int currentCarCount = 0;

    void Start()
    {
        SpawnInitialCars();
        
        if (continuousSpawning)
        {
            InvokeRepeating(nameof(MaintainCarCount), spawnInterval, spawnInterval);
        }
    }

    void SpawnInitialCars()
    {
        int carsToSpawn = CalculateSpawnAmount();
        Debug.Log($"Spawning {carsToSpawn} cars initially");
        
        SpawnCarsAtDifferentPoints(carsToSpawn);
    }

    int CalculateSpawnAmount()
    {
        if (spawnRandomAmount)
        {
            return Random.Range(minCars, maxCars + 1);
        }
        else
        {
            return Mathf.Clamp(targetCarCount, minCars, maxCars);
        }
    }

    void SpawnCarsAtDifferentPoints(int carCount)
    {
        // If no spawn points are set, use waypoints as spawn points
        Transform[] spawners = spawnPoints.Length > 0 ? spawnPoints : waypoints;
        
        if (spawners.Length == 0)
        {
            Debug.LogError("No spawn points or waypoints assigned!");
            return;
        }

        // Track cars per spawn point to avoid overcrowding
        int[] carsAtSpawnPoint = new int[spawners.Length];

        for (int i = 0; i < carCount; i++)
        {
            // Find a spawn point that hasn't reached its limit
            int randomSpawnIndex = FindAvailableSpawnPoint(spawners, carsAtSpawnPoint);
            
            if (randomSpawnIndex == -1)
            {
                Debug.Log($"All spawn points at capacity. Spawned {i} cars instead of {carCount}");
                break;
            }
            
            Transform spawnPoint = spawners[randomSpawnIndex];
            carsAtSpawnPoint[randomSpawnIndex]++;
            
            // Add some random offset so cars don't spawn on top of each other
            Vector3 randomOffset = new Vector3(
                Random.Range(-3f, 3f), 
                0, 
                Random.Range(-3f, 3f)
            );
            
            Vector3 spawnPos = spawnPoint.position + randomOffset;
            
            // Spawn car facing the same direction as the spawn point
            Quaternion spawnRotation = spawnPoint.rotation;
            GameObject car = Instantiate(carPrefab, spawnPos, spawnRotation);
            
            // Setup AI waypoints
            CarAI carAI = car.GetComponent<CarAI>();
            if (carAI != null && waypoints.Length > 0)
            {
                carAI.waypoints = waypoints;
            }
            
            currentCarCount++;
            Debug.Log($"Spawned car {i + 1}/{carCount} at spawn point {randomSpawnIndex}. Total cars: {currentCarCount}");
        }
    }

    int FindAvailableSpawnPoint(Transform[] spawners, int[] carsAtSpawnPoint)
    {
        // Try to find a spawn point that's not at capacity
        for (int attempts = 0; attempts < spawners.Length * 2; attempts++)
        {
            int randomIndex = Random.Range(0, spawners.Length);
            if (carsAtSpawnPoint[randomIndex] < carsPerSpawnPoint)
            {
                return randomIndex;
            }
        }
        
        // If all spawn points are at capacity, return -1
        return -1;
    }

    void MaintainCarCount()
    {
        // Count existing cars (remove destroyed ones)
        GameObject[] existingCars = GameObject.FindGameObjectsWithTag("Car"); // Assumes cars have "Car" tag
        currentCarCount = existingCars.Length;
        
        // Spawn more cars if below target
        if (currentCarCount < targetCarCount)
        {
            int carsNeeded = targetCarCount - currentCarCount;
            Debug.Log($"Maintaining car count: {currentCarCount}/{targetCarCount}. Spawning {carsNeeded} more cars.");
            SpawnCarsAtDifferentPoints(carsNeeded);
        }
    }

    // Public methods for runtime control
    public void SpawnMoreCars(int amount)
    {
        SpawnCarsAtDifferentPoints(amount);
    }

    public void SetTargetCarCount(int newTarget)
    {
        targetCarCount = Mathf.Clamp(newTarget, minCars, maxCars);
    }

    public int GetCurrentCarCount()
    {
        return currentCarCount;
    }
}
