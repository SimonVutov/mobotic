using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class gameManager : MonoBehaviour
{
    // List of vehicles
    public List<vehicle> vehicles;

    // UI elements
    public GameObject buttonPrefab;
    public Transform buttonContainer; // Parent container where buttons will be instantiated
    public GameObject uiPanel; // Panel that will be hidden after vehicle spawn

    // Add this field at the top with other public variables
    public CameraController cameraController;

    private GameObject currentVehicle; // Add this field at the top with other variables

    // Start is called before the first frame update
    void Start()
    {
        // Create UI buttons for each vehicle
        foreach (var v in vehicles)
        {
            CreateVehicleButton(v);
        }
    }

    // Create a UI button for the vehicle
    void CreateVehicleButton(vehicle v)
    {
        // Instantiate the button prefab
        GameObject button = Instantiate(buttonPrefab, buttonContainer);

        // Get the button component and setup position
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        rectTransform.anchoredPosition = new Vector2(0, -100 * vehicles.IndexOf(v)); // Offset each button by 100 pixels vertically

        // Get the button component
        Button btn = button.GetComponent<Button>();

        // Find the Text component specifically in children
        Text buttonText = button.GetComponentInChildren<Text>(true);
        if (buttonText != null)
        {
            buttonText.text = v.name;
        }
        else
        {
            Debug.LogError("No Text component found in button prefab!");
        }

        // Find the Image component (excluding the button's own image component)
        Image buttonImage = button.GetComponentsInChildren<Image>()[1]; // Get the second Image component
        if (buttonImage != null)
        {
            buttonImage.sprite = v.image;
        }
        else
        {
            Debug.LogError("No Image component found for vehicle sprite!");
        }

        // Make sure the button is active and properly sized
        button.SetActive(true);

        // Add click listener
        btn.onClick.AddListener(() => OnVehicleSelected(v));
    }

    // When a vehicle is selected, spawn it and hide the UI
    void OnVehicleSelected(vehicle v)
    {
        // Destroy the current vehicle if it exists
        if (currentVehicle != null)
        {
            Debug.Log("Attempting to destroy current vehicle");

            // First handle all forklift scripts
            forklift[] forklifts = currentVehicle.GetComponents<forklift>();
            foreach (forklift fork in forklifts)
            {
                if (fork != null)
                {
                    fork.DestroyForklift();
                }
            }

            // Then destroy the game object
            Destroy(currentVehicle);
        }

        // Spawn the vehicle in the scene and store the reference
        currentVehicle = Instantiate(v.vehiclePrefab, Vector3.zero, Quaternion.identity);

        // Set the camera controller's target to the spawned vehicle
        if (cameraController != null)
        {
            cameraController.SetTarget(currentVehicle.transform);
        }
        else
        {
            Debug.LogError("Camera Controller reference not set in Game Manager!");
        }

        // Hide the UI panel
        uiPanel.SetActive(false);
    }

    void Update()
    {
        // Show selector menu when Escape or X is pressed
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
        {
            uiPanel.SetActive(true);

            // Optional: You might want to show the cursor when the menu is open
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}

[System.Serializable]
public class vehicle
{
    public Sprite image; // The image of the vehicle
    public string name;  // The name of the vehicle
    public GameObject vehiclePrefab; // The vehicle prefab to be spawned
}
