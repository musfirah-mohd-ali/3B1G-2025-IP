using UnityEngine;

public class Waypoints : MonoBehaviour
{
    [Header("Car Waypoint System")]
    public Transform[] points; // Car waypoints (for backward compatibility)
    
    [Header("Pedestrian Waypoint System")]
    public Transform[] pedestrianWaypoints; // Pedestrian waypoints
    
    [Header("Visual Debug - Cars")]
    public bool showCarWaypointGizmos = true;
    public bool showCarWaypointPath = true;
    public Color carWaypointColor = Color.green;
    public Color carPathColor = Color.yellow;
    public float carWaypointSize = 0.5f;
    
    [Header("Visual Debug - Pedestrians")]
    public bool showPedestrianWaypointGizmos = true;
    public bool showPedestrianWaypointPath = true;
    public Color pedestrianWaypointColor = Color.blue;
    public Color pedestrianPathColor = Color.cyan;
    public float pedestrianWaypointSize = 0.3f;

    void Start()
    {
        // Validate both car and pedestrian waypoints on start
        ValidateCarWaypoints();
        ValidatePedestrianWaypoints();
    }
    
    void ValidateCarWaypoints()
    {
        if (points == null || points.Length == 0)
        {
            Debug.LogWarning("Waypoints component has no car waypoints assigned! Cars won't know where to go.");
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
            Debug.LogWarning($"Car waypoints has {nullCount} null waypoints. Please assign all waypoint slots.");
        }
        else
        {
            Debug.Log($"Car waypoints system ready with {points.Length} waypoints for AI cars.");
        }
    }
    
    void ValidatePedestrianWaypoints()
    {
        if (pedestrianWaypoints == null || pedestrianWaypoints.Length == 0)
        {
            Debug.LogWarning("Waypoints component has no pedestrian waypoints assigned! Pedestrians won't know where to go.");
            return;
        }
        
        // Check for null waypoints
        int nullCount = 0;
        for (int i = 0; i < pedestrianWaypoints.Length; i++)
        {
            if (pedestrianWaypoints[i] == null)
                nullCount++;
        }
        
        if (nullCount > 0)
        {
            Debug.LogWarning($"Pedestrian waypoints has {nullCount} null waypoints. Please assign all waypoint slots.");
        }
        else
        {
            Debug.Log($"Pedestrian waypoints system ready with {pedestrianWaypoints.Length} waypoints for AI pedestrians.");
        }
    }
    
    // Public methods to get waypoints
    public Transform[] GetCarWaypoints()
    {
        return points;
    }
    
    public Transform[] GetPedestrianWaypoints()
    {
        return pedestrianWaypoints;
    }

#if UNITY_EDITOR
    void OnDrawGizmos()
    {
        // Draw car waypoints
        DrawCarWaypoints();
        
        // Draw pedestrian waypoints
        DrawPedestrianWaypoints();
    }
    
    void DrawCarWaypoints()
    {
        if (!showCarWaypointGizmos || points == null) return;
        
        // Draw car waypoints
        Gizmos.color = carWaypointColor;
        for (int i = 0; i < points.Length; i++)
        {
            if (points[i] != null)
            {
                Gizmos.DrawWireSphere(points[i].position, carWaypointSize);
                
                // Draw waypoint number
                UnityEditor.Handles.Label(points[i].position + Vector3.up, $"CAR {i}");
            }
        }
        
        // Draw path between car waypoints
        if (showCarWaypointPath && points.Length > 1)
        {
            Gizmos.color = carPathColor;
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
    
    void DrawPedestrianWaypoints()
    {
        if (!showPedestrianWaypointGizmos || pedestrianWaypoints == null) return;
        
        // Draw pedestrian waypoints
        Gizmos.color = pedestrianWaypointColor;
        for (int i = 0; i < pedestrianWaypoints.Length; i++)
        {
            if (pedestrianWaypoints[i] != null)
            {
                Gizmos.DrawWireSphere(pedestrianWaypoints[i].position, pedestrianWaypointSize);
                
                // Draw waypoint number
                UnityEditor.Handles.Label(pedestrianWaypoints[i].position + Vector3.up * 1.5f, $"PED {i}");
            }
        }
        
        // Draw path between pedestrian waypoints
        if (showPedestrianWaypointPath && pedestrianWaypoints.Length > 1)
        {
            Gizmos.color = pedestrianPathColor;
            for (int i = 0; i < pedestrianWaypoints.Length; i++)
            {
                if (pedestrianWaypoints[i] != null)
                {
                    int nextIndex = (i + 1) % pedestrianWaypoints.Length;
                    if (pedestrianWaypoints[nextIndex] != null)
                    {
                        Gizmos.DrawLine(pedestrianWaypoints[i].position, pedestrianWaypoints[nextIndex].position);
                    }
                }
            }
        }
    }
#endif
}
