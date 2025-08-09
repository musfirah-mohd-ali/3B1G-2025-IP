using UnityEngine;

public class Waypoints : MonoBehaviour
{
    [Header("Waypoint System")]
    public Transform[] points;
    
    [Header("Visual Debug")]
    public bool showWaypointGizmos = true;
    public bool showWaypointPath = true;
    public Color waypointColor = Color.green;
    public Color pathColor = Color.yellow;
    public float waypointSize = 0.5f;

    void Start()
    {
        // Validate waypoints on start
        ValidateWaypoints();
    }
    
    void ValidateWaypoints()
    {
        if (points == null || points.Length == 0)
        {
            Debug.LogWarning("Waypoints component has no waypoints assigned! Cars won't know where to go.");
            return;
        }
        
        // Check for null waypoints
        int nullCount = 0;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] == null)
                nullCount++;
        }
        
        if (nullCount > 0)
        {
            Debug.LogWarning($"Waypoints component has {nullCount} null waypoints. Please assign all waypoint slots.");
        }
        else
        {
            Debug.Log($"Waypoints system ready with {points.Length} waypoints for AI cars.");
        }
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        if (!showWaypointGizmos || points == null) return;
        
        // Draw waypoints
        Gizmos.color = waypointColor;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null)
            {
                Gizmos.DrawWireSphere(points[i].position, waypointSize);
                
                // Draw waypoint number
                UnityEditor.Handles.Label(points[i].position + Vector3.up, $"WP {i}");
            }
        }
        
        // Draw path between waypoints
        if (showWaypointPath && points.Length > 1)
        {
            Gizmos.color = pathColor;
            for (int i = 0; i < points.Length; i++)
            {
                if (points[i] != null)
                {
                    int nextIndex = (i + 1) % points.Length;
                    if (points[nextIndex] != null)
                    {
                        Gizmos.DrawLine(points[i].position, points[nextIndex].position);
                    }
                }
            }
        }
    }
#endif
}
