using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehControllThreeWheel : MonoBehaviour
{
    public WheelComponent LeftWheel;
    public WheelComponent RightWheel;
    public float MaxTorque = 100;
    public float MaxSteering = -90;
    private Gamepad gamepad;
    // Start is called before the first frame update
    void Start()
    {
        gamepad = Gamepad.current;
        if (gamepad == null)
        {
            Debug.LogError("No gamepad detected! Make sure your Logitech F710 is connected.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (gamepad == null) return;
        Vector2 leftStick = gamepad.leftStick.ReadValue();
        LeftWheel.input = new Vector2(leftStick.x * MaxSteering, leftStick.y * MaxTorque);
        RightWheel.input = new Vector2(leftStick.x * MaxSteering, leftStick.y * MaxTorque);

    }
}
