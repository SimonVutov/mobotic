using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public poweredWheel[] wheels;
    public freeWheel[] freeWheels;
    public GameObject wheelPrefab;
    [HideInInspector]
    public GameObject[] wheelObjects;
    [HideInInspector]
    float throttle = 0.0f;
    Rigidbody rb;

    // Start is called before the first frame update
    void Start()
    {
        // Create wheel objects
        if (wheels != null) {
            wheelObjects = new GameObject[wheels.Length];
            for (int i = 0; i < wheels.Length; i++) {
                wheelObjects[i] = Instantiate(wheelPrefab, transform);
                wheelObjects[i].transform.localPosition = wheels[i].localPosition;
                // scale the wheel object based on the size of the wheel
                wheelObjects[i].transform.localScale = 2 * new Vector3(wheels[i].size, wheels[i].size, wheels[i].size);
            }
        }
        rb = GetComponent<Rigidbody>();
    }

    // Update is used only for capturing input to avoid any missed inputs
    void Update() {
        float inputThrottle = Input.GetKey(KeyCode.W) ? 1.0f : 0.0f;
        inputThrottle -= Input.GetKey(KeyCode.S) ? 1.0f : 0.0f;
        throttle = inputThrottle;
    }

    // FixedUpdate is used for physics calculations
    void FixedUpdate() {
        float turn = Input.GetAxis("Horizontal");

        for (int i = 0; i < wheels.Length; i++) {
            var wheel = wheels[i];
            GameObject wheelObj = wheelObjects[i]; // This is the parent GameObject
            Transform wheelVisual = wheelObj.transform.GetChild(0); // Assuming the visual component is the first child

            // Update wheel rotations per second
            wheel.rotationsPerSecond += throttle * Time.fixedDeltaTime * wheel.power / (2 * Mathf.PI * wheel.size);
            wheel.rotationsPerSecond *= (1/(1+rb.velocity.magnitude * 0.05f));  // Damping to simulate friction losses

            // Calculate turn angle
            float turnAngle = wheel.turnable ? turn * wheel.turnAngle : 0;

            // Update wheel world position
            wheel.wheelWorldPosition = transform.TransformPoint(wheel.localPosition);

            // Calculate slip direction based on local velocity
            Vector3 localVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(wheel.wheelWorldPosition));
            wheel.localSlipDirection = wheel.grip * new Vector3(
                -localVelocity.x + wheel.rotationsPerSecond * wheel.size * Mathf.PI * 2 * Mathf.Sin(turnAngle * Mathf.Deg2Rad),
                0,
                -localVelocity.z + wheel.rotationsPerSecond * wheel.size * Mathf.PI * 2 * Mathf.Cos(turnAngle * Mathf.Deg2Rad)
            );
            wheel.worldSlipDirection = transform.rotation * wheel.localSlipDirection;

            // Limit slip direction force
            if (wheel.worldSlipDirection.magnitude > wheel.maxGrip)
                wheel.worldSlipDirection = wheel.worldSlipDirection.normalized * wheel.maxGrip;

            // Raycast to check wheel contact and apply forces
            RaycastHit hit;
            if (Physics.Raycast(wheel.wheelWorldPosition, -transform.up, out hit, wheel.size * 2)) {
                wheel.inContact = true;
                float damp = Mathf.Clamp(wheel.lastSuspensionLength - hit.distance, 0, 1);
                wheel.suspensionForceDirection = transform.up * (wheel.size * 2 - hit.distance + damp * 4f) * wheel.suspensionForce;
                rb.AddForceAtPosition(wheel.suspensionForceDirection + wheel.worldSlipDirection, wheel.wheelWorldPosition);
                wheelObj.transform.position = hit.point + transform.up * wheel.size;
            } else {
                wheel.inContact = false;
                wheelObj.transform.position = wheel.wheelWorldPosition - transform.up * wheel.size;
            }
            wheel.lastSuspensionLength = hit.distance;

            // Rotate the wheel visual for spinning motion
            wheelVisual.Rotate(Vector3.forward, wheel.rotationsPerSecond * 360 * Time.fixedDeltaTime, Space.Self);

            // Adjust steering rotation smoothly
            if (wheel.turnable) {
                Quaternion targetRotation = Quaternion.Euler(0, turnAngle, 0);
                wheelObj.transform.localRotation = Quaternion.Lerp(wheelObj.transform.localRotation, targetRotation, Time.fixedDeltaTime * 5);
            }
        }
    }

    // Visualization using Gizmos
    private void OnDrawGizmos() {
        if (wheels != null) {
            foreach (var wheel in wheels) {
                Gizmos.color = Color.green; // Green for powered wheels
                Gizmos.DrawSphere(wheel.wheelWorldPosition, 0.1f);
                Gizmos.color = Color.blue; // Blue for suspension force
                Gizmos.DrawLine(wheel.wheelWorldPosition, wheel.wheelWorldPosition + wheel.suspensionForceDirection);
                Gizmos.color = Color.red; // Red for slip direction
                Gizmos.DrawLine(wheel.wheelWorldPosition, wheel.wheelWorldPosition + wheel.worldSlipDirection);
            }
        }

        if (freeWheels != null) {
            foreach (var wheel in freeWheels) {
                Gizmos.color = Color.red; // Red for free wheels
                Gizmos.DrawSphere(transform.position + wheel.localPosition, wheel.size);
            }
        }
    }
}