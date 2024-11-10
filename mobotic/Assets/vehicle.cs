using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class Vehicle : MonoBehaviour
{
    [Header("Vehicle Properties")]
    public float massInKg = 100.0f;
    public wheel[] wheels;
    public GameObject wheelPrefab;

    [HideInInspector] public GameObject[] wheelObjects;
    private Rigidbody rb;
    private Vector2 input = Vector2.zero;

    [Header("Suspension Properties")]
    private float suspensionForceClamp = 900f;
    private float dampAmount = 2.5f;

    [Header("Other Properties")]
    private float totalParts = 4;

    private void OnValidate()
    {
        if (wheels == null) return;

        // Apply default values to wheel properties where necessary
        foreach (var wheel in wheels)
        {
            if (wheel == null || wheel.size <= 0){
                wheel.size = Mathf.Max(wheel.size, 0.3f);
                wheel.power = Mathf.Max(wheel.power, 4.0f);
                wheel.suspensionForce = Mathf.Max(wheel.suspensionForce, 90.0f);
                wheel.grip = Mathf.Max(wheel.grip, 24.0f);
                wheel.maxGrip = Mathf.Max(wheel.maxGrip, 400.0f);
                wheel.frictionCo = Mathf.Max(wheel.frictionCo, 1f);
                wheel.maxTorque = Mathf.Max(wheel.maxTorque, 30.0f);
            }
        }
    }

    private void Start()
    {
        // Ensure Rigidbody component is attached
        rb = GetComponent<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody component missing from Vehicle.");
            enabled = false;
            return;
        }

        // Instantiate wheel objects if wheels are configured
        if (wheels == null || wheels.Length == 0)
        {
            Debug.LogWarning("No wheels configured for Vehicle.");
            return;
        }

        wheelObjects = new GameObject[wheels.Length];
        for (int i = 0; i < wheels.Length; i++)
        {
            if (wheelPrefab == null)
            {
                Debug.LogError("Wheel prefab not assigned in Vehicle.");
                return;
            }

            // Instantiate and configure each wheel
            wheelObjects[i] = Instantiate(wheelPrefab, transform);
            wheelObjects[i].transform.localPosition = wheels[i].localPosition;
            wheelObjects[i].transform.localScale = 2 * new Vector3(wheels[i].size, wheels[i].size, wheels[i].size);

            // Calculate wheel circumference for rotational calculations
            wheels[i].wheelCircumference = 2 * Mathf.PI * wheels[i].size;
        }
    }

    private void Update()
    {
        // Capture input for throttle and steering
        input = new Vector2(Input.GetAxis("Horizontal") - 2f * transform.InverseTransformDirection(rb.angularVelocity).y, Input.GetAxis("Vertical"));
        Vector2.ClampMagnitude(input, 1);
    }

    // FixedUpdate handles physics calculations
    private void FixedUpdate()
    {
        ApplyDragForce();

        for (int i = 0; i < wheels.Length; i++)
        {
            ProcessWheelPhysics(wheels[i], wheelObjects[i]);
        }
    }

    private void ApplyDragForce()
    {
        // Calculate and apply drag based on vehicle velocity
        float dragCoefficient = 0.04f;
        Vector3 dragForce = -dragCoefficient * rb.velocity.sqrMagnitude * rb.velocity.normalized;
        rb.AddForce(dragForce);
    }

    private void ProcessWheelPhysics(wheel wheel, GameObject wheelObj)
    {
        Transform wheelVisual = wheelObj.transform.GetChild(0);
        int isPoweredWheel = wheel.wheelState == 1 ? 1 : 0;

        // Calculate wheel rotational speed
        float wheelSpeed = transform.InverseTransformDirection(rb.GetPointVelocity(wheel.wheelWorldPosition)).z;
        wheel.rotationsPerSecond = wheelSpeed / wheel.wheelCircumference;

        // Apply a torque reduction curve for a realistic acceleration curve
        float maxRotationsPerSecond = wheel.maxTorque / (wheel.size * massInKg / totalParts);
        float torqueReductionFactor = Mathf.Clamp01(1 - Mathf.Pow(wheel.rotationsPerSecond / maxRotationsPerSecond, 4));

        // Compute throttle input and available torque
        float inputThrottle = Mathf.Clamp(input.y + input.x * wheel.biDirectional, -1, 1);
        float availableTorque = inputThrottle * wheel.maxTorque * torqueReductionFactor;

        // Set the calculated torque on the wheel
        wheel.torque = availableTorque;

        // Calculate rotational acceleration and update wheel rotation
        float rotationalAcceleration = availableTorque / (wheel.size * massInKg / totalParts);
        wheel.rotationsPerSecond += rotationalAcceleration * Time.fixedDeltaTime / wheel.wheelCircumference;

        // Apply friction to wheel rotation
        float frictionDeceleration = wheel.frictionCo * Mathf.Abs(wheel.rotationsPerSecond);
        wheel.rotationsPerSecond -= Mathf.Sign(wheel.rotationsPerSecond) * frictionDeceleration * Time.fixedDeltaTime;

        // Calculate steering angle
        float turnAngle = input.x * wheel.turnAngle;

        // Update wheel position and calculate slip direction based on local velocity
        wheel.wheelWorldPosition = transform.TransformPoint(wheel.localPosition);
        Vector3 localVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(wheel.wheelWorldPosition));
        wheel.localSlipDirection = wheel.grip * new Vector3(
            -localVelocity.x * (isPoweredWheel != 0 ? 1 : 0) + wheel.rotationsPerSecond * wheel.wheelCircumference * Mathf.Sin(turnAngle * Mathf.Deg2Rad) * isPoweredWheel,
            0,
            -localVelocity.z * isPoweredWheel + wheel.rotationsPerSecond * wheel.wheelCircumference * Mathf.Cos(turnAngle * Mathf.Deg2Rad) * isPoweredWheel
        );
        wheel.worldSlipDirection = Vector3.ClampMagnitude(transform.rotation * wheel.localSlipDirection, wheel.maxGrip);

        ApplySuspensionAndRaycastForces(wheel, wheelObj);
        RotateWheelVisual(wheel, wheelObj, wheelVisual, turnAngle, isPoweredWheel);
    }

    private void ApplySuspensionAndRaycastForces(wheel wheel, GameObject wheelObj)
    {
        // Perform a raycast to simulate suspension force
        RaycastHit hit;
        if (Physics.Raycast(wheel.wheelWorldPosition, -transform.up, out hit, wheel.size * 2))
        {
            wheel.suspensionForceDirection = transform.up * Mathf.Clamp(
                wheel.size * 2 - hit.distance + Mathf.Clamp(wheel.lastSuspensionLength - hit.distance, -1, 1) * dampAmount,
                0,
                Mathf.Infinity
            ) * wheel.suspensionForce;
            wheel.suspensionForceDirection = Vector3.ClampMagnitude(wheel.suspensionForceDirection, suspensionForceClamp);
            rb.AddForceAtPosition(wheel.suspensionForceDirection + wheel.worldSlipDirection, wheel.wheelWorldPosition);
            wheelObj.transform.position = hit.point + transform.up * wheel.size;
        }
        else
        {
            wheelObj.transform.position = wheel.wheelWorldPosition - transform.up * wheel.size;
        }
        wheel.lastSuspensionLength = hit.distance;
    }

    private void RotateWheelVisual(wheel wheel, GameObject wheelObj, Transform wheelVisual, float turnAngle, int isPoweredWheel)
    {
        // Rotate the wheel visual for realistic motion
        Vector3 forwardInWheelSpace = wheelObj.transform.InverseTransformDirection(rb.GetPointVelocity(wheel.wheelWorldPosition));
        float wheelRotationSpeed = forwardInWheelSpace.z * 360 / wheel.wheelCircumference;
        wheelVisual.Rotate(Vector3.forward, wheelRotationSpeed * Time.fixedDeltaTime, Space.Self);

        // Adjust the wheel's steering rotation
        if (wheel.wheelState == 1)
        {
            Quaternion targetRotation = Quaternion.Euler(0, turnAngle, 0);
            wheelObj.transform.localRotation = Quaternion.Lerp(wheelObj.transform.localRotation, targetRotation, Time.fixedDeltaTime * 100);
        }
        else if (wheel.wheelState == 0 && rb.velocity.magnitude > 0.04f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(rb.GetPointVelocity(wheel.wheelWorldPosition), transform.up);
            wheelObj.transform.rotation = Quaternion.Lerp(wheelObj.transform.rotation, targetRotation, Time.fixedDeltaTime * 100);
        }
    }

    private void OnDrawGizmos()
    {
        // Visualize wheel suspension and slip directions in the scene view
        if (wheels == null) return;

        foreach (var wheel in wheels)
        {
            Vector3 worldPosition = transform.TransformPoint(wheel.localPosition);
            Gizmos.color = Color.green;
            Gizmos.DrawSphere(worldPosition, 0.1f);

            if (wheel.suspensionForceDirection != Vector3.zero)
            {
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(worldPosition, worldPosition + wheel.suspensionForceDirection);
            }

            if (wheel.worldSlipDirection != Vector3.zero)
            {
                Gizmos.color = Color.red;
                Gizmos.DrawLine(worldPosition, worldPosition + wheel.worldSlipDirection);
            }
        }
    }
}