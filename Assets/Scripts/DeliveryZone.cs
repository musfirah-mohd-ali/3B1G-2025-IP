using UnityEngine;

public class DeliveryZone : MonoBehaviour
{
    [HideInInspector]
    public DeliveryManager deliveryManager;
    
    [Header("Audio")]
    public AudioSource audioSource;
    public AudioClip deliveryCompleteSound;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"DeliveryZone: Object entered trigger - {other.name} with tag: {other.tag}");
        
        // Check if the player (with camera) entered the zone
        Camera camera = other.GetComponentInChildren<Camera>();
        if (camera != null)
        {
            Debug.Log($"DeliveryZone: Camera found - {camera.name}");
            
            if (deliveryManager != null)
            {
                Debug.Log("DeliveryZone: Calling OnPlayerEnterZone");
                deliveryManager.OnPlayerEnterZone();
                
                // Sound will be played only when delivery is actually completed
                // via PlayDeliveryCompleteSound() method called by DeliveryManager
            }
            else
            {
                Debug.LogError("DeliveryZone: DeliveryManager is null!");
            }
        }
        else
        {
            Debug.Log($"DeliveryZone: No camera found on {other.name}");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"DeliveryZone: Object exited trigger - {other.name}");
        
        // Check if the player (with camera) left the zone
        if (other.GetComponentInChildren<Camera>() != null)
        {
            if (deliveryManager != null)
            {
                Debug.Log("DeliveryZone: Calling OnPlayerExitZone");
                deliveryManager.OnPlayerExitZone();
            }
        }
    }
    
    /// <summary>
    /// Public method to be called by DeliveryManager when delivery is actually completed
    /// </summary>
    public void PlayDeliveryCompleteSound()
    {
        PlayDeliverySound();
    }
    
    private void PlayDeliverySound()
    {
        if (audioSource != null && deliveryCompleteSound != null)
        {
            audioSource.PlayOneShot(deliveryCompleteSound);
            Debug.Log("DeliveryZone: Playing delivery complete sound");
        }
        else if (deliveryCompleteSound != null)
        {
            // If no AudioSource is assigned, try to find one on this GameObject
            AudioSource foundAudioSource = GetComponent<AudioSource>();
            if (foundAudioSource != null)
            {
                foundAudioSource.PlayOneShot(deliveryCompleteSound);
                Debug.Log("DeliveryZone: Playing delivery complete sound using found AudioSource");
            }
            else
            {
                Debug.LogWarning("DeliveryZone: No AudioSource found to play delivery sound!");
            }
        }
        else
        {
            Debug.LogWarning("DeliveryZone: No delivery complete sound assigned!");
        }
    }
}
