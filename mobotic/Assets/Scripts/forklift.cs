using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forklift : MonoBehaviour
{
    public float moveSpeed = 1f;
    public float heightSmoothTime = 0.1f;
    public List<Piece> pieces;

    private float currentHeight = 0f;
    private float targetHeight = 0f;
    private float heightVelocity = 0f;
    private BoxCollider forkliftCollider;

    void Start()
    {
        // Create a single collider for the forklift
        forkliftCollider = gameObject.AddComponent<BoxCollider>();

        foreach (var piece in pieces)
        {
            // Create visual representation for each piece
            GameObject pieceObject = new GameObject($"Piece_{pieces.IndexOf(piece)}");
            MeshFilter meshFilter = pieceObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = pieceObject.AddComponent<MeshRenderer>();

            // Create a cube mesh
            meshFilter.mesh = CreateCubeMesh();
            pieceObject.transform.localPosition = piece.position;
            pieceObject.transform.localScale = piece.shape;
            pieceObject.transform.parent = this.transform;

            // Add rigidbody but no individual collider
            Rigidbody rb = pieceObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;

            piece.pieceObject = pieceObject;
        }

        // Calculate and set the compound collider size
        UpdateForkliftCollider();
    }

    void Update()
    {
        HandleMovement();
        UpdateForkliftCollider();
    }

    private void HandleMovement()
    {
        if (Input.GetKey(KeyCode.E))
        {
            targetHeight += moveSpeed * Time.deltaTime;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            targetHeight -= moveSpeed * Time.deltaTime;
        }

        foreach (var piece in pieces)
        {
            if (piece.pieceObject != null)
            {
                float maxAllowedHeight = piece.maxHeight;
                targetHeight = Mathf.Clamp(targetHeight, 0, maxAllowedHeight);
            }
        }

        currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref heightVelocity, heightSmoothTime);

        foreach (var piece in pieces)
        {
            if (piece.pieceObject != null)
            {
                piece.pieceObject.transform.localPosition = new Vector3(
                    piece.position.x,
                    piece.position.y + currentHeight,
                    piece.position.z
                );
            }
        }
    }

    private void UpdateForkliftCollider()
    {
        if (pieces.Count == 0) return;

        // Calculate bounds that encompass all pieces
        Bounds bounds = new Bounds(pieces[0].pieceObject.transform.position, pieces[0].shape);

        foreach (var piece in pieces)
        {
            if (piece.pieceObject != null)
            {
                // Create bounds for this piece
                Vector3 piecePosition = piece.pieceObject.transform.position;
                Vector3 pieceSize = piece.shape;
                Bounds pieceBounds = new Bounds(piecePosition, pieceSize);

                // Expand the total bounds to include this piece
                bounds.Encapsulate(pieceBounds);
            }
        }

        // Update the forklift's collider to match the calculated bounds
        forkliftCollider.center = bounds.center - transform.position; // Convert to local space
        forkliftCollider.size = bounds.size;
    }

    private Mesh CreateCubeMesh()
    {
        return Resources.GetBuiltinResource<Mesh>("Cube.fbx");
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
}