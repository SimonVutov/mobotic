using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SpawnVehicleButton : MonoBehaviour
{
    // Optional custom spawn position for this button
    public Vector3 customSpawnPosition = Vector3.zero;

    // Whether to use the custom spawn position
    public bool useCustomSpawnPosition = false;

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