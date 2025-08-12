using UnityEngine;

public class CarEntry : MonoBehaviour
{
    [Header("Car Entry Settings")]
    public CarBehaviour carBehaviour; // Reference to the car behaviour script
    public KeyCode entryKey = KeyCode.E; // Key to press to enter car
    
    private bool playerInTrigger = false;
    private GameObject currentPlayer = null;

    void Update()
    {
        // Check if player is in trigger and presses entry key
        if (playerInTrigger && Input.GetKeyDown(entryKey) && currentPlayer != null)
        {
            EnterCar();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Check if the object entering has a Camera (likely the FPS controller)
        if (other.GetComponentInChildren<Camera>() != null)
        {
            playerInTrigger = true;
            currentPlayer = other.gameObject;
            Debug.Log("Press E to enter the car");
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Check if the object leaving has a Camera
        if (other.GetComponentInChildren<Camera>() != null && other.gameObject == currentPlayer)
        {
            playerInTrigger = false;
            currentPlayer = null;
            Debug.Log("Moved away from car");
        }
    }

    private void EnterCar()
    {
        if (carBehaviour != null && currentPlayer != null)
        {
            // Call the car's re-entry method
            carBehaviour.EnterCar(currentPlayer);
            
            // Clear trigger state
            playerInTrigger = false;
            currentPlayer = null;
        }
    }
}
