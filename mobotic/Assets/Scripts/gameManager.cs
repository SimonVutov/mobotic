using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class gameManager : MonoBehaviour
{
    // Lists
    public List<vehicle> vehicles;
    public List<map> maps;

    // UI elements
    public GameObject buttonPrefab;
    public Transform mapButtonContainer;    // Separate container for map buttons
    public Transform vehicleButtonContainer; // Separate container for vehicle buttons
    public GameObject vehiclePanel;
    public GameObject mapPanel;

    public CameraController cameraController;
    private GameObject currentVehicle;
    private GameObject currentMap;    // Track current map

    private float verticalSpacing = 110f;    // Spacing between rows
    private float horizontalSpacing = 220f;  // Spacing between columns

    private List<GameObject> createdMapButtons = new List<GameObject>();
    private List<GameObject> createdVehicleButtons = new List<GameObject>();

    private float scrollPosition = 0f;
    private float scrollSpeed = 300f;  // Adjust this to control scroll speed
    private float maxScroll = 0f;      // Will be calculated based on number of buttons

    // Start is called before the first frame update
    void Start()
    {
        // Start with map selection, hide vehicle selection
        vehiclePanel.SetActive(false);
        mapPanel.SetActive(true);

        // Create UI buttons for each map
        foreach (var m in maps)
        {
            CreateMapButton(m);
        }

        // Make sure the text objects are visible
        foreach (Transform child in mapPanel.transform)
        {
            if (child.GetComponent<TextMeshProUGUI>() != null)
            {
                child.gameObject.SetActive(true);
            }
        }
    }

    void CreateMapButton(map m)
    {
        GameObject button = Instantiate(buttonPrefab, mapButtonContainer);
        createdMapButtons.Add(button);  // Add to our list of created buttons

        // Calculate row and column
        int index = maps.IndexOf(m);
        int row = index / 2;
        int column = index % 2;

        // Setup button position using grid layout
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        float xPos = (column * horizontalSpacing) - (horizontalSpacing / 2);
        float yPos = -verticalSpacing * row;
        rectTransform.anchoredPosition = new Vector2(xPos, yPos);

        // Setup button text - get TextMeshProUGUI component
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = m.mapReference != null ? m.name : m.name + " (coming soon)";
        }
        else
        {
            Debug.LogError("No TextMeshProUGUI component found in button prefab!");
        }

        // Setup button image - get the second Image component
        Image[] images = button.GetComponentsInChildren<Image>();
        if (images.Length > 1)
        {
            images[1].sprite = m.image;
            images[1].preserveAspect = true;
            images[1].type = Image.Type.Simple;
        }
        else
        {
            Debug.LogError("Not enough Image components found in button prefab!");
        }

        button.SetActive(true);

        // Get button component and set interactable state
        Button btn = button.GetComponent<Button>();
        btn.interactable = m.mapReference != null;

        // Only add click listener if the map reference exists
        if (m.mapReference != null)
        {
            btn.onClick.AddListener(() => OnMapSelected(m));
        }
    }

    void OnMapSelected(map m)
    {
        // Disable all maps first
        foreach (var mapItem in maps)
        {
            if (mapItem.mapReference != null)
            {
                mapItem.mapReference.SetActive(false);
            }
        }

        // Enable selected map
        m.mapReference.SetActive(true);
        currentMap = m.mapReference;

        // Clear only the buttons we created
        foreach (GameObject button in createdMapButtons)
        {
            Destroy(button);
        }
        createdMapButtons.Clear();

        // Switch to vehicle selection, but ensure text stays visible
        mapPanel.SetActive(false);
        vehiclePanel.SetActive(true);

        // Create vehicle buttons
        foreach (var v in vehicles)
        {
            CreateVehicleButton(v);
        }
    }

    void Update()
    {
        // Handle scrolling for active panel
        float scrollInput = -Input.mouseScrollDelta.y;
        if (mapPanel.activeSelf)
        {
            HandleScrolling(scrollInput, mapButtonContainer, maps.Count);
        }
        else if (vehiclePanel.activeSelf)
        {
            HandleScrolling(scrollInput, vehicleButtonContainer, vehicles.Count);
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
        {
            // If we're controlling the vehicle (both panels inactive)
            if (!vehiclePanel.activeSelf && !mapPanel.activeSelf)
            {
                // Show vehicle selection panel
                vehiclePanel.SetActive(true);

                // Create vehicle buttons
                foreach (var v in vehicles)
                {
                    CreateVehicleButton(v);
                }
            }
            // If vehicle panel is active, go back to map selection
            else if (vehiclePanel.activeSelf)
            {
                // Clear only the vehicle buttons we created
                foreach (GameObject button in createdVehicleButtons)
                {
                    Destroy(button);
                }
                createdVehicleButtons.Clear();

                // Disable vehicle panel
                vehiclePanel.SetActive(false);

                // Enable map panel and recreate map buttons
                mapPanel.SetActive(true);
                foreach (var m in maps)
                {
                    CreateMapButton(m);
                }

                // Destroy current vehicle if it exists
                if (currentVehicle != null)
                {
                    forklift[] forklifts = currentVehicle.GetComponents<forklift>();
                    foreach (forklift fork in forklifts)
                    {
                        if (fork != null)
                        {
                            fork.DestroyForklift();
                        }
                    }
                    Destroy(currentVehicle);
                }

                // Disable current map if it exists
                if (currentMap != null)
                {
                    currentMap.SetActive(false);
                    currentMap = null;
                }
            }

            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }

    void HandleScrolling(float scrollInput, Transform container, int itemCount)
    {
        // Calculate max scroll based on number of rows (itemCount / 2 rounded up)
        int rows = (itemCount + 1) / 2;
        maxScroll = Mathf.Max(0, (rows - 2) * 110f);  // 110f is the vertical spacing between rows
                                                      // Subtract 2 rows to account for visible area

        // Update scroll position
        scrollPosition += scrollInput * scrollSpeed * Time.deltaTime;
        scrollPosition = Mathf.Clamp(scrollPosition, 0, maxScroll);

        // Apply scroll position to container
        Vector3 pos = container.localPosition;
        pos.y = scrollPosition;
        container.localPosition = pos;
    }

    // Create a UI button for the vehicle
    void CreateVehicleButton(vehicle v)
    {
        GameObject button = Instantiate(buttonPrefab, vehicleButtonContainer);
        createdVehicleButtons.Add(button);  // Add to our list of created buttons

        // Calculate row and column
        int index = vehicles.IndexOf(v);
        int row = index / 2;
        int column = index % 2;

        // Setup button position using grid layout
        RectTransform rectTransform = button.GetComponent<RectTransform>();
        rectTransform.localScale = Vector3.one;
        float xPos = (column * horizontalSpacing) - (horizontalSpacing / 2);
        float yPos = -verticalSpacing * row;
        rectTransform.anchoredPosition = new Vector2(xPos, yPos);

        // Setup button text - get TextMeshProUGUI component
        TextMeshProUGUI buttonText = button.GetComponentInChildren<TextMeshProUGUI>();
        if (buttonText != null)
        {
            buttonText.text = v.vehiclePrefab != null ? v.name : v.name + " (coming soon)";
        }
        else
        {
            Debug.LogError("No TextMeshProUGUI component found in button prefab!");
        }

        // Setup button image - get the second Image component
        Image[] images = button.GetComponentsInChildren<Image>();
        if (images.Length > 1)
        {
            images[1].sprite = v.image;
            images[1].preserveAspect = true;
            images[1].type = Image.Type.Simple;
        }
        else
        {
            Debug.LogError("Not enough Image components found in button prefab!");
        }

        button.SetActive(true);

        // Get button component and set interactable state
        Button btn = button.GetComponent<Button>();
        btn.interactable = v.vehiclePrefab != null;

        // Only add click listener if the vehicle prefab exists
        if (v.vehiclePrefab != null)
        {
            btn.onClick.AddListener(() => OnVehicleSelected(v));
        }
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
        vehiclePanel.SetActive(false);
    }
}

[System.Serializable]
public class vehicle
{
    public Sprite image; // The image of the vehicle
    public string name;  // The name of the vehicle
    public GameObject vehiclePrefab; // The vehicle prefab to be spawned
}

[System.Serializable]
public class map
{
    public Sprite image; // The image of the vehicle
    public string name;  // The name of the vehicle
    public GameObject mapReference; // The vehicle prefab to be spawned
}
