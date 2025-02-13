using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Forklift : MonoBehaviour
{
    [SerializeField]
    private float moveSpeed = 0.7f;
    [SerializeField]
    private float heightSmoothTime = 0.1f;
    public List<Piece> pieces;

    private float currentHeight = 0f;
    private float targetHeight = 0f;
    private float heightVelocity = 0f;
    private float pieceMass = 0.01f;
    private BoxCollider forkliftCollider;

    // Cache the cube mesh resource
    private Mesh cubeMesh;

    void Start()
    {
        // Create (or add) a single collider for the forklift
        forkliftCollider = gameObject.AddComponent<BoxCollider>();

        // Cache the built-in cube mesh (make sure the name matches your project’s asset)
        cubeMesh = Resources.GetBuiltinResource<Mesh>("Cube.fbx");

        // Create each piece’s GameObject
        foreach (var piece in pieces)
        {
            GameObject pieceObject = new GameObject($"Piece_{pieces.IndexOf(piece)}");
            MeshFilter meshFilter = pieceObject.AddComponent<MeshFilter>();
            MeshRenderer meshRenderer = pieceObject.AddComponent<MeshRenderer>();

            // Set up the visual using the cube mesh
            meshFilter.mesh = cubeMesh;
            pieceObject.transform.localPosition = piece.position;
            pieceObject.transform.localScale = piece.shape;
            pieceObject.transform.parent = this.transform;

            // Add a Rigidbody (set to kinematic so it doesn’t interfere with manual movement)
            Rigidbody rb = pieceObject.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.mass = pieceMass;

            piece.pieceObject = pieceObject;
        }

        // Set the initial compound collider size
        UpdateForkliftCollider();
    }

    void Update()
    {
        HandleMovement();
        UpdateForkliftCollider();
    }

    private void HandleMovement()
    {
        // Adjust target height based on input
        if (Input.GetKey(KeyCode.E))
        {
            targetHeight += moveSpeed * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Q))
        {
            targetHeight -= moveSpeed * Time.deltaTime;
        }

        // Determine the overall max allowed height (so none of the pieces exceed their limit)
        float overallMaxHeight = float.MaxValue;
        foreach (var piece in pieces)
        {
            overallMaxHeight = Mathf.Min(overallMaxHeight, piece.maxHeight);
        }
        targetHeight = Mathf.Clamp(targetHeight, 0, overallMaxHeight);

        // Smoothly adjust the current height toward the target height
        currentHeight = Mathf.SmoothDamp(currentHeight, targetHeight, ref heightVelocity, heightSmoothTime);

        // Update each piece’s position (based on its original position plus the common height offset)
        foreach (var piece in pieces)
        {
            if (piece.pieceObject != null)
            {
                Vector3 basePos = piece.position;
                piece.pieceObject.transform.localPosition = new Vector3(
                    basePos.x,
                    basePos.y + currentHeight,
                    basePos.z
                );
            }
        }
    }

    private void UpdateForkliftCollider()
    {
        if (pieces.Count == 0)
            return;

        // Start with the bounds of the first piece using its renderer's bounds
        Bounds bounds = new Bounds(pieces[0].pieceObject.transform.position, Vector3.zero);
        bounds.Encapsulate(GetPieceWorldBounds(pieces[0]));

        // Expand bounds to include all pieces
        foreach (var piece in pieces)
        {
            if (piece.pieceObject != null)
            {
                Bounds pieceBounds = GetPieceWorldBounds(piece);
                bounds.Encapsulate(pieceBounds);
            }
        }

        // Set the BoxCollider's center and size (convert center to local space)
        forkliftCollider.center = transform.InverseTransformPoint(bounds.center);
        forkliftCollider.size = bounds.size;
    }

    // Helper: Get the world-space bounds of a piece (prefers renderer bounds)
    private Bounds GetPieceWorldBounds(Piece piece)
    {
        MeshRenderer renderer = piece.pieceObject.GetComponent<MeshRenderer>();
        if (renderer != null)
        {
            return renderer.bounds;
        }
        else
        {
            // Fallback: assume unit cube scaled by piece.shape
            return new Bounds(piece.pieceObject.transform.position, piece.shape);
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
}