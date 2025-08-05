using UnityEngine;

public class TrafficSpawner : MonoBehaviour
{
    [Header("Simple Car Spawner")]
    public GameObject carPrefab;
    public Transform[] spawnPoints;
    public Transform[] waypoints;
    public int numberOfCars = 5;

    void Start()
    {
        SpawnCars();
    }

    void SpawnCars()
    {
        // Use waypoints if no spawn points are set
        Transform[] spawners = spawnPoints.Length > 0 ? spawnPoints : waypoints;
        
        if (spawners.Length == 0)
        {
            Debug.LogError("No spawn points or waypoints assigned!");
            return;
        }

        for (int i = 0; i < numberOfCars; i++)
        {
            // Pick random spawn location
            int randomSpawn = Random.Range(0, spawners.Length);
            Transform spawnPoint = spawners[randomSpawn];
            
            // Random position around spawn point
            Vector3 spawnPos = spawnPoint.position + new Vector3(
                Random.Range(-2f, 2f), 
                0, 
                Random.Range(-2f, 2f)
            );
            
            // Create car
            GameObject car = Instantiate(carPrefab, spawnPos, spawnPoint.rotation);
            
            // Set up AI
            CarAI carAI = car.GetComponent<CarAI>();
            if (carAI != null && waypoints.Length > 0)
            {
                carAI.waypoints = waypoints;
            }
        }
    }
}
