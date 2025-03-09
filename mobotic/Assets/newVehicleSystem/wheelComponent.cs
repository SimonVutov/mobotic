using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WheelComponent : MonoBehaviour
{
    [Header("Wheel Visual")]
    public Transform wheelVisual;  // Reference to existing wheel visual component
    private Transform wheelRim;    // Will be set to the first child of wheelVisual
    public bool hideWheelVisual = false;
    
    [Header("Wheel Configuration")]
    public bool freeRoll = false;  // If true, wheel will free roll; if false, wheel is powered
    [Range(0.1f, 1f)] public float wheelRadius = 0.3f;
    public bool flipX = false;
    
    [Header("Suspension")]
    [Range(0.01f, 2f)] public float suspensionLength = 0.1f;
    
    [Header("Physics Properties")]
    [Range(0.1f, 5f)] public float frictionCoefficient = 1f;
    [Range(10f, 1000f)] public float suspensionForce = 90f;
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
    private int rollingDirectionMultiplier = 1;
    private float lerpedInputX = 0f;

    private void OnValidate()
    {
        // Set default values if not already set
        if (wheelRadius <= 0) wheelRadius = 0.3f;
        if (suspensionForce <= 0) suspensionForce = 270.0f;
        if (maxGrip <= 0) maxGrip = 400.0f;
        if (frictionCoefficient <= 0) frictionCoefficient = 1f;
        if (suspensionLength <= 0) suspensionLength = 0.1f;
    }

    private void Start()
    {
        InitializeComponents();
        InitializeWheel();
        if (hideWheelVisual) wheelVisual.gameObject.SetActive(false);
    }

    private void FixedUpdate()
    {
        ApplyAerodynamicDrag();
        UpdateWheel();
        lerpedInputX = Mathf.Lerp(lerpedInputX, input.x, Time.fixedDeltaTime * 5);
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

        // Get the first child of wheelVisual (the rim/tire that should rotate)
        if (wheelVisual.childCount > 0)
        {
            wheelRim = wheelVisual.GetChild(0);
        }
        else
        {
            Debug.LogError("Wheel visual has no children. Please add a child object to represent the rotating rim/tire.");
            return;
        }
        
        // Initialize wheel properties
        wheelCircumference = 2 * Mathf.PI * wheelRadius;
        
        // Handle wheel visual flipping and set rolling direction multiplier
        if (flipX)
        {
            Vector3 visualScale = wheelVisual.localScale;
            visualScale.x = -Mathf.Abs(visualScale.x);
            wheelVisual.localScale = visualScale;
            rollingDirectionMultiplier = -1; // Reverse the rolling direction
        }
        else
        {
            rollingDirectionMultiplier = 1;
        }
        
        // Set initial position (now using transform position as base)
        wheelWorldPosition = transform.position;
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
        if (wheelVisual == null || parentRigidbody == null) return;

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
        torque = freeRoll ? 0 : input.y;

        // Calculate force from torque
        if (vehicleController != null)
        {
            hitPointForce = torque / wheelRadius * 
                           (vehicleController.sectionCount / vehicleController.massInKg);
        }
        else
        {
            hitPointForce = torque / wheelRadius;
        }

        // Apply friction
        torque -= frictionCoefficient * 
                 Mathf.Sign(parentRigidbody.GetPointVelocity(wheelWorldPosition).z) * 
                 Time.fixedDeltaTime;

        // Update world position (now using transform position)
        wheelWorldPosition = transform.position;

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
            // Get the current steering rotation in wheel space
            Quaternion steeringRotation = Quaternion.Euler(0, lerpedInputX, 0);
            
            // Transform the local velocity to wheel-aligned space
            Vector3 wheelAlignedVelocity = steeringRotation * localVelocity;
            
            // Calculate lateral grip (sideways resistance)
            float lateralGrip = -wheelAlignedVelocity.x * wheelGrip;
            
            // Calculate longitudinal grip (forward/backward force including drive torque)
            float longitudinalGrip = -wheelAlignedVelocity.z * rollingResistance * wheelGrip + hitPointForce;
            
            // Combine forces in wheel-aligned space
            Vector3 wheelAlignedForces = new Vector3(lateralGrip, 0, longitudinalGrip);
            
            // Transform back to vehicle space
            localSlipDirection = Quaternion.Inverse(steeringRotation) * wheelAlignedForces;
        }

        // Convert to world space and clamp magnitude
        worldSlipDirection = Vector3.ClampMagnitude(
            transform.TransformDirection(localSlipDirection), 
            maxGrip);
    }

    private void UpdateWheelPosition()
    {
        // Raycast to check ground contact
        RaycastHit hit;
        if (Physics.Raycast(wheelWorldPosition, -transform.up, out hit, suspensionLength + wheelRadius))
        {
            // Calculate suspension force
            suspensionForceDirection = hit.normal * 
                                     Mathf.Clamp(
                                         suspensionLength + wheelRadius - hit.distance + 
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
            wheelVisual.position = hit.point + transform.up * wheelRadius;
            lastSuspensionLength = hit.distance;
        }
        else
        {
            // Position wheel at maximum extension when no contact
            wheelVisual.position = wheelWorldPosition - transform.up * suspensionLength;
            suspensionForceDirection = Vector3.zero;
        }
    }

    private void UpdateWheelRotation()
    {
        // STEP 1: Update the wheelVisual rotation (steering - Y axis only)
        if (!freeRoll)
        {
            // Only rotate around Y axis for steering
            Quaternion targetYRotation = Quaternion.Euler(0, -lerpedInputX, 0);
            wheelVisual.localRotation = Quaternion.Lerp(
                wheelVisual.localRotation,
                targetYRotation,
                Time.fixedDeltaTime * 15);
        }
        else if (parentRigidbody.velocity.magnitude > 0.04f && 
                suspensionForceDirection != Vector3.zero)
        {
            // For free rolling wheels, align with velocity direction but only on Y axis
            Vector3 velocity = parentRigidbody.GetPointVelocity(wheelWorldPosition);
            if (velocity.magnitude > 0.1f)
            {
                // Project velocity onto XZ plane
                Vector3 velocityXZ = new Vector3(velocity.x, 0, velocity.z).normalized;
                
                // Convert to local direction relative to parent
                Vector3 localDir = transform.InverseTransformDirection(velocityXZ);
                
                // Calculate target Y rotation only
                float targetYAngle = Mathf.Atan2(localDir.x, localDir.z) * Mathf.Rad2Deg;
                Quaternion targetYRotation = Quaternion.Euler(0, targetYAngle, 0);
                
                wheelVisual.localRotation = Quaternion.Lerp(
                    wheelVisual.localRotation,
                    targetYRotation,
                    Time.fixedDeltaTime * 15);
            }
        }

        // STEP 2: Update the wheelRim rotation (rolling motion)
        if (wheelRim != null)
        {
            // Calculate rotation speed based on forward velocity in local wheel space
            Vector3 wheelLocalVelocity = wheelVisual.InverseTransformDirection(
                parentRigidbody.GetPointVelocity(wheelWorldPosition));
                
            float wheelRotationSpeed = wheelLocalVelocity.z * 360 / wheelCircumference;
            
            // Apply the direction multiplier based on flipX setting
            wheelRotationSpeed *= rollingDirectionMultiplier;
            
            // Rotate the rim (first child) around its X axis
            wheelRim.Rotate(Vector3.right, wheelRotationSpeed * Time.fixedDeltaTime, Space.Self);
        }
    }

    #endregion

    #region Debug Methods

    private void OnDrawGizmos()
    {
        // Draw wheel position
        Gizmos.color = Color.green;
        Vector3 gizmoPosition = Application.isPlaying ? wheelWorldPosition : transform.position;
        Gizmos.DrawSphere(gizmoPosition, 0.1f);
        
        // Draw suspension range
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(gizmoPosition, gizmoPosition - transform.up * (suspensionLength + wheelRadius));

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