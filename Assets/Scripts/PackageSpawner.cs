using UnityEngine;

public class PackageSpawner : MonoBehaviour
{
    [Header("Package Settings")]
    public GameObject packagePrefab;
    public Transform[] spawnPoints;
    public int numberOfPackages = 10;
    
    [Header("Random Spawn Area (if no spawn points)")]
    public float spawnRadius = 50f;
    public LayerMask groundLayer = 1; // What layer is the ground

    void Start()
    {
        SpawnPackages();
    }

    void SpawnPackages()
    {
        if (packagePrefab == null)
        {
            Debug.LogError("No package prefab assigned!");
            return;
        }

        for (int i = 0; i < numberOfPackages; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            
            if (spawnPos != Vector3.zero)
            {
                GameObject package = Instantiate(packagePrefab, spawnPos, Random.rotation);
                Debug.Log($"Spawned package {i + 1} at {spawnPos}");
            }
        }
    }

    Vector3 GetRandomSpawnPosition()
    {
        // If we have specific spawn points, use them
        if (spawnPoints.Length > 0)
        {
            Transform randomSpawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            
            // Add small random offset
            Vector3 offset = new Vector3(
                Random.Range(-3f, 3f),
                0,
                Random.Range(-3f, 3f)
            );
            
            return randomSpawnPoint.position + offset;
        }
        
        // Otherwise, spawn randomly around this spawner object
        return GetRandomPositionAroundSpawner();
    }

    Vector3 GetRandomPositionAroundSpawner()
    {
        for (int attempts = 0; attempts < 10; attempts++)
        {
            // Random position in circle around spawner
            Vector2 randomCircle = Random.insideUnitCircle * spawnRadius;
            Vector3 randomPos = transform.position + new Vector3(randomCircle.x, 0, randomCircle.y);
            
            // Try to place on ground
            RaycastHit hit;
            if (Physics.Raycast(randomPos + Vector3.up * 100f, Vector3.down, out hit, 200f, groundLayer))
            {
                return hit.point + Vector3.up * 0.5f; // Slightly above ground
            }
        }
        
        Debug.LogWarning("Couldn't find valid ground position for package");
        return Vector3.zero;
    }

    // Call this to spawn more packages during gameplay
    public void SpawnMorePackages(int amount)
    {
        for (int i = 0; i < amount; i++)
        {
            Vector3 spawnPos = GetRandomSpawnPosition();
            if (spawnPos != Vector3.zero)
            {
                Instantiate(packagePrefab, spawnPos, Random.rotation);
            }
        }
    }
}
