using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class TorqueDisplay : MonoBehaviour
{
    [Header("Setup")]
    public GameObject rootVehicle;          // Root Vehicle GameObject
    public GameObject torqueTextPrefab;     // Assign a Text or TMP Text prefab in the Inspector
    public Canvas canvas;                   // Reference to the canvas in the scene
    public float lerpSpeed = 5.0f;          // Speed of the lerp for smooth torque display

    private vehicleController[] vehicles;   
    private List<Wheel> allWheels = new List<Wheel>();
    private List<GameObject> torqueTextObjects = new List<GameObject>();
    private List<float> displayedTorques = new List<float>();

    void Start()
    {
        // 1. Verify we have a root vehicle:
        if (rootVehicle == null)
        {
            Debug.LogError("rootVehicle is not assigned.");
            return;
        }

        // 2. Gather all vehicleController scripts in the rootVehicle hierarchy:
        vehicles = rootVehicle.GetComponentsInChildren<vehicleController>(true); 
        if (vehicles == null || vehicles.Length == 0)
        {
            Debug.LogWarning("No vehicleController scripts found in the rootVehicle hierarchy.");
            return;
        }

        // 3. Ensure we have a Canvas:
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

        // 4. Flatten all wheels from all vehicleControllers into a single list
        foreach (var vehicle in vehicles)
        {
            if (vehicle == null) continue;
            // Suppose each 'vehicleController' has an array/list of "wheel" scripts,
            // and each "wheel" script has an array of "Wheel" objects inside it:
            foreach (var wheelScript in vehicle.wheels) // 'wheelScript' presumably has `Wheel[] wheels`
            {
                foreach (var w in wheelScript.wheels)
                {
                    if (w != null)
                        allWheels.Add(w);
                }
            }
        }

        // 5. Create text objects for all wheels
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
                displayText = "-";   // e.g. free wheel or rolling wheel?
            }
            else
            {
                // Smoothly lerp the displayed torque toward the actual torque
                displayedTorques[textIndex] = Mathf.Lerp(
                    displayedTorques[textIndex], 
                    w.torque, 
                    Time.deltaTime * lerpSpeed
                );
                int roundedTorque = Mathf.RoundToInt(displayedTorques[textIndex]);
                displayText = $"Torque: {roundedTorque} Nm";
            }

            // Convert the wheel's world position to screen space
            Vector3 wheelWorldPos = w.wheelObject.transform.position;
            Vector3 targetScreenPosition = mainCamera.WorldToScreenPoint(wheelWorldPos + Vector3.up * 0.5f);

            // Lerp the text object's position
            RectTransform textRectTransform = torqueTextObjects[textIndex].GetComponent<RectTransform>();
            textRectTransform.position = Vector3.Lerp(
                textRectTransform.position,
                targetScreenPosition,
                Time.deltaTime * lerpSpeed
            );

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