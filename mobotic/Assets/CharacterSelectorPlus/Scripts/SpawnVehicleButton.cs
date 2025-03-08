using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SpawnVehicleButton : MonoBehaviour
{
    // Optional custom spawn position for this button
    public Vector3 customSpawnPosition = Vector3.zero;

    // Whether to use the custom spawn position
    public bool useCustomSpawnPosition = false;

    // Reference to the UI camera
    public Camera uiCamera;

    private Button button;
    private CharacterSelector characterSelector;

    private void Awake()
    {
        button = GetComponent<Button>();
        characterSelector = FindObjectOfType<CharacterSelector>();

        if (characterSelector == null)
        {
            Debug.LogError("CharacterSelector not found in the scene!");
            button.interactable = false;
        }
        else
        {
            button.onClick.AddListener(SpawnVehicle);

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
    }

    private void Update()
    {
        // Check for Escape or X key to return to menu
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
        {
            ReturnToMenu();
        }
    }

    private void OnDestroy()
    {
        if (button != null)
        {
            button.onClick.RemoveListener(SpawnVehicle);
        }
    }

    /// <summary>
    /// Spawns the currently selected vehicle
    /// </summary>
    public void SpawnVehicle()
    {
        if (characterSelector != null)
        {
            // Set custom spawn position if needed
            if (useCustomSpawnPosition)
            {
                characterSelector.vehicleSpawnPoint = customSpawnPosition;
            }

            // Spawn the vehicle
            characterSelector.SpawnSelectedVehicle();

            // Disable the UI camera when driving
            if (uiCamera != null)
            {
                uiCamera.enabled = false;
                Debug.Log("UI camera disabled for driving in SpawnVehicleButton");
            }

            // Hide the menu
            ManagerSelector menuManager = FindObjectOfType<ManagerSelector>();
            if (menuManager != null)
            {
                menuManager.HideMenu();
            }
        }
    }

    /// <summary>
    /// Returns to the menu and deletes the spawned vehicle
    /// </summary>
    public void ReturnToMenu()
    {
        // Enable the UI camera when returning to menu
        if (uiCamera != null)
        {
            uiCamera.enabled = true;
            Debug.Log("UI camera enabled for menu in SpawnVehicleButton");
        }

        if (characterSelector != null)
        {
            characterSelector.ReturnToMenuAndDeleteVehicle();
        }
        else
        {
            // Find and activate the menu canvas
            ManagerSelector menuManager = FindObjectOfType<ManagerSelector>();
            if (menuManager != null)
            {
                menuManager.ShowMenu();
            }
        }
    }
}