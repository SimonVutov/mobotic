using UnityEngine;
using UnityEngine.UI; // Include if using standard UI Text
using TMPro; // Include if using TextMeshPro
using System.Collections.Generic;

public class TorqueDisplay : MonoBehaviour
{
    public Vehicle[] vehicles; // Array of Vehicle scripts
    public GameObject torqueTextPrefab; // Assign a Text prefab in the Inspector
    public Canvas canvas; // Reference to the canvas in the scene
    public float lerpSpeed = 5.0f; // Speed of the lerp for smooth torque display

    private List<GameObject> torqueTextObjects = new List<GameObject>();
    private List<float> displayedTorques = new List<float>(); // Store displayed torque values for each wheel

    void Start()
    {
        if (vehicles == null || vehicles.Length == 0)
        {
            Debug.LogError("Vehicle script references are missing.");
            return;
        }

        if (canvas == null)
        {
            Debug.LogError("Canvas reference is missing.");
            return;
        }

        // Instantiate torque text objects for each wheel in each Vehicle
        foreach (var vehicle in vehicles)
        {
            foreach (var wheel in vehicle.wheels)
            {
                // Create a new text object for each wheel and initialize displayed torque
                GameObject torqueTextObj = Instantiate(torqueTextPrefab, canvas.transform);
                torqueTextObjects.Add(torqueTextObj);
                displayedTorques.Add(0f);
            }
        }
    }

    void Update()
    {
        Camera mainCamera = Camera.main; // Reference to the main camera
        int textIndex = 0; // Track the index for torqueTextObjects and displayedTorques

        // Loop through each vehicle and each wheel in that vehicle
        foreach (var vehicle in vehicles)
        {
            for (int i = 0; i < vehicle.wheels.Length; i++)
            {
                if (vehicle.wheels[i] == null || vehicle.wheelObjects[i] == null)
                {
                    continue;
                }
                var wheel = vehicle.wheels[i];
                var wheelObj = vehicle.wheelObjects[i];

                // Check if the wheel is unpowered (wheelState is 0 or 2)
                string displayText;
                if (wheel.wheelState == 0 || wheel.wheelState == 2)
                {
                    displayText = "-";
                }
                else
                {
                    // Smoothly lerp the displayed torque value toward the actual torque value
                    displayedTorques[textIndex] = Mathf.Lerp(displayedTorques[textIndex], wheel.torque, Time.deltaTime * lerpSpeed);

                    // Round to the nearest integer for display
                    int roundedTorque = Mathf.RoundToInt(displayedTorques[textIndex]);
                    displayText = $"Torque: {roundedTorque} Nm";
                }

                // Convert the world position of the wheel to screen space
                Vector3 targetScreenPosition = mainCamera.WorldToScreenPoint(wheelObj.transform.position + Vector3.up * 0.5f);

                // Smoothly lerp the text object's position toward the target screen position
                RectTransform textRectTransform = torqueTextObjects[textIndex].GetComponent<RectTransform>();
                textRectTransform.position = Vector3.Lerp(textRectTransform.position, targetScreenPosition, Time.deltaTime * lerpSpeed);

                // Update the text component with the display text
                Text textComponent = torqueTextObjects[textIndex].GetComponent<Text>();
                if (textComponent != null)
                {
                    textComponent.text = displayText;
                }

                // If using TextMeshPro, update TMP_Text component
                TMP_Text tmpTextComponent = torqueTextObjects[textIndex].GetComponent<TMP_Text>();
                if (tmpTextComponent != null)
                {
                    tmpTextComponent.text = displayText;
                }

                textIndex++; // Move to the next text object and displayed torque value
            }
        }
    }
}