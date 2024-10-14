using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class poweredWheel
{
    // Attributes for the powered wheel
    public Vector3 localPosition;
    public float power = 100.0f;
    public float size = 1.0f;
    public bool isControlEnabled = true;
    public float suspensionForce = 1.0f;
    public float grip = 1.0f;
    public bool inContact = false;
    public bool turnable = false;
    public float turnAngle = 45.0f;
    public float maxGrip = 1.0f;
    [HideInInspector]
    public float lastSuspensionLength = 0.0f;
    //[HideInInspector]
    public float rotationsPerSecond = 0.0f;
    [HideInInspector]
    public float torque = 0.0f;
    [HideInInspector]
    public float brakeTorque = 0.0f;
    [HideInInspector]
    public float throttle = 0.0f;
    [HideInInspector]
    public Vector3 localSlipDirection;
    [HideInInspector]
    public Vector3 worldSlipDirection;
    [HideInInspector]
    public Vector3 suspensionForceDirection;
    [HideInInspector]
    public Vector3 wheelWorldPosition;
}