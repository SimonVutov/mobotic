using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class VehControllDiff : MonoBehaviour
{
    public WheelComponent LeftWheel;
    public WheelComponent RightWheel;
    public float MaxTorque = 200;
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
    void Update() { 
    
        if (gamepad == null) return;
        Vector2 leftStick = gamepad.leftStick.ReadValue();
        Vector2 rightStick = gamepad.rightStick.ReadValue();
        RightWheel.input = new Vector2(0, rightStick.y* MaxTorque );
        LeftWheel.input = new Vector2(0, leftStick.y* MaxTorque * (-1));
        




    }
}
