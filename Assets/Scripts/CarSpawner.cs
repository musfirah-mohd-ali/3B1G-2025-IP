using UnityEngine;

public class TrafficSpawner : MonoBehaviour
{
    public GameObject carPrefab;
    public Transform[] waypoints;
    public int carCount = 5;

    void Start()
    {
        for (int i = 0; i < carCount; i++)
        {
            Vector3 spawnPos = waypoints[0].position + new Vector3(Random.Range(-2f, 2f), 0, Random.Range(-2f, 2f));
            GameObject car = Instantiate(carPrefab, spawnPos, Quaternion.identity);
            car.GetComponent<CarAI>().waypoints = waypoints;
        }
    }
}
