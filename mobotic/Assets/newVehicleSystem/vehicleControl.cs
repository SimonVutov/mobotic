// VEHICLECONTROL.CS

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class WheelReference
{
    // set defaults button, to setup this wheel

    public Transform wheel;
    public KeyCode forwardsKey = KeyCode.W;
    public KeyCode backwardsKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    public float turnAngle = 45;
    public float torque = 200;
}


public class vehicleControl : MonoBehaviour
{
    public Vector3 centerOfMass = new Vector3(0, 0, 0);
    public WheelReference[] wheels;

    public int sectionCount = 1;
    public float massInKg = 100.0f;

    void Awake()
    {
        foreach (WheelReference child in wheels)
        {
            child.wheel.GetComponent<WheelComponent>().parentRigidbody = GetComponent<Rigidbody>();
            child.wheel.GetComponent<WheelComponent>().vehicleController = this;
        }
    }

    void Start()
    {
        // add center of mass offset to current center of mass
        GetComponent<Rigidbody>().centerOfMass += centerOfMass;
    }

    void Update()
    {
        foreach (WheelReference child in wheels)
        {
            child.wheel.GetComponent<WheelComponent>().input = new Vector2(
                (Input.GetKey(child.rightKey) ? 1 : Input.GetKey(child.leftKey) ? -1 : 0) * child.turnAngle,
                (Input.GetKey(child.forwardsKey) ? 1 : Input.GetKey(child.backwardsKey) ? -1 : 0) * child.torque
            );
        }
    }
}
