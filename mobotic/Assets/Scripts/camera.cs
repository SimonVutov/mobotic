using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public Vector3[] angles;
    public Vector3[] lookAts;
    int curAngle = 0;
    private Gamepad gamepad;


    public bool ignoreRaycast = true;
    public Transform target; // The target GameObject to follow
    public float zoomSpeed = 10.0f; // Speed of zooming in/out
    public float minFOV = 20.0f; // Minimum field of view
    public float maxFOV = 90.0f; // Maximum field of view

    public float rotationSpeedX = 250.0f; // Speed of rotation around X-axis
    public float rotationSpeedY = 120.0f; // Speed of rotation around Y-axis
    public float minYAngle = -20f; // Minimum Y angle
    public float maxYAngle = 80f;  // Maximum Y angle

    private float currentX = 0.0f; // Current X rotation angle
    private float currentY = 20.0f; // Current Y rotation angle

    private Camera cam; // Reference to the Camera component

    void Awake()
    {
        // Initialize angles and lookAts arrays if they're not set
        if (angles == null || angles.Length == 0)
        {
            angles = new Vector3[1];
            angles[0] = new Vector3(0, 2, -5); // Default position behind and above the target
            Debug.LogWarning("Camera angles array not set. Using default values.");
        }

        if (lookAts == null || lookAts.Length == 0)
        {
            lookAts = new Vector3[1];
            lookAts[0] = new Vector3(0, 0, 2); // Default look at point in front of the target
            Debug.LogWarning("Camera lookAts array not set. Using default values.");
        }
    }

    void Start()
    {
        gamepad = Gamepad.current;
        //adjust camera near clipping
        Camera.main.nearClipPlane = 0.01f;

        cam = GetComponent<Camera>();
        if (cam == null)
        {
            Debug.LogError("CameraController requires a Camera component on the same GameObject.");
            enabled = false;
            return;
        }

        if (target == null)
        {
            Debug.LogWarning("Target not set for CameraController. Camera will not follow any object until a target is set.");
            // Don't disable the controller, just wait for a target to be set
            // enabled = false;
            // return;
        }
        else
        {
            // Hide and lock the cursor only when we have a target
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }

    void Update()
    {
        // Ensure we have a valid camera reference
        if (cam == null) return;

        // Adjust camera FOV with scroll wheel
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scrollInput) > 0.01f)
        {
            cam.fieldOfView -= scrollInput * zoomSpeed;
            cam.fieldOfView = Mathf.Clamp(cam.fieldOfView, minFOV, maxFOV);
        }
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.C) || gamepad.dpad.left.wasPressedThisFrame)
        {
            curAngle++;
            if (curAngle >= angles.Length)
            {
                curAngle = 0;
            }
        }

        // Only call camera modes if we have a target
        if (target != null)
        {
            StrictCamMode();
            // FollowCamMode();
        }
    }

    void StrictCamMode()
    {
        if (target == null) return;

        // Check if angles and lookAts arrays are properly initialized
        if (angles == null || lookAts == null || angles.Length == 0 || lookAts.Length == 0 || curAngle >= angles.Length || curAngle >= lookAts.Length)
        {
            Debug.LogWarning("Camera angles or lookAts arrays not properly initialized. Cannot use StrictCamMode.");
            return;
        }

        // Calculate the desired position and rotation
        Vector3 desiredPos = angles[curAngle].x * target.right + angles[curAngle].y * target.up + angles[curAngle].z * target.forward;
        Vector3 lookAt = lookAts[curAngle].x * target.right + lookAts[curAngle].y * target.up + lookAts[curAngle].z * target.forward;

        // Update the camera's position and rotation
        Vector3 velocity = Vector3.zero;
        transform.position = target.position + desiredPos;
        transform.LookAt(target.position + lookAt);
    }

    void FollowCamMode()
    {
        if (target == null) return;

        // Mouse input for orbiting around the target
        currentX += Input.GetAxis("Mouse X") * rotationSpeedX * Time.deltaTime;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeedY * Time.deltaTime;
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

        // Calculate the desired position and rotation
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = target.position - rotation * Vector3.forward * 5.0f;

        // Raycast to detect obstacles between the camera and the target
        if (!ignoreRaycast)
        {
            RaycastHit hit;
            if (Physics.Linecast(target.position, desiredPosition, out hit))
            {
                // Check if any parent of the hit object has a Vehicle component
                Transform currentTransform = hit.collider.transform;
                bool isVehiclePart = false;

                while (currentTransform != null)
                {
                    if (currentTransform.GetComponent<wheel>() != null)
                    {
                        isVehiclePart = true;
                        break;
                    }
                    currentTransform = currentTransform.parent;
                }

                // Adjust camera position if the hit object is not part of a vehicle
                if (!isVehiclePart)
                {
                    desiredPosition = hit.point + hit.normal * 0.05f;
                }
            }
        }

        // Update the camera's position and rotation
        transform.position = desiredPosition;
        transform.LookAt(target.position);
    }

    void OnDisable()
    {
        // Show the cursor again when the script is disabled
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
        if (target != null)
        {
            enabled = true;
            // Reset camera position and rotation for new target
            currentX = 0.0f;
            currentY = 20.0f;

            // Force an immediate update to position camera correctly
            FollowCamMode();

            // Hide and lock the cursor when we have a target
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
        else
        {
            // If target is null, don't disable the camera controller
            // Just log a message and show the cursor
            Debug.Log("Camera target set to null, waiting for new target");

            // Show the cursor when we don't have a target
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }
    }
}