using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Piece
{
    public float inertiaTensorMultiplier = 100f;
    public GameObject forkPrefab;
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

[System.Serializable]
public class InPlacePiece
{
    public GameObject existingPiece; // Reference to an existing GameObject
    public Vector3 positionOffset; // Offset from the forklift's position
    public float maxHeight;
    [HideInInspector]
    public float targetHeight;
    [HideInInspector]
    public Vector3 oldForce;
}

public class forklift : MonoBehaviour
{
    public float moveSpeed = 2f;
    public List<Piece> pieces;
    public List<InPlacePiece> inPlacePieces; // New list for in-place pieces
    [HideInInspector]
    public List<GameObject> piecesObjects;
    private float input;

    private float springForce = 210f;
    private float dampingForce = 9f;
    private float clampForce = 210f;

    private void Start()
    {
        // Initialize regular pieces
        foreach (Piece piece in pieces)
        {
            piece.pieceObject = Instantiate(piece.forkPrefab, transform.position, transform.rotation);
            piecesObjects.Add(piece.pieceObject);

            piece.pieceObject.transform.localScale = piece.shape;

            piece.pieceObject.transform.position = transform.TransformPoint(piece.position);
            piece.pieceObject.transform.rotation = transform.rotation;

            // Remove rotation constraints, let physics handle it
            Rigidbody rb = piece.pieceObject.GetComponent<Rigidbody>();
            if (rb == null)
            {
                continue;
            }
            rb.inertiaTensor = rb.inertiaTensor * piece.inertiaTensorMultiplier;

            // make forklift a child of the vehicle so that it gets deleted when the vehicle is destroyed
            piece.pieceObject.transform.parent = transform;
        }

        // Initialize in-place pieces (just add to piecesObjects list for tracking)
        foreach (InPlacePiece inPlacePiece in inPlacePieces)
        {
            if (inPlacePiece.existingPiece != null)
            {
                piecesObjects.Add(inPlacePiece.existingPiece);
                // Calculate initial position offset if not set
                if (inPlacePiece.positionOffset == Vector3.zero)
                {
                    inPlacePiece.positionOffset = transform.InverseTransformPoint(inPlacePiece.existingPiece.transform.position);
                }
            }
        }
    }

    void Update()
    {
        input = Input.GetKey(KeyCode.E) ? 1 : Input.GetKey(KeyCode.Q) ? -1 : 0;
    }

    void FixedUpdate()
    {
        // Handle regular pieces
        foreach (Piece piece in pieces)
        {
            piece.targetHeight = piece.targetHeight + input * moveSpeed;
            piece.targetHeight = Mathf.Clamp(piece.targetHeight, 0, piece.maxHeight);
            // Calculate target position in world space, using the piece's position offset
            Vector3 targetWorldPosition = transform.TransformPoint(new Vector3(
                piece.position.x,
                piece.position.y + piece.targetHeight,
                piece.position.z
            ));
            Vector3 currentWorldPosition = piece.pieceObject.transform.position;

            if (piece.pieceObject.GetComponent<Rigidbody>() == null)
            {
                // piece is just a visual object, move it directly
                piece.pieceObject.transform.position = targetWorldPosition;
                piece.pieceObject.transform.rotation = transform.rotation;

                continue;
            }

            // Calculate the force with spring-damper approach
            Vector3 positionError = targetWorldPosition - currentWorldPosition;
            Vector3 velocity = piece.pieceObject.GetComponent<Rigidbody>().velocity;

            Vector3 totalForce = (positionError * springForce) - (velocity * dampingForce);
            totalForce = Vector3.ClampMagnitude(totalForce, clampForce); // Prevent extreme forces

            // Apply equal and opposite forces
            piece.pieceObject.GetComponent<Rigidbody>().AddForce(totalForce, ForceMode.Force);
            GetComponent<Rigidbody>().AddForceAtPosition(-totalForce, piece.pieceObject.transform.position, ForceMode.Force);

            // Calculate and apply gentler torque to maintain orientation
            Quaternion currentRotation = piece.pieceObject.transform.rotation;
            Quaternion targetRotation = transform.rotation;

            // Calculate rotation difference using FromToRotation
            Quaternion rotationDiff = targetRotation * Quaternion.Inverse(currentRotation);
            Vector3 torqueDirection = new Vector3(
                Mathf.DeltaAngle(0, rotationDiff.eulerAngles.x),
                Mathf.DeltaAngle(0, rotationDiff.eulerAngles.y),
                Mathf.DeltaAngle(0, rotationDiff.eulerAngles.z)
            ) * Mathf.Deg2Rad; // Convert to radians

            float torqueMagnitude = torqueDirection.magnitude * 10000f;
            Vector3 torque = Vector3.ClampMagnitude(torqueDirection * torqueMagnitude, 10000f);

            // Add damping based on angular velocity
            Vector3 angularVel = piece.pieceObject.GetComponent<Rigidbody>().angularVelocity;
            Vector3 dampingTorque = angularVel * 20f; // Adjust damping factor as needed
            Vector3 totalTorque = torque - dampingTorque;

            totalTorque = Vector3.ClampMagnitude(totalTorque, 80f); // Prevent extreme torques

            // Apply equal and opposite torques
            piece.pieceObject.GetComponent<Rigidbody>().AddTorque(totalTorque, ForceMode.Force);
            //GetComponent<Rigidbody>().AddTorque(-totalTorque, ForceMode.Force);
        }

        // Handle in-place pieces
        foreach (InPlacePiece inPlacePiece in inPlacePieces)
        {
            if (inPlacePiece.existingPiece == null) continue;

            inPlacePiece.targetHeight = inPlacePiece.targetHeight + input * moveSpeed;
            inPlacePiece.targetHeight = Mathf.Clamp(inPlacePiece.targetHeight, 0, inPlacePiece.maxHeight);

            // Calculate target position in world space, using the piece's position offset
            Vector3 targetWorldPosition = transform.TransformPoint(new Vector3(
                inPlacePiece.positionOffset.x,
                inPlacePiece.positionOffset.y + inPlacePiece.targetHeight,
                inPlacePiece.positionOffset.z
            ));
            Vector3 currentWorldPosition = inPlacePiece.existingPiece.transform.position;

            Rigidbody rb = inPlacePiece.existingPiece.GetComponent<Rigidbody>();
            if (rb == null)
            {
                // piece is just a visual object, move it directly
                inPlacePiece.existingPiece.transform.position = targetWorldPosition;
                inPlacePiece.existingPiece.transform.rotation = transform.rotation;
                continue;
            }

            // Calculate the force with spring-damper approach
            Vector3 positionError = targetWorldPosition - currentWorldPosition;
            Vector3 velocity = rb.velocity;

            Vector3 totalForce = (positionError * springForce) - (velocity * dampingForce);
            totalForce = Vector3.ClampMagnitude(totalForce, clampForce); // Prevent extreme forces

            // Apply equal and opposite forces
            rb.AddForce(totalForce, ForceMode.Force);
            GetComponent<Rigidbody>().AddForceAtPosition(-totalForce, inPlacePiece.existingPiece.transform.position, ForceMode.Force);

            // Calculate and apply gentler torque to maintain orientation
            Quaternion currentRotation = inPlacePiece.existingPiece.transform.rotation;
            Quaternion targetRotation = transform.rotation;

            // Calculate rotation difference using FromToRotation
            Quaternion rotationDiff = targetRotation * Quaternion.Inverse(currentRotation);
            Vector3 torqueDirection = new Vector3(
                Mathf.DeltaAngle(0, rotationDiff.eulerAngles.x),
                Mathf.DeltaAngle(0, rotationDiff.eulerAngles.y),
                Mathf.DeltaAngle(0, rotationDiff.eulerAngles.z)
            ) * Mathf.Deg2Rad; // Convert to radians

            float torqueMagnitude = torqueDirection.magnitude * 10000f;
            Vector3 torque = Vector3.ClampMagnitude(torqueDirection * torqueMagnitude, 10000f);

            // Add damping based on angular velocity
            Vector3 angularVel = rb.angularVelocity;
            Vector3 dampingTorque = angularVel * 20f; // Adjust damping factor as needed
            Vector3 totalTorque = torque - dampingTorque;

            totalTorque = Vector3.ClampMagnitude(totalTorque, 80f); // Prevent extreme torques

            // Apply equal and opposite torques
            rb.AddTorque(totalTorque, ForceMode.Force);
        }
    }

    public void DestroyForklift()
    {
        foreach (Piece piece in pieces)
        {
            if (piece.pieceObject != null)
            {
                Destroy(piece.pieceObject);
            }
        }
        // We don't destroy in-place pieces as they're external references
        pieces.Clear();
        piecesObjects.Clear();
    }

    private void OnDestroy()
    {
        // Destroy all pieces when the vehicle is destroyed
        DestroyAllPieces();
    }

    private void DestroyAllPieces()
    {
        // Only destroy the pieces we created, not the in-place pieces
        foreach (Piece piece in pieces)
        {
            if (piece.pieceObject != null)
            {
                Destroy(piece.pieceObject);
            }
        }

        // Store in-place pieces before clearing the list
        List<GameObject> inPlacePieceObjects = new List<GameObject>();
        foreach (InPlacePiece inPlacePiece in inPlacePieces)
        {
            if (inPlacePiece.existingPiece != null)
            {
                inPlacePieceObjects.Add(inPlacePiece.existingPiece);
            }
        }

        // Clear the pieces list
        pieces.Clear();

        // Clear and rebuild the piecesObjects list with only in-place pieces
        piecesObjects.Clear();
        foreach (GameObject obj in inPlacePieceObjects)
        {
            piecesObjects.Add(obj);
        }
    }
}