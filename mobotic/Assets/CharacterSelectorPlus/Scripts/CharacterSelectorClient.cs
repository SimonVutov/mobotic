using UnityEngine;

public class CharacterSelectorClient : MonoBehaviour
{

    // Vector3 for the spawn position
    public Vector3 vehicleSpawnPosition = Vector3.zero;

    // Reference to the existing camera controller
    public CameraController cameraController;

    // Reference to the UI camera
    public Camera uiCamera;

    // Reference to the currently spawned vehicle instance
    private GameObject currentVehicleInstance;

    // Reference to the CharacterSelector
    private CharacterSelector characterSelector;

    private void Awake()
    {
        // Find the CharacterSelector in the scene
        characterSelector = FindObjectOfType<CharacterSelector>();
        if (characterSelector != null)
        {
            // Set the spawn point in the CharacterSelector
            characterSelector.vehicleSpawnPoint = vehicleSpawnPosition;

            // Validate the vehicle prefabs array
            if (characterSelector.vehiclePrefabs == null || characterSelector.vehiclePrefabs.Length == 0)
            {
                Debug.LogWarning("Vehicle prefabs array is not set up in CharacterSelector. Vehicle spawning will not work.");
            }
            else if (characterSelector.vehiclePrefabs.Length != characterSelector.prefab.Length)
            {
                Debug.LogWarning("Vehicle prefabs array length does not match display prefabs array length in CharacterSelector. Vehicle spawning may not work correctly.");
            }

            // Share the UI camera reference if it's set here but not in CharacterSelector
            if (uiCamera != null && characterSelector.uiCamera == null)
            {
                characterSelector.uiCamera = uiCamera;
            }
            // Or get the UI camera from CharacterSelector if it's not set here
            else if (uiCamera == null && characterSelector.uiCamera != null)
            {
                uiCamera = characterSelector.uiCamera;
            }
        }
        else
        {
            Debug.LogWarning("CharacterSelector not found in the scene. Vehicle spawning may not work correctly.");
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
    }

    private void Update()
    {
        // Check for Escape or X key to return to menu
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
        {
            if (characterSelector != null)
            {
                characterSelector.ReturnToMenuAndDeleteVehicle();
            }
            else
            {
                DestroyCurrentVehicle();

                // Enable the UI camera when returning to menu
                if (uiCamera != null)
                {
                    uiCamera.enabled = true;
                    Debug.Log("UI camera enabled for menu in CharacterSelectorClient");
                }

                // Find and activate the menu canvas
                ManagerSelector menuManager = FindObjectOfType<ManagerSelector>();
                if (menuManager != null)
                {
                    menuManager.ShowMenu();
                }
            }
        }
    }

    private void OnEnable()
    {
        CharacterSelector.OnCharacterSelected += CharacterSelected;
        CharacterSelector.OnPurchaseCharacter += PurchaseCharacter;
        CharacterSelector.OnVehicleSpawn += HandleVehicleSpawn;
        CharacterSelector.OnReturnToMenu += HandleReturnToMenu;
    }

    private void OnDisable()
    {
        CharacterSelector.OnCharacterSelected -= CharacterSelected;
        CharacterSelector.OnPurchaseCharacter -= PurchaseCharacter;
        CharacterSelector.OnVehicleSpawn -= HandleVehicleSpawn;
        CharacterSelector.OnReturnToMenu -= HandleReturnToMenu;
    }

    // Handle the return to menu event
    private void HandleReturnToMenu()
    {
        DestroyCurrentVehicle();

        // Enable the UI camera when returning to menu
        if (uiCamera != null)
        {
            uiCamera.enabled = true;
            Debug.Log("UI camera enabled for menu in HandleReturnToMenu");
        }
    }

    // Destroy the current vehicle instance
    private void DestroyCurrentVehicle()
    {
        if (currentVehicleInstance != null)
        {
            Destroy(currentVehicleInstance);
            currentVehicleInstance = null;

            // Reset the camera target
            if (cameraController != null)
            {
                cameraController.target = null;
                Debug.Log("Camera target reset when destroying vehicle");
            }

            Debug.Log("Vehicle destroyed when returning to menu");
        }
    }

    private void CharacterSelected(string characterName)
    {
        foreach (Transform child in transform)
        {
            child.gameObject.SetActive(child.gameObject.name == characterName);
        }
    }

    private void PurchaseCharacter(CharacterProperty characterProperty)
    {
        Debug.Log("You must pay " + characterProperty.name + ", for the character: " + characterProperty.nameObj);
        GameObject.Find("CharacterSelectorController").GetComponent<CharacterSelector>().paymentConfirmed(characterProperty);
    }

    private void HandleVehicleSpawn(GameObject vehiclePrefab, Vector3 position, Quaternion rotation)
    {
        // Destroy the current vehicle instance if it exists
        if (currentVehicleInstance != null)
        {
            Destroy(currentVehicleInstance);
        }

        // Instantiate the new vehicle at the specified position
        currentVehicleInstance = Instantiate(vehiclePrefab, position, rotation);

        // Make sure the vehicle is active and visible
        currentVehicleInstance.SetActive(true);

        // Reset the scale (since the prefab might have scale 0,0,0 from the CharacterProperty Awake method)
        currentVehicleInstance.transform.localScale = Vector3.one;

        // Set the camera target to follow the spawned vehicle
        if (cameraController != null)
        {
            // Use the existing camera controller's target property
            cameraController.target = currentVehicleInstance.transform;
            Debug.Log("Camera target set to spawned vehicle: " + currentVehicleInstance.name);
        }
        else
        {
            Debug.LogWarning("Camera controller not assigned. Cannot set camera target.");
        }

        // Disable the UI camera when driving
        if (uiCamera != null)
        {
            uiCamera.enabled = false;
            Debug.Log("UI camera disabled for driving in HandleVehicleSpawn");
        }

        Debug.Log("Vehicle spawned in CharacterSelectorClient: " + vehiclePrefab.name);
    }
}
