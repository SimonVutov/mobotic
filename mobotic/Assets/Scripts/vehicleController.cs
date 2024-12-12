using System.Collections.Generic;
using UnityEngine;

public class vehicleController : MonoBehaviour
{
    public List<wheel> wheels; // Reference to multiple Wheel scripts
    public Vector2 input; // Input for the vehicle (e.g., steering and throttle)

    private Rigidbody rb;

    private void Start()
    {
        // Initialize Rigidbody
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("No Rigidbody component found on the VehicleController.");
            return;
        }

        // Ensure all wheels have proper parent Rigidbody setup
        foreach (wheel whee in wheels)
        {
            foreach (Wheel w in whee.wheels)
            {
                if (w == null)
                {
                    Debug.LogError("Wheel reference is missing.");
                    continue;
                }

                // Automatically assign parent Rigidbody if not set
                if (w.parent == null)
                {
                    w.parent = gameObject;
                }

                Rigidbody wheelRb = w.parent.GetComponent<Rigidbody>();
                if (wheelRb == null)
                {
                    Debug.LogError($"Parent of wheel {w} does not have a Rigidbody.");
                }
                else
                {
                    w.parentRigidbody = wheelRb;
                }
            }
        }
    }

    private void Update()
    {
        // Process input from the player (e.g., using Unity's Input system)
        input.x = Input.GetAxis("Horizontal"); // Steering
        input.y = Input.GetAxis("Vertical");   // Throttle/Brake

        // Set input for each wheel
        foreach (wheel wheel in wheels)
        {
            if (wheel == null) continue;
            wheel.input = input;
        }
    }
}