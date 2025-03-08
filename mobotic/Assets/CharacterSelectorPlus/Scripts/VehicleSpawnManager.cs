using UnityEngine;

public class VehicleSpawnManager : MonoBehaviour
{
    // The parent transform where vehicles will be spawned
    public Transform vehicleParent;

    // The position offset for spawning vehicles
    public Vector3 spawnOffset = Vector3.zero;

    // Reference to the existing camera controller
    public CameraController cameraController;

    // Reference to the UI camera
    public Camera uiCamera;

    // Reference to the currently spawned vehicle
    private GameObject currentVehicle;

    // Reference to the CharacterSelector
    private CharacterSelector characterSelector;

    private void Awake()
    {
        // Find the CharacterSelector in the scene
        characterSelector = FindObjectOfType<CharacterSelector>();
        if (characterSelector == null)
        {
            Debug.LogWarning("CharacterSelector not found in the scene. Vehicle spawning may not work correctly.");
        }
        else
        {
            // Validate the vehicle prefabs array
            if (characterSelector.vehiclePrefabs == null || characterSelector.vehiclePrefabs.Length == 0)
            {
                Debug.LogWarning("Vehicle prefabs array is not set up in CharacterSelector. Vehicle spawning will not work.");
            }
            else if (characterSelector.vehiclePrefabs.Length != characterSelector.prefab.Length)
            {
                Debug.LogWarning("Vehicle prefabs array length does not match display prefabs array length in CharacterSelector. Vehicle spawning may not work correctly.");
            }
        }

        // Find the camera controller if not assigned
        if (cameraController == null)
        {
            cameraController = FindObjectOfType<CameraController>();
            if (cameraController == null)
            {
                Debug.LogWarning("Camera controller not found in the scene. Camera will not follow spawned vehicles.");
            }
        }

        // If UI camera is not assigned, try to get it from the CharacterSelector
        if (uiCamera == null && characterSelector != null && characterSelector.uiCamera != null)
        {
            uiCamera = characterSelector.uiCamera;
        }
    }

    private void OnEnable()
    {
        // Subscribe to the vehicle spawn event
        CharacterSelector.OnVehicleSpawn += SpawnVehicle;

        // Subscribe to the return to menu event
        CharacterSelector.OnReturnToMenu += HandleReturnToMenu;
    }

    private void OnDisable()
    {
        // Unsubscribe from the vehicle spawn event
        CharacterSelector.OnVehicleSpawn -= SpawnVehicle;

        // Unsubscribe from the return to menu event
        CharacterSelector.OnReturnToMenu -= HandleReturnToMenu;
    }

    // Handle the return to menu event
    private void HandleReturnToMenu()
    {
        // Delete the current vehicle
        DestroyCurrentVehicle();

        // Enable the UI camera when returning to menu
        if (uiCamera != null)
        {
            uiCamera.enabled = true;
            Debug.Log("UI camera enabled for menu in VehicleSpawnManager");
        }
    }

    // Destroy the current vehicle
    private void DestroyCurrentVehicle()
    {
        if (currentVehicle != null)
        {
            Destroy(currentVehicle);
            currentVehicle = null;

            // Reset the camera target
            if (cameraController != null)
            {
                cameraController.target = null;
                Debug.Log("Camera target reset when destroying vehicle");
            }

            Debug.Log("Vehicle destroyed when returning to menu");
        }
    }

    /// <summary>
    /// Spawns a vehicle at the specified position and rotation
    /// </summary>
    /// <param name="vehiclePrefab">The vehicle prefab to spawn</param>
    /// <param name="position">The position to spawn the vehicle at</param>
    /// <param name="rotation">The rotation to spawn the vehicle with</param>
    private void SpawnVehicle(GameObject vehiclePrefab, Vector3 position, Quaternion rotation)
    {
        // Destroy the current vehicle if it exists
        if (currentVehicle != null)
        {
            Destroy(currentVehicle);
        }

        // Apply the spawn offset
        Vector3 spawnPosition = position + spawnOffset;

        // Instantiate the new vehicle
        currentVehicle = Instantiate(vehiclePrefab, spawnPosition, rotation);

        // Set the parent if specified
        if (vehicleParent != null)
        {
            currentVehicle.transform.SetParent(vehicleParent);
        }

        // Make sure the vehicle is active and visible
        currentVehicle.SetActive(true);

        // Reset the scale (since the prefab might have scale 0,0,0 from the CharacterProperty Awake method)
        currentVehicle.transform.localScale = Vector3.one;

        // Set the camera target to follow the spawned vehicle
        if (cameraController != null)
        {
            // Use the existing camera controller's target property
            cameraController.target = currentVehicle.transform;
            Debug.Log("Camera target set to spawned vehicle: " + currentVehicle.name);
        }
        else
        {
            Debug.LogWarning("Camera controller not assigned. Cannot set camera target.");
        }

        // Disable the UI camera when driving
        if (uiCamera != null)
        {
            uiCamera.enabled = false;
            Debug.Log("UI camera disabled for driving in VehicleSpawnManager");
        }

        Debug.Log("Vehicle spawned successfully: " + vehiclePrefab.name);
    }

    /// <summary>
    /// Public method to manually spawn a vehicle
    /// </summary>
    public void ManuallySpawnVehicle()
    {
        if (characterSelector != null)
        {
            // Call the SpawnSelectedVehicle method
            characterSelector.SpawnSelectedVehicle();
        }
        else
        {
            Debug.LogError("CharacterSelector not found in the scene!");
        }
    }
}