using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class vehicleControl : MonoBehaviour
{
    public Transform[] wheels;

    public int sectionCount = 1;
    public float massInKg = 100.0f;

    void Awake()
    {
        foreach (Transform child in wheels)
        {
            child.GetComponent<WheelComponent>().parentRigidbody = GetComponent<Rigidbody>();
            child.GetComponent<WheelComponent>().vehicleController = this;

        }
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        foreach (Transform child in wheels)
        {
            child.GetComponent<WheelComponent>().input = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        }
    }
}
