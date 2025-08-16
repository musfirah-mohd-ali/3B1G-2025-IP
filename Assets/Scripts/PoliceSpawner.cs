using UnityEngine;
using System.Collections;

public class PoliceSpawner : MonoBehaviour
{
    [Header("Police Settings")]
    public GameObject policePrefab;           // Assign your police car prefab
    public Transform[] spawnPoints;           // Assign spawn points in inspector
    public int minPolice = 1;
    public int maxPolice = 3;
    public bool spawnRandomAmount = true;

    [Header("Chase Settings")]
    public float chaseSpeed = 8f;             // Speed at which police chase the player

    void Start()
    {
        SpawnPolice();
    }

    public void SpawnPolice()
    {
        if (policePrefab == null)
        {
            Debug.LogError("[PoliceSpawner] Police prefab not assigned!");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("[PoliceSpawner] No spawn points assigned!");
            return;
        }

        int policeToSpawn = spawnRandomAmount ? Random.Range(minPolice, maxPolice + 1) : maxPolice;

        for (int i = 0; i < policeToSpawn; i++)
        {
            Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            // Ensure police faces forward relative to spawn point
            Vector3 spawnPosition = spawnPoint.position;
            Quaternion spawnRotation = Quaternion.LookRotation(spawnPoint.forward);

            GameObject police = Instantiate(policePrefab, spawnPosition, spawnRotation);

            // Add PoliceChaserAI if not already on prefab
            PoliceChaserAI chaserAI = police.GetComponent<PoliceChaserAI>();
            if (chaserAI == null)
                chaserAI = police.AddComponent<PoliceChaserAI>();

            // Set chase speed
            chaserAI.chaseSpeed = chaseSpeed;

            // Start chasing the player immediately
            chaserAI.StartChase();
        }

        Debug.Log($"[PoliceSpawner] Spawned {policeToSpawn} police cars.");
    }
}
