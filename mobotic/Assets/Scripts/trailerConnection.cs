using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class trailerConnection : MonoBehaviour
{
    public GameObject trailer;
    public GameObject connectionPointOnTrailer;
    public GameObject vehicle;
    public GameObject connectionPointOnVehicle;
    float maxDistance = 0.8f;
    float lastDistance = 1.0f;
    float strength = 40.0f;
    float damp = 400.0f;
    Rigidbody VehicleRb;
    Rigidbody TrailerRb;
    // Start is called before the first frame update
    void Start()
    {
        VehicleRb = vehicle.GetComponent<Rigidbody>();
        TrailerRb = trailer.GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float dist = Vector3.Distance(connectionPointOnTrailer.transform.position, connectionPointOnVehicle.transform.position);
        float pushPull = (maxDistance - dist) * strength + (lastDistance - dist) * damp;        
        Vector3 force = pushPull * (connectionPointOnTrailer.transform.position - connectionPointOnVehicle.transform.position).normalized;
        VehicleRb.AddForceAtPosition(-force, connectionPointOnVehicle.transform.position);
        TrailerRb.AddForceAtPosition(force, connectionPointOnTrailer.transform.position);
        lastDistance = dist;

        //render a line renderer
        LineRenderer lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, connectionPointOnTrailer.transform.position);
        lineRenderer.SetPosition(1, connectionPointOnVehicle.transform.position);
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawLine(connectionPointOnTrailer.transform.position, connectionPointOnVehicle.transform.position);
    }
}
