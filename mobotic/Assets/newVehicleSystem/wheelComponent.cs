using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelComponent : MonoBehaviour
{
    [Header("Wheel Visual")]
    public Transform wheelVisual;  // Reference to existing wheel visual component
    
    [Header("Wheel Configuration")]
    public bool freeRoll = false;  // If true, wheel will free roll; if false, wheel is powered
    public Vector3 wheelPosition;
    [Range(0.1f, 1f)] public float wheelSize = 0.3f;
    [Range(0f, 90f)] public float turnAngle = 0f;
    public bool flipX = false;
    
    [Header("Physics Properties")]
    [Range(0.1f, 5f)] public float frictionCoefficient = 1f;
    [Range(10f, 1000f)] public float suspensionForce = 90f;
    [Range(10f, 1000f)] public float maxTorque = 300f;
    [Range(10f, 1000f)] public float maxGrip = 250f;
    [Range(0.1f, 30f)] public float wheelGrip = 15f;
    [Range(0.1f, 400f)] public float suspensionForceClamp = 200f;
    [Range(0.1f, 10f)] public float dampingAmount = 2.5f;
    [Range(0.01f, 1.0f)] public float dragCoefficient = 0.3f;
    [Range(0.001f, 0.1f)] public float rollingResistance = 0.015f;

    // Hidden Public Variables
    [HideInInspector] public Rigidbody parentRigidbody;
    [HideInInspector] public Vector2 input = Vector2.zero;
    [HideInInspector] public bool isMovingForward = false;
    [HideInInspector] public vehicleControl vehicleController;

    // Runtime wheel properties
    private float wheelCircumference;
    private Vector3 wheelWorldPosition;
    private Vector3 localVelocity;
    private Vector3 localSlipDirection;
    private Vector3 worldSlipDirection;
    private Vector3 suspensionForceDirection;
    private float lastSuspensionLength = 0f;
    private float torque = 0f;
    private float hitPointForce;
    private Transform wheelObject;

    private void OnValidate()
    {
        // Set default values if not already set
        if (wheelSize <= 0) wheelSize = 0.3f;
        if (suspensionForce <= 0) suspensionForce = 270.0f;
        if (maxGrip <= 0) maxGrip = 400.0f;
        if (frictionCoefficient <= 0) frictionCoefficient = 1f;
        if (maxTorque <= 0) maxTorque = 90.0f;
    }

    private void Start()
    {
        InitializeComponents();
        InitializeWheel();
    }

    private void FixedUpdate()
    {
        ApplyAerodynamicDrag();
        UpdateWheel();
    }

    #region Initialization Methods

    private void InitializeComponents()
    {
        // Find vehicle controller if needed
        if (vehicleController == null)
        {
            vehicleController = transform.root.GetComponent<vehicleControl>();
        }
        
        // Find rigidbody
        parentRigidbody = GetComponentInParent<Rigidbody>();
        if (parentRigidbody == null)
        {
            Debug.LogError("No Rigidbody component found on the GameObject or its parent.");
        }
    }

    private void InitializeWheel()
    {
        // Verify wheel visual is set
        if (wheelVisual == null)
        {
            Debug.LogError("Wheel visual is not assigned. Please assign a Transform for the wheel visual.");
            return;
        }

        // Set up wheel object for physics
        wheelObject = wheelVisual;
        
        // Initialize wheel properties
        wheelCircumference = 2 * Mathf.PI * wheelSize;
        
        // Handle wheel visual flipping
        if (flipX)
        {
            Vector3 visualScale = wheelVisual.localScale;
            visualScale.x = -Mathf.Abs(visualScale.x);
            wheelVisual.localScale = visualScale;
        }
        
        // Set initial position
        wheelWorldPosition = transform.TransformPoint(wheelPosition);
    }

    #endregion

    #region Physics Update Methods

    private void ApplyAerodynamicDrag()
    {
        if (parentRigidbody == null) return;
        
        Vector3 dragForce = -dragCoefficient * 
                            parentRigidbody.velocity.sqrMagnitude * 
                            parentRigidbody.velocity.normalized * 
                            Time.fixedDeltaTime;
        
        parentRigidbody.AddForce(dragForce);
    }

    private void UpdateWheel()
    {
        if (wheelObject == null || parentRigidbody == null) return;

        // Update wheel physics
        UpdateWheelForces();
        
        // Update wheel position
        UpdateWheelPosition();
        
        // Update wheel rotation
        UpdateWheelRotation();
    }

    private void UpdateWheelForces()
    {
        // Calculate torque from input (only if not free rolling)
        torque = freeRoll ? 0 : Mathf.Clamp(input.y, -1, 1) * maxTorque;

        // Calculate force from torque
        if (vehicleController != null)
        {
            hitPointForce = torque / wheelSize * 
                           (vehicleController.sectionCount / vehicleController.massInKg);
        }
        else
        {
            hitPointForce = torque / wheelSize;
        }

        // Apply friction
        torque -= frictionCoefficient * 
                 Mathf.Sign(parentRigidbody.GetPointVelocity(wheelWorldPosition).z) * 
                 Time.fixedDeltaTime;

        // Update world position
        wheelWorldPosition = transform.TransformPoint(wheelPosition);

        // Calculate slip direction
        CalculateSlipDirection();
    }

    private void CalculateSlipDirection()
    {
        // Get local velocity
        localVelocity = transform.InverseTransformDirection(
            parentRigidbody.GetPointVelocity(wheelWorldPosition));

        // Check forward direction
        isMovingForward = localVelocity.z > 0;

        // Calculate slip direction
        if (freeRoll)
        {
            localSlipDirection = Vector3.zero;
        }
        else
        {
            float turnAngleRad = turnAngle * input.x * Mathf.Deg2Rad;
            
            localSlipDirection = wheelGrip * new Vector3(
                -localVelocity.x * Mathf.Cos(turnAngleRad) + 
                localVelocity.z * Mathf.Sin(turnAngleRad),
                0,
                (-localVelocity.z * Mathf.Cos(turnAngleRad) - 
                localVelocity.x * Mathf.Sin(turnAngleRad)) * 
                rollingResistance + hitPointForce / wheelGrip
            );
        }

        // Convert to world space and clamp magnitude
        worldSlipDirection = Vector3.ClampMagnitude(
            transform.rotation * localSlipDirection, 
            maxGrip);
    }

    private void UpdateWheelPosition()
    {
        // Raycast to check ground contact
        RaycastHit hit;
        if (Physics.Raycast(wheelWorldPosition, -transform.up, out hit, wheelSize * 2))
        {
            // Calculate suspension force
            suspensionForceDirection = hit.normal * 
                                     Mathf.Clamp(
                                         wheelSize * 2 - hit.distance + 
                                         Mathf.Clamp(lastSuspensionLength - hit.distance, -1, 1) * 
                                         dampingAmount, 
                                         0, Mathf.Infinity) * 
                                     suspensionForce * 
                                     Time.fixedDeltaTime * 50;
            
            // Clamp suspension force
            suspensionForceDirection = Vector3.ClampMagnitude(
                suspensionForceDirection, 
                suspensionForceClamp);

            // Apply forces to the parent Rigidbody
            parentRigidbody.AddForceAtPosition(
                suspensionForceDirection + worldSlipDirection, 
                wheelWorldPosition);

            // Position wheel at contact point
            wheelObject.position = hit.point + transform.up * wheelSize;
            lastSuspensionLength = hit.distance;
        }
        else
        {
            // Position wheel at raycast endpoint when no contact
            wheelObject.position = wheelWorldPosition - transform.up * wheelSize;
            suspensionForceDirection = Vector3.zero;
        }
    }

    private void UpdateWheelRotation()
    {
        // Spin the wheel based on velocity
        Vector3 forwardInWheelSpace = wheelObject.InverseTransformDirection(
            parentRigidbody.GetPointVelocity(wheelWorldPosition)
        );

        float wheelRotationSpeed = forwardInWheelSpace.z * 360 / wheelCircumference;
        wheelVisual.Rotate(Vector3.right, wheelRotationSpeed * Time.fixedDeltaTime, Space.Self);

        // Rotate wheel for steering or alignment with velocity
        if (!freeRoll)
        {
            // Steering wheel
            Quaternion targetRotation = Quaternion.Euler(0, input.x * turnAngle, 0);
            wheelObject.localRotation = Quaternion.Lerp(
                wheelObject.localRotation, 
                targetRotation, 
                Time.fixedDeltaTime * 10);
        }
        else if (parentRigidbody.velocity.magnitude > 0.04f && 
                suspensionForceDirection != Vector3.zero)
        {
            // Free wheel aligns with velocity
            Quaternion targetRotation = Quaternion.LookRotation(
                parentRigidbody.GetPointVelocity(wheelWorldPosition), 
                transform.up);
                
            wheelObject.rotation = Quaternion.Lerp(
                wheelObject.rotation, 
                targetRotation, 
                Time.fixedDeltaTime * 10);
        }
    }

    #endregion

    #region Debug Methods

    private void OnDrawGizmos()
    {
        // Draw wheel position
        Gizmos.color = Color.green;
        Vector3 gizmoPosition = Application.isPlaying ? wheelWorldPosition : transform.TransformPoint(wheelPosition);
        Gizmos.DrawSphere(gizmoPosition, 0.1f);

        if (!Application.isPlaying) return;

        // Draw suspension force
        if (suspensionForceDirection != Vector3.zero)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawLine(wheelWorldPosition, 
                            wheelWorldPosition + suspensionForceDirection);
        }

        // Draw slip direction
        if (worldSlipDirection != Vector3.zero)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawLine(wheelWorldPosition, 
                            wheelWorldPosition + worldSlipDirection);
        }
    }

    #endregion
}