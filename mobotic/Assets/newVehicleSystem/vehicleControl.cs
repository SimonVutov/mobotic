using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class WheelReference
{
    public Transform wheel;
    public KeyCode forwardsKey = KeyCode.W;
    public KeyCode backwardsKey = KeyCode.S;
    public KeyCode leftKey = KeyCode.A;
    public KeyCode rightKey = KeyCode.D;

    public float turnAngle = 45;
    public float torque = 200;
    public Rigidbody parentRigidbody;
    
}


public class vehicleControl : MonoBehaviour
{
    public WheelReference[] wheels;

    public int sectionCount = 1;
    public float massInKg = 100.0f;

    void Awake()
    {
        foreach (WheelReference child in wheels)
        {
            child.wheel.GetComponent<WheelComponent>().vehicleController = this;
            if (child.parentRigidbody == null)
            {
                child.parentRigidbody = GetComponent<Rigidbody>();
            }

            child.wheel.GetComponent<WheelComponent>().parentRigidbody = GetComponent<Rigidbody>();

        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
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
