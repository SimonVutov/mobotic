using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TorqueDisplay : MonoBehaviour
{
    public vehicleController[] vehicles; // Array of vehicleController scripts
    public GameObject torqueTextPrefab; // Assign a Text or TextMeshPro prefab in the Inspector
    public Canvas canvas; // Reference to the canvas in the scene
    public float lerpSpeed = 5.0f; // Speed of the lerp for smooth torque display

    private List<Wheel> allWheels = new List<Wheel>();      // Flattened list of all wheels
    private List<GameObject> torqueTextObjects = new List<GameObject>();
    private List<float> displayedTorques = new List<float>();

    void Start()
    {
        if (vehicles == null || vehicles.Length == 0)
        {
            Debug.LogError("VehicleController script references are missing.");
            return;
        }

        if (canvas == null)
        {
            canvas = FindObjectOfType<Canvas>();
            if (canvas == null)
            {
                // Create a new canvas if none exists in the scene
                GameObject canvasObj = new GameObject("Canvas");
                canvas = canvasObj.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }
        }

        // Flatten all wheels into a single list
        foreach (var vehicle in vehicles)
        {
            foreach (var wheel in vehicle.wheels) // These are likely "wheel" controllers that contain multiple Wheel objects
            {
                foreach (var w in wheel.wheels) // w is the actual Wheel object
                {
                    if (w != null)
                        allWheels.Add(w);
                }
            }
        }

        // Create text objects for all wheels
        foreach (var w in allWheels)
        {
            GameObject torqueTextObj = Instantiate(torqueTextPrefab, canvas.transform);
            torqueTextObjects.Add(torqueTextObj);
            displayedTorques.Add(0f);
        }
    }

    void Update()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("No main camera found.");
            return;
        }

        int textIndex = 0;
        for (int i = 0; i < allWheels.Count; i++)
        {
            Wheel w = allWheels[i];
            if (w == null)
            {
                continue;
            }

            string displayText;
            if (w.wheelState == 0 || w.wheelState == 2)
            {
                displayText = "-";
            }
            else
            {
                // Smoothly lerp the displayed torque value toward the actual torque value
                displayedTorques[textIndex] = Mathf.Lerp(displayedTorques[textIndex], w.torque, Time.deltaTime * lerpSpeed);

                int roundedTorque = Mathf.RoundToInt(displayedTorques[textIndex]);
                displayText = $"Torque: {roundedTorque} Nm";
            }

            // Convert the wheel's world position to screen space
            // Adjust as needed if you have a reference to the wheel's transform.
            // ... Other code unchanged ...

            // Convert the wheel's world position to screen space using the wheelObject transform
            Vector3 wheelWorldPos = w.wheelObject.transform.position;
            Vector3 targetScreenPosition = mainCamera.WorldToScreenPoint(wheelWorldPos + Vector3.up * 0.5f);

            // Lerp the text object's position
            RectTransform textRectTransform = torqueTextObjects[textIndex].GetComponent<RectTransform>();
            textRectTransform.position = Vector3.Lerp(textRectTransform.position, targetScreenPosition, Time.deltaTime * lerpSpeed);

            // ... Rest of the code ...

            // Update the UI text
            Text textComponent = torqueTextObjects[textIndex].GetComponent<Text>();
            if (textComponent != null)
            {
                textComponent.text = displayText;
            }

            TMP_Text tmpTextComponent = torqueTextObjects[textIndex].GetComponent<TMP_Text>();
            if (tmpTextComponent != null)
            {
                tmpTextComponent.text = displayText;
            }

            textIndex++;
        }
    }
}