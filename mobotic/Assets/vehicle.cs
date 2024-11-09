using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    public poweredWheel[] wheels;
    public GameObject wheelPrefab;
    GameObject[] wheelObjects;
    Rigidbody rb;
    Vector2 input = Vector2.zero;
    private float suspensionForceClamp = 200f;
    private float dampAmount = 2.5f;

    private void OnValidate()
    {
        if (wheels == null) return;

        foreach (var wheel in wheels)
        {
            if (wheel == null || wheel.size != 0.0) continue;

            if (wheel.size == 0) wheel.size = 0.3f;
            if (wheel.power == 0) wheel.power = 4.0f;
            if (wheel.suspensionForce == 0) wheel.suspensionForce = 90.0f;
            if (wheel.grip == 0) wheel.grip = 6.0f;
            if (wheel.maxGrip == 0) wheel.maxGrip = 400.0f;
            if (wheel.frictionCo == 0) wheel.frictionCo = 1f;
        }
    }

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

                wheels[i].wheelCircumference = 2 * Mathf.PI * wheels[i].size;
            }
        }
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    // FixedUpdate is used for physics calculations
    void FixedUpdate() {
        float dragCoefficient = 0.3f; // Adjust based on the vehicle type
        Vector3 dragForce = -dragCoefficient * rb.velocity.sqrMagnitude * rb.velocity.normalized;
        rb.AddForce(dragForce);
        for (int i = 0; i < wheels.Length; i++) {
            var wheel = wheels[i];
            GameObject wheelObj = wheelObjects[i]; // This is the parent GameObject
            Transform wheelVisual = wheelObj.transform.GetChild(0); // Assuming the visual component is the first child
            int ws01 = wheel.wheelState == 1 ? 1 : 0;

            // Update wheel rotations per second
            // Update wheel rotations per second due to input
            float inputTorque = Mathf.Clamp(input.y + input.x * wheel.biDirectional, -1, 1) * wheel.power;
            wheel.rotationsPerSecond += inputTorque * Time.fixedDeltaTime / wheel.wheelCircumference;

            // Apply friction to decrease wheel rotations per second
            float frictionDeceleration = wheel.frictionCo * Mathf.Abs(wheel.rotationsPerSecond * wheel.rotationsPerSecond);
            if (wheel.rotationsPerSecond > 0)
            {
                wheel.rotationsPerSecond = Mathf.Max(wheel.rotationsPerSecond - frictionDeceleration * Time.fixedDeltaTime, 0);
            }
            else if (wheel.rotationsPerSecond < 0)
            {
                wheel.rotationsPerSecond = Mathf.Min(wheel.rotationsPerSecond + frictionDeceleration * Time.fixedDeltaTime, 0);
            }
            wheel.rotationsPerSecond -= wheel.localSlipDirection.z * 0.001f;

            float turnAngle = input.x * wheel.turnAngle;

            // Update wheel world position
            wheel.wheelWorldPosition = transform.TransformPoint(wheel.localPosition);

            // Calculate slip direction based on local velocity
            Vector3 localVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(wheel.wheelWorldPosition));
            wheel.localSlipDirection = wheel.grip * new Vector3(
                -localVelocity.x * ((wheel.wheelState == 1 || wheel.wheelState == 2) ? 1 : 0) + wheel.rotationsPerSecond * wheels[i].wheelCircumference * Mathf.Sin(turnAngle * Mathf.Deg2Rad) * ws01,
                0,
                -localVelocity.z * ws01 + wheel.rotationsPerSecond * wheels[i].wheelCircumference * Mathf.Cos(turnAngle * Mathf.Deg2Rad) * ws01
            );
            wheel.worldSlipDirection = Vector3.ClampMagnitude(transform.rotation * wheel.localSlipDirection, wheel.maxGrip);

            // Raycast to check wheel contact and apply forces
            RaycastHit hit;
            Physics.Raycast(wheel.wheelWorldPosition, -transform.up, out hit, wheel.size * 2);
            if (hit.collider != null) {
                wheel.suspensionForceDirection = transform.up * Mathf.Clamp(wheel.size * 2 - hit.distance + Mathf.Clamp(wheel.lastSuspensionLength - hit.distance, -1, 1) * dampAmount, 0, Mathf.Infinity) * wheel.suspensionForce;
                wheel.suspensionForceDirection = Vector3.ClampMagnitude(wheel.suspensionForceDirection, suspensionForceClamp);
                rb.AddForceAtPosition(wheel.suspensionForceDirection + wheel.worldSlipDirection, wheel.wheelWorldPosition);
                wheelObj.transform.position = hit.point + transform.up * wheel.size;
            } else wheelObj.transform.position = wheel.wheelWorldPosition - transform.up * wheel.size;
            wheel.lastSuspensionLength = hit.distance;

            // Rotate the wheel visual for spinning motion
            // Calculate the wheel's local rotation speed based on its frame of reference
            Vector3 forwardInWheelSpace = wheelObj.transform.InverseTransformDirection(rb.GetPointVelocity(wheel.wheelWorldPosition));

            float wheelRotationSpeed = (wheel.wheelState == 1) ? 
                        wheel.rotationsPerSecond * 360 : (wheel.wheelState == 1) ?
                        Mathf.Abs(forwardInWheelSpace.z) * 360 / wheel.wheelCircumference :
                        forwardInWheelSpace.z * 360 / wheel.wheelCircumference;
                        
            // Apply the rotation to the visual wheel
            wheelVisual.Rotate(Vector3.forward, wheelRotationSpeed * Time.fixedDeltaTime, Space.Self);

            // Adjust steering rotation smoothly
            if (wheel.wheelState == 1) {
                Quaternion targetRotation = Quaternion.Euler(0, turnAngle, 0);
                wheelObj.transform.localRotation = Quaternion.Lerp(wheelObj.transform.localRotation, targetRotation, Time.fixedDeltaTime * 100);
            } else if (wheel.wheelState == 0 && rb.velocity.magnitude > 0.04f) {
                // make wheel point in the direction of motion
                Quaternion targetRotation = Quaternion.LookRotation(rb.GetPointVelocity(wheel.wheelWorldPosition), transform.up);
                wheelObj.transform.rotation = Quaternion.Lerp(wheelObj.transform.rotation, targetRotation, Time.fixedDeltaTime * 100);
            }
        }
    }

    private void OnDrawGizmos() {
        if (wheels != null) {
            foreach (var wheel in wheels) {
                // Calculate the world position from the local position
                Vector3 worldPosition = transform.TransformPoint(wheel.localPosition);
                
                // Draw a green sphere at the wheel's world position
                Gizmos.color = Color.green;
                Gizmos.DrawSphere(worldPosition, 0.1f);

                // Draw the suspension and slip direction based on their world positions
                if (wheel.suspensionForceDirection != Vector3.zero) {
                    Gizmos.color = Color.blue;
                    Gizmos.DrawLine(worldPosition, worldPosition + wheel.suspensionForceDirection);
                }

                if (wheel.worldSlipDirection != Vector3.zero) {
                    Gizmos.color = Color.red;
                    Gizmos.DrawLine(worldPosition, worldPosition + wheel.worldSlipDirection);
                }
            }
        }
    }
}