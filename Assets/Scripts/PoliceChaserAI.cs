using UnityEngine;
using System.Collections;

public class PoliceChaserAI : MonoBehaviour
{
    [Header("Movement Settings")]
    public float chaseSpeed = 8f;
    public float waypointReachDistance = 1f;

    private Transform player;
    private bool chasing = false;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        if (player == null)
            Debug.LogWarning("PoliceChaserAI: No Player found in scene!");
        
        StartChase();
    }

    void Update()
    {
        if (chasing && player != null)
        {
            transform.position = Vector3.MoveTowards(transform.position, player.position, chaseSpeed * Time.deltaTime);
            transform.LookAt(player);
        }
    }

    public void StartChase()
    {
        chasing = true;
    }
}
