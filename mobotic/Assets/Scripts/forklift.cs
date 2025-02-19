using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forklift : MonoBehaviour
{
    private float moveSpeed = 0.02f;
    private float heightSmoothTime = 0.1f;
    public List<Piece> pieces;
    [HideInInspector]
    public List<GameObject> piecesObjects;
    public GameObject forkPrefab;
    private float input;

    private float springForce = 20f;
    private float dampingForce = 0.8f;
    private float clampForce = 20f;

    private void Start()
    {
        foreach (Piece piece in pieces)
        {
            piece.pieceObject = Instantiate(forkPrefab, piece.position, Quaternion.identity);
            piecesObjects.Add(piece.pieceObject);

            piece.pieceObject.transform.localScale = piece.shape;

            piece.pieceObject.transform.position = transform.TransformPoint(piece.position);
            piece.pieceObject.transform.rotation = transform.rotation;

            // Only freeze rotation on X and Z axes (local), allow Y rotation to follow the vehicle
            Rigidbody rb = piece.pieceObject.GetComponent<Rigidbody>();
            rb.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        }
    }
    void Update()
    {
        input = Input.GetKey(KeyCode.E) ? 1 : Input.GetKey(KeyCode.Q) ? -1 : 0;
    }

    void FixedUpdate()
    {
        foreach (Piece piece in pieces)
        {
            piece.targetHeight += input * moveSpeed;
            piece.targetHeight = Mathf.Clamp(piece.targetHeight, 0, piece.maxHeight);
            // Calculate target position in world space, using the piece's position offset
            Vector3 targetWorldPosition = transform.TransformPoint(new Vector3(
                piece.position.x,
                piece.position.y + piece.targetHeight,
                piece.position.z
            ));
            Vector3 currentWorldPosition = piece.pieceObject.transform.position;

            // Calculate the force with spring-damper approach
            Vector3 positionError = targetWorldPosition - currentWorldPosition;
            Vector3 velocity = piece.pieceObject.GetComponent<Rigidbody>().velocity;

            Vector3 totalForce = (positionError * springForce) - (velocity * dampingForce);
            totalForce = Vector3.ClampMagnitude(totalForce, clampForce); // Prevent extreme forces

            // Apply equal and opposite forces
            piece.pieceObject.GetComponent<Rigidbody>().AddForce(totalForce, ForceMode.Force);
            GetComponent<Rigidbody>().AddForceAtPosition(-totalForce, piece.pieceObject.transform.position, ForceMode.Force);

            // Directly set the rotation to match the vehicle
            piece.pieceObject.transform.rotation = transform.rotation;

            // Remove rigidbody rotation
            piece.pieceObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
        }
    }
}

[System.Serializable]
public class Piece
{
    public Vector3 position;
    public Vector3 shape;
    public float maxHeight;
    [HideInInspector]
    public GameObject pieceObject;
    [HideInInspector]
    public float targetHeight;
    [HideInInspector]
    public Vector3 oldForce;
}