using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using System.Text.RegularExpressions;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class CANBus : MonoBehaviour
{
    public WheelReference[] wheels;



    private SerialPort serialPort;
    public string portName = "COM3"; // Change this to match your CANable COM port
    public int baudRate = 115200;

    // Start is called before the first frame update
    void Start()
    {
        try
        {
            serialPort = new SerialPort(portName, baudRate);
            serialPort.Open();
            Debug.Log("Connected to CANable on " + portName);
        }
        catch (Exception e)
        {
            Debug.LogError("Error opening serial port: " + e.Message);
        }
    }

    void Update()
    {
        //read CAN
        if (serialPort != null && serialPort.IsOpen)
        {
            try
            {
                string message = serialPort.ReadLine().Trim(); // Read incoming message
                Debug.Log("Raw CAN Data: " + message);

                CANFrame parsedFrame = ParseSLCANMessage(message);
                if (parsedFrame != null)
                {
                    Debug.Log($"Parsed CAN ID: {parsedFrame.canId}, DLC: {parsedFrame.dataLength}, Data: {BitConverter.ToString(parsedFrame.data)}");
                }
            }
            catch (TimeoutException) { }
        }

        //Read steering and traction values from 
        foreach (WheelReference child in wheels)
        {
            float SteeringAngle=child.wheel.GetComponent<WheelComponent>().turnAngle;
            float RPM = child.wheel.GetComponent<WheelComponent>().wheelRotationSpeed;
            Debug.Log("SteeringAngle: " + SteeringAngle+ " RPM:"+RPM);

        }

    }

    public class CANFrame
    {
        public int canId;
        public int dataLength;
        public byte[] data;
    }

    CANFrame ParseSLCANMessage(string message)
    {
        // Check if it's a standard 11-bit CAN frame
        if (Regex.IsMatch(message, @"^t[0-9A-Fa-f]{3}[0-8][0-9A-Fa-f]*$"))
        {
            int canId = Convert.ToInt32(message.Substring(1, 3), 16); // Extract CAN ID
            int dataLength = int.Parse(message.Substring(4, 1)); // Extract Data Length
            byte[] data = new byte[dataLength];

            for (int i = 0; i < dataLength; i++)
            {
                string byteHex = message.Substring(5 + (i * 2), 2);
                data[i] = Convert.ToByte(byteHex, 16);
            }

            return new CANFrame { canId = canId, dataLength = dataLength, data = data };
        }

        Debug.LogWarning("Invalid CAN message: " + message);
        return null;
    }
    public void SendCANMessage(int canId, byte[] data)
    {
        if (serialPort != null && serialPort.IsOpen)
        {
            string canIdHex = canId.ToString("X3"); // Convert ID to 3-digit hex
            string dataHex = BitConverter.ToString(data).Replace("-", ""); // Convert data to hex

            string message = $"t{canIdHex}{data.Length}{dataHex}\r"; // Format as SLCAN
            serialPort.WriteLine(message);
            Debug.Log("Sent CAN Message: " + message);
        }
        else
        {
            Debug.LogError("Serial port is not open!");
        }
    }

}
