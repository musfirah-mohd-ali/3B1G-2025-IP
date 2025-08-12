using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    [HideInInspector]
    public DeliveryManager deliveryManager;

    private void OnTriggerEnter(Collider other)
    {
        // Check if the player (with camera) entered the zone
        if (other.GetComponentInChildren<Camera>() != null)
        {
            if (deliveryManager != null)
            {
                deliveryManager.OnPlayerEnterZone();
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the player (with camera) left the zone
        if (other.GetComponentInChildren<Camera>() != null)
        {
            if (deliveryManager != null)
            {
                deliveryManager.OnPlayerExitZone();
            }
        }
    }
}
