using UnityEngine;


public class SpawnVehicle : MonoBehaviour
{
    public GameObject[] vehiclePrefabs; // Assign 4 vehicle prefabs in the Inspector.
    public GameObject torqueDisplay;
    public GameObject camera;

    private void Awake()
    {
        int chosenIndex = Mathf.Clamp(VehicleSelection.selectedVehicleIndex, 0, vehiclePrefabs.Length - 1);

        // For example, spawn it at some starting position.
        Vector3 spawnPos = new Vector3(0, 0, 0);
        Quaternion spawnRot = Quaternion.identity;

        GameObject vehicle = Instantiate(vehiclePrefabs[chosenIndex], spawnPos, spawnRot);

        // set camera controller target
        camera.GetComponent<CameraController>().target = vehicle.transform;
        // set torque display root vehicle
        torqueDisplay.GetComponent<TorqueDisplay>().rootVehicle = vehicle;
    }
}