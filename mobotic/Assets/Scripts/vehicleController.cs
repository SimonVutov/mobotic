using System.Collections.Generic;
using UnityEngine;

public class vehicleController : MonoBehaviour
{
    public float maxSpeed = 15f; // Maximum speed of the vehicle in m/s
    public bool QAandWSControl = false; // if true, the vehicle will be controlled by QA and WS keys
    public float massInKg = 100.0f;
    public int sectionCount = 4;
    public List<wheel> wheels; // Reference to multiple Wheel scripts
    public Vector2 input; // Input for the vehicle (e.g., steering and throttle)

    private Rigidbody rb;
    private bool breaking = false;

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
        input.x = Input.GetAxisRaw("Horizontal"); // Steering
        input.y = Input.GetAxisRaw("Vertical");   // Throttle/Brake

        if (true) { // reduce fast turning, add anti turn
            input.x -= rb.angularVelocity.y * (0.4f + (Mathf.Abs(rb.angularVelocity.y) > 2f ? 1 : 0));
        }
        Vector2.ClampMagnitude(input, 1);

        if (Input.GetKeyDown(KeyCode.Space) || breaking || rb.velocity.magnitude > 5f)
        {
            breaking = true;

            if (rb.velocity.magnitude < 0.1f) breaking = false;
        }

        // Set input for each wheel
        foreach (wheel wheel in wheels)
        {
            if (wheel == null) continue;
            wheel.input = input;

            if (QAandWSControl)
            { // change it so it uses qa and ws for controlling the wheels
                Vector2 newInput = Vector2.zero;
                newInput.x += Input.GetKey(KeyCode.Q) ? 1 : 0;
                newInput.y += Input.GetKey(KeyCode.Q) ? 1 : 0;

                newInput.x -= Input.GetKey(KeyCode.W) ? 1 : 0;
                newInput.y += Input.GetKey(KeyCode.W) ? 1 : 0;

                newInput.x += Input.GetKey(KeyCode.S) ? 1 : 0;
                newInput.y -= Input.GetKey(KeyCode.S) ? 1 : 0;

                newInput.x -= Input.GetKey(KeyCode.A) ? 1 : 0;
                newInput.y -= Input.GetKey(KeyCode.A) ? 1 : 0;
                wheel.input = Vector2.ClampMagnitude(newInput, 1);
            }

            if (breaking && false)
            {
                wheel.input.y = -1;
                wheel.input.x = 0.5f;
            }

            if (rb.velocity.magnitude > maxSpeed)
            {
                if (wheel.Forwards) wheel.input.y = Mathf.Clamp(wheel.input.y, -1, 0);
                else wheel.input.y = Mathf.Clamp(wheel.input.y, 0, 1);
            }
        }


    }
}