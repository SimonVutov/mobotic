using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class wheel : MonoBehaviour
{
    public GameObject poweredWheelPrefab;   // Prefab for wheelState = 1 (Powered)
    public GameObject freeWheelPrefab;     // Prefab for wheelState = 0 (Free)
    public GameObject rollingWheelPrefab;  // Prefab for wheelState = 2 (Free rolling)
    public Wheel[] wheels;
    private Rigidbody rb;
    [HideInInspector]
    public Vector2 input = Vector2.zero;
    private float suspensionForceClamp = 200f;
    private float dampAmount = 2.5f;
    vehicleController vc;

    private void OnValidate()
    {
        if (wheels == null) return;

        foreach (var wheel in wheels)
        {
            if (wheel == null || wheel.size != 0.0) continue;

            if (wheel.size == 0) wheel.size = 0.3f;
            if (wheel.suspensionForce == 0) wheel.suspensionForce = 270.0f;
            if (wheel.maxGrip == 0) wheel.maxGrip = 400.0f;
            if (wheel.frictionCo == 0) wheel.frictionCo = 1f;
            if (wheel.maxTorque == 0) wheel.maxTorque = 90.0f;
        }
    }

    void Start()
    {
        vc = transform.root.GetComponent<vehicleController>();
        // Create wheel objects
        if (wheels != null)
        {
            for (int i = 0; i < wheels.Length; i++)
            {
                if (wheels[i].parent == null && this.GetComponent<Rigidbody>() != null)
                {
                    wheels[i].parent = this.gameObject;
                } else if (wheels[i].parent == null)
                {
                    //move up parnet until found object with Rigidbody
                    Transform parent = transform;
                    while (parent != null && parent.GetComponent<Rigidbody>() == null)
                    {
                        parent = parent.parent;
                    }
                    if (parent == null)
                    {
                        Debug.LogError("No Rigidbody component found on the GameObject or its parent.");
                        return;
                    }
                    wheels[i].parent = parent.gameObject;
                }

                // Convert localPosition from main body to parent-relative position
                Vector3 parentRelativePosition = wheels[i].parent.transform.InverseTransformPoint(transform.TransformPoint(wheels[i].localPosition));
                wheels[i].localPosition = parentRelativePosition;

                // Select the appropriate prefab based on wheelState
                GameObject selectedPrefab = GetPrefabForWheelState(wheels[i].wheelState);
                if (selectedPrefab == null)
                {
                    Debug.LogError($"No prefab assigned for wheel state {wheels[i].wheelState}");
                    continue;
                }

                // Instantiate the wheel object and parent it to the specified parent transform
                wheels[i].wheelObject = Instantiate(selectedPrefab, wheels[i].parent.transform);
                wheels[i].wheelObject.transform.localPosition = wheels[i].localPosition;
                wheels[i].wheelObject.transform.eulerAngles = transform.eulerAngles;

                // Scale the wheel object based on the size of the wheel
                wheels[i].wheelObject.transform.localScale = 2 * new Vector3(wheels[i].size, wheels[i].size, wheels[i].size);

                // Flip the wheel visual if flipX is true
                Transform wheelVisual = wheels[i].wheelObject.transform.GetChild(0); // Assuming the visual is the first child
                if (wheels[i].flipX)
                {
                    Vector3 visualScale = wheelVisual.localScale;
                    visualScale.x = -Mathf.Abs(visualScale.x); // Ensure flipping along the X-axis
                    wheelVisual.localScale = visualScale;
                }

                // Initialize wheel properties
                wheels[i].wheelCircumference = 2 * Mathf.PI * wheels[i].size;
                wheels[i].parentRigidbody = wheels[i].parent.GetComponent<Rigidbody>();
            }
        }
        // if no Rigidbody is attached to the GameObject, go to parent until found
        if (rb == null)
        {
            rb = GetComponentInParent<Rigidbody>();
        }
        else if (rb == null)
        {
            Debug.LogError("No Rigidbody component found on the GameObject or its parent.");
        }
        else
        {
            rb = GetComponent<Rigidbody>();
        }
    }

    void Update()
    {
        //input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
    }

    // FixedUpdate is used for physics calculations
    void FixedUpdate()
    {
        float dragCoefficient = 0.0f; // Adjust based on the vehicle type
        Vector3 dragForce = -dragCoefficient * rb.velocity.sqrMagnitude * rb.velocity.normalized * Time.fixedDeltaTime;
        rb.AddForce(dragForce);

        for (int i = 0; i < wheels.Length; i++)
        {
            var wheel = wheels[i];
            GameObject wheelObj = wheels[i].wheelObject; // This is the parent GameObject
            Transform wheelVisual = wheelObj.transform.GetChild(0); // Assuming the visual component is the first child
            int ws01 = wheel.wheelState == 1 ? 1 : 0;

            // Update wheel rotations per second due to input
            wheel.torque = Mathf.Clamp(input.y + input.x * wheel.biDirectional, -1, 1) * wheel.maxTorque;
            wheel.hitPointForce = 0.08f * wheel.torque / wheel.size * (vc.sectionCount / vc.massInKg);
            wheel.rotationsPerSecond += wheel.hitPointForce * Time.fixedDeltaTime / wheel.wheelCircumference;

            float turnAngle = input.x * wheel.turnAngle;

            // Update wheel world position
            wheel.wheelWorldPosition = wheels[i].parent.transform.TransformPoint(wheel.localPosition);

            // Calculate slip direction based on local velocity
            Vector3 localVelocity = transform.InverseTransformDirection(rb.GetPointVelocity(wheel.wheelWorldPosition));
            wheel.localSlipDirection = 56f * new Vector3(
                -localVelocity.x * ((wheel.wheelState == 1 || wheel.wheelState == 2) ? 1 : 0) + wheel.rotationsPerSecond * wheels[i].wheelCircumference * Mathf.Sin(turnAngle * Mathf.Deg2Rad) * ws01,
                0,
                -localVelocity.z * ws01 + wheel.rotationsPerSecond * wheels[i].wheelCircumference * Mathf.Cos(turnAngle * Mathf.Deg2Rad) * ws01
            );
            wheel.worldSlipDirection = Vector3.ClampMagnitude(transform.rotation * wheel.localSlipDirection, wheel.maxGrip);

            // Raycast to check wheel contact and apply forces
            RaycastHit hit;
            Physics.Raycast(wheel.wheelWorldPosition, -transform.up, out hit, wheel.size * 2);
            if (hit.collider != null)
            {
                wheel.suspensionForceDirection = hit.normal * Mathf.Clamp(wheel.size * 2 - hit.distance + Mathf.Clamp(wheel.lastSuspensionLength - hit.distance, -1, 1) * dampAmount, 0, Mathf.Infinity) * wheel.suspensionForce * Time.fixedDeltaTime * 50;
                wheel.suspensionForceDirection = Vector3.ClampMagnitude(wheel.suspensionForceDirection, suspensionForceClamp);

                // Apply forces to the parent Rigidbody
                wheel.parentRigidbody.AddForceAtPosition(wheel.suspensionForceDirection + wheel.worldSlipDirection, wheel.wheelWorldPosition);

                wheelObj.transform.position = hit.point + transform.up * wheel.size;
            }
            else
            {
                wheelObj.transform.position = wheel.wheelWorldPosition - transform.up * wheel.size;
            }
            wheel.lastSuspensionLength = hit.distance;

            Debug.Log(wheel.hitPointForce + " " + wheel.worldSlipDirection + " speed: " + rb.velocity.magnitude);

            // Rotate the wheel visual for spinning motion
            Vector3 forwardInWheelSpace = wheelObj.transform.InverseTransformDirection(
                wheel.parentRigidbody.GetPointVelocity(wheel.wheelWorldPosition)
            );

            float wheelRotationSpeed = forwardInWheelSpace.z * 360 / wheel.wheelCircumference;
            wheelVisual.Rotate(Vector3.right, wheelRotationSpeed * Time.fixedDeltaTime, Space.Self);

            if (wheel.wheelState == 1)
            {
                Quaternion targetRotation = Quaternion.Euler(0, input.x * wheel.turnAngle, 0);
                wheelObj.transform.localRotation = Quaternion.Lerp(wheelObj.transform.localRotation, targetRotation, Time.fixedDeltaTime * 100);
            }
            else if (wheel.wheelState == 0 && rb.velocity.magnitude > 0.04f && hit.point != Vector3.zero)
            {
                Quaternion targetRotation = Quaternion.LookRotation(rb.GetPointVelocity(wheel.wheelWorldPosition), transform.up);
                wheelObj.transform.rotation = Quaternion.Lerp(wheelObj.transform.rotation, targetRotation, Time.fixedDeltaTime * 100);
            }
        }
    }

    private GameObject GetPrefabForWheelState(int wheelState)
    {
        return wheelState switch
        {
            0 => freeWheelPrefab,
            1 => poweredWheelPrefab,
            2 => rollingWheelPrefab,
            _ => null
        };
    }

    private void OnDrawGizmos()
    {
        if (wheels != null)
        {
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
}

// Extend the wheel class to include additional properties and methods

[System.Serializable]
public class Wheel
{
    public int wheelState = 1; // 0 = fully free, 1 = powered, 2 = free rolling
    public float biDirectional = 0; // If the wheel can go in reverse to turn the vehicle
    public float frictionCo = 1f; // Adjust this value as needed
    public Vector3 localPosition;
    public float size = 0.3f;
    public float suspensionForce = 90.0f;
    public float turnAngle = 0.0f;
    [HideInInspector]
    public float lastSuspensionLength = 0.0f;
    [HideInInspector]
    public float rotationsPerSecond = 0.0f;
    [HideInInspector]
    public Vector3 localSlipDirection;
    [HideInInspector]
    public Vector3 worldSlipDirection;
    [HideInInspector]
    public Vector3 suspensionForceDirection;
    [HideInInspector]
    public Vector3 wheelWorldPosition;
    [HideInInspector]
    public float wheelCircumference;
    [HideInInspector]
    public float torque = 0.0f;
    public float maxTorque = 300.0f;
    public GameObject parent;
    [HideInInspector]
    public Rigidbody parentRigidbody;
    public bool flipX = false;
    [HideInInspector]
    public GameObject wheelObject; // The actual wheel GameObject
    [HideInInspector]
    public float hitPointForce;
    public float maxGrip = 250f;
}