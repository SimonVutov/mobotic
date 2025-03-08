using UnityEngine;
using UnityEngine.InputSystem;

public class GamepadTester : MonoBehaviour
{
    private Gamepad gamepad;

    void Start()
    {
        gamepad = Gamepad.current;
        if (gamepad == null)
        {
            Debug.LogError("No gamepad detected! Make sure your Logitech F710 is connected.");
        }
    }

    void Update()
    {
        if (gamepad == null) return;

        // Button Tests
        if (gamepad.buttonSouth.wasPressedThisFrame) Debug.Log("A Button Pressed");
        if (gamepad.buttonNorth.wasPressedThisFrame) Debug.Log("Y Button Pressed");
        if (gamepad.buttonEast.wasPressedThisFrame) Debug.Log("B Button Pressed");
        if (gamepad.buttonWest.wasPressedThisFrame) Debug.Log("X Button Pressed");

        if (gamepad.leftTrigger.wasPressedThisFrame) Debug.Log("Left Trigger Pressed");
        if (gamepad.rightTrigger.wasPressedThisFrame) Debug.Log("Right Trigger Pressed");

        if (gamepad.leftShoulder.wasPressedThisFrame) Debug.Log("Left Bumper Pressed");
        if (gamepad.rightShoulder.wasPressedThisFrame) Debug.Log("Right Bumper Pressed");

        if (gamepad.startButton.wasPressedThisFrame) Debug.Log("Start Button Pressed");
        if (gamepad.selectButton.wasPressedThisFrame) Debug.Log("Select Button Pressed");

        // D-Pad
        if (gamepad.dpad.up.wasPressedThisFrame) Debug.Log("D-Pad Up Pressed");
        if (gamepad.dpad.down.wasPressedThisFrame) Debug.Log("D-Pad Down Pressed");
        if (gamepad.dpad.left.wasPressedThisFrame) Debug.Log("D-Pad Left Pressed");
        if (gamepad.dpad.right.wasPressedThisFrame) Debug.Log("D-Pad Right Pressed");

        // Stick Press
        if (gamepad.leftStickButton.wasPressedThisFrame) Debug.Log("Left Stick Pressed");
        if (gamepad.rightStickButton.wasPressedThisFrame) Debug.Log("Right Stick Pressed");

        // Stick Movement
        Vector2 leftStick = gamepad.leftStick.ReadValue();
        Vector2 rightStick = gamepad.rightStick.ReadValue();
        if (leftStick.magnitude > 0.1f) Debug.Log($"Left Stick: {leftStick}");
        if (rightStick.magnitude > 0.1f) Debug.Log($"Right Stick: {rightStick}");

        // Triggers (Analog)
        if (gamepad.leftTrigger.ReadValue() > 0.1f) Debug.Log($"Left Trigger: {gamepad.leftTrigger.ReadValue()}");
        if (gamepad.rightTrigger.ReadValue() > 0.1f) Debug.Log($"Right Trigger: {gamepad.rightTrigger.ReadValue()}");
    }
}
