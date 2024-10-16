using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[System.Serializable]
public class poweredWheel
{
    [HideInInspector]
    public bool inContact = false;
    public bool freeWheel = false; // enabling will make the wheel free to rotate and move in any direction
    // Attributes for the powered wheel
    public Vector3 localPosition;
    public float power = 8.0f;
    public float size = 0.3f;
    public float suspensionForce = 90.0f;
    public float grip = 6.0f;
    public bool turnable = false;
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
}