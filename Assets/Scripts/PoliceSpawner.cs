using UnityEngine;

public class PoliceSpawner : MonoBehaviour
{
    [Header("Police Settings")]
    public GameObject policePrefab;      // Police car prefab (must have PoliceChaserAI)
    public Transform[] spawnPoints;      // Police spawn points
    public int minPolice = 1;
    public int maxPolice = 3;
    public bool spawnRandomAmount = true;

    public void SpawnPolice()
    {
        if (policePrefab == null || spawnPoints.Length == 0)
        {
            Debug.LogWarning("PoliceSpawner: Missing prefab or spawn points!");
            return;
        }

        int policeToSpawn = spawnRandomAmount ? Random.Range(minPolice, maxPolice + 1) : maxPolice;

        for (int i = 0; i < policeToSpawn; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
            GameObject police = Instantiate(policePrefab, spawnPoint.position, spawnPoint.rotation);

            // Ensure PoliceChaserAI is active and starts chasing
            PoliceChaserAI chaser = police.GetComponent<PoliceChaserAI>();
            if (chaser != null)
                chaser.StartChase();
            else
                Debug.LogWarning("PoliceSpawner: Spawned police prefab has no PoliceChaserAI component!");
        }

        Debug.Log($"PoliceSpawner: Spawned {policeToSpawn} police cars!");
    }
}
