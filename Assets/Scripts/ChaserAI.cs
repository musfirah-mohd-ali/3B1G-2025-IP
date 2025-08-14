using UnityEngine;
using System.Collections;
using UnityEngine.AI;

public class ChaserAI : MonoBehaviour
{
    NavMeshAgent myAgent; // Reference to the NavMeshAgent component

    [SerializeField]
    private GameObject[] PatrolPoints; // Array of patrol points to move between
    private Transform targetTransform; // Target to chase, set when player enters trigger
    public string currentState; // Current state of the agent (Patrol, Chase, Idle)
    private Coroutine currentRoutine; // Store the current routine for stopping it later

    void Start()
    {
        myAgent = GetComponent<NavMeshAgent>();
        currentState = "Patrol";
        currentRoutine = StartCoroutine(Patrol());
    }

    IEnumerator Patrol()
    {
        int i = 0;
        while (PatrolPoints.Length > 0)
        {
            myAgent.SetDestination(PatrolPoints[i].transform.position);

            // Wait until agent is close to the patrol point
            while (myAgent.remainingDistance > 0.2f)
                yield return null;

            // Enter idle state at patrol point
            yield return StartCoroutine(Idle());

            i = (i + 1) % PatrolPoints.Length;
        }
    }

    IEnumerator Idle()
    {
        currentState = "Idle";
        Debug.Log("Idling at patrol point...");
        yield return new WaitForSeconds(2f); // Idle for 2 seconds
        currentState = "Patrol";
    }

    IEnumerator Chase()
    {
        currentState = "Chase";
        Debug.Log("Chasing player!");
        while (targetTransform != null)
        {
            // Check if the target is still valid
            myAgent.SetDestination(targetTransform.position);
            yield return null;// Wait for the next frame
        }
        currentState = "Patrol";
        currentRoutine = StartCoroutine(Patrol());
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // Start chasing the player
            targetTransform = other.transform;
            // Stop any current routine
            if (currentRoutine != null)
                StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(Chase());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            targetTransform = null;
            if (currentRoutine != null)
                StopCoroutine(currentRoutine);
            currentRoutine = StartCoroutine(SwitchState(3f));
        }
    }

    IEnumerator SwitchState(float delay)
    {
        yield return new WaitForSeconds(delay);
        currentRoutine = StartCoroutine(Patrol());
    }
}