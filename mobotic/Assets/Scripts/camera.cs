using UnityEngine;

public class CameraController : MonoBehaviour
{
    public Transform target; // The target GameObject to follow
    public float distance = 5.0f; // Default distance from the target
    public float zoomSpeed = 2.0f; // Speed of zooming in/out
    public float minDistance = 2.0f; // Minimum camera distance
    public float maxDistance = 15.0f; // Maximum camera distance

    public float rotationSpeedX = 250.0f; // Speed of rotation around X-axis
    public float rotationSpeedY = 120.0f; // Speed of rotation around Y-axis
    public float minYAngle = -20f; // Minimum Y angle
    public float maxYAngle = 80f;  // Maximum Y angle

    private float currentX = 0.0f; // Current X rotation angle
    private float currentY = 20.0f; // Current Y rotation angle
    private bool isFreeCam = false; // Toggle between free cam and follow cam

    void Start()
    {
        if (target == null)
        {
            Debug.LogError("Target not set for CameraController. Please set a target GameObject in the Inspector.");
            enabled = false;
            return;
        }

        // Hide and lock the cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void Update()
    {
        // Toggle between free cam and follow cam on pressing 'C'
        if (Input.GetKeyDown(KeyCode.C))
        {
            isFreeCam = !isFreeCam;
            Cursor.lockState = isFreeCam ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = isFreeCam;
        }
    }

    void LateUpdate()
    {
        if (isFreeCam)
        {
            FreeCamMode();
        }
        else
        {
            FollowCamMode();
        }
    }

    void FreeCamMode()
    {
        // Free cam movement controls (Arrow keys for movement, right mouse button for looking around)
        float moveSpeed = 10.0f * Time.deltaTime;
        if (Input.GetKey(KeyCode.UpArrow)) transform.position += transform.forward * moveSpeed;
        if (Input.GetKey(KeyCode.DownArrow)) transform.position -= transform.forward * moveSpeed;
        if (Input.GetKey(KeyCode.LeftArrow)) transform.position -= transform.right * moveSpeed;
        if (Input.GetKey(KeyCode.RightArrow)) transform.position += transform.right * moveSpeed;

        // Look around with the mouse when the right mouse button is held
        if (Input.GetMouseButton(1))
        {
            currentX += Input.GetAxis("Mouse X") * rotationSpeedX * Time.deltaTime;
            currentY -= Input.GetAxis("Mouse Y") * rotationSpeedY * Time.deltaTime;
            currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

            transform.rotation = Quaternion.Euler(currentY, currentX, 0);
        }
    }

    void FollowCamMode()
    {
        if (target == null) return;

        // Mouse input for orbiting around the target
        currentX += Input.GetAxis("Mouse X") * rotationSpeedX * Time.deltaTime;
        currentY -= Input.GetAxis("Mouse Y") * rotationSpeedY * Time.deltaTime;
        currentY = Mathf.Clamp(currentY, minYAngle, maxYAngle);

        // Scroll wheel input for zooming
        float scrollInput = Input.GetAxis("Mouse ScrollWheel");
        distance -= scrollInput * zoomSpeed;
        distance = Mathf.Clamp(distance, minDistance, maxDistance);

        // Calculate the desired position and rotation
        Quaternion rotation = Quaternion.Euler(currentY, currentX, 0);
        Vector3 desiredPosition = target.position - rotation * Vector3.forward * distance;

        // Raycast to detect obstacles between the camera and the target
        RaycastHit hit;
        if (Physics.Linecast(target.position, desiredPosition, out hit))
        {
            // Check if the root parent's name starts with "Vehicle"
            Transform rootParent = hit.collider.transform.root;
            if (!rootParent.name.StartsWith("Vehicle"))
            {
                // Adjust the camera position to avoid clipping through objects
                desiredPosition = hit.point + hit.normal * 0.5f;
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
}