using UnityEngine;

public class Hinge : MonoBehaviour
{
    public GameObject mainBody;
    public GameObject thisBody;
    public Vector3 rotationAxis;
    public float angularSpeed = 100f;

    private Vector3 localHingePosition;
    private Vector3 oldChangeInPosition = Vector3.zero;
    private float dampingFactor = 5f;
    private float bindingFactor = 50f;
    private Rigidbody mainRb;
    private Rigidbody thisRb;
    private float forceClamp = 100f;
    private float snapThreshold = 1.0f;

    private void Start()
    {
        localHingePosition = mainBody.transform.InverseTransformPoint(thisBody.transform.position);
        mainRb = mainBody.GetComponent<Rigidbody>();
        thisRb = thisBody.GetComponent<Rigidbody>();
        rotationAxis = rotationAxis.normalized;
    }

    private void FixedUpdate()
    {
        Vector3 hingeWorldPosition = mainBody.transform.TransformPoint(localHingePosition);
        Vector3 changeInPosition = thisBody.transform.position - hingeWorldPosition;

        // Calculate the world-space rotation axis based on the mainBody's current orientation
        Vector3 worldRotationAxis = mainBody.transform.TransformDirection(rotationAxis);

        // Snap back if too far from hinge position
        if (changeInPosition.magnitude > snapThreshold)
        {
            SnapBack();
            return;
        }

        // Apply damping forces to maintain hinge position
        Vector3 dampedForce = changeInPosition * bindingFactor + (changeInPosition - oldChangeInPosition) * dampingFactor;
        dampedForce = Vector3.ClampMagnitude(dampedForce, forceClamp);
        mainRb.AddForceAtPosition(dampedForce, hingeWorldPosition);
        thisRb.AddForceAtPosition(-dampedForce, thisBody.transform.position);

        oldChangeInPosition = changeInPosition;
    }

    private void SnapBack()
    {
        Debug.Log("SnapBack");
        thisBody.transform.position = mainBody.transform.TransformPoint(localHingePosition);
        thisRb.velocity = Vector3.zero;
        thisRb.angularVelocity = Vector3.zero;
        thisBody.transform.rotation = mainBody.transform.rotation;
    }
}