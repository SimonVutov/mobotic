using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class poweredWheel
{
    public int wheelState = 1; // 0 = fully free, 1 = powered, 2 = free rolling
    public float biDirectional = 0; // If the wheel can go in reverse to turn the vehicle
    public Vector3 localPosition;
    public float power = 8.0f;
    public float size = 0.3f;
    public float suspensionForce = 90.0f;
    public float grip = 6.0f;
    public float turnAngle = 45.0f;
    public float maxGrip = 400.0f;
    [HideInInspector]
    public float lastSuspensionLength = 0.0f;
    [HideInInspector]
    public float rotationsPerSecond = 0.0f;
    [HideInInspector]
    public Vector3 localSlipDirection;
    [HideInInspector]
    public Vector3 worldSlipDirection;
    [HideInInspector]
    public Vector3 suspensionForceDirection;
    [HideInInspector]
    public Vector3 wheelWorldPosition;
    [HideInInspector]
    public float wheelCircumference;
}