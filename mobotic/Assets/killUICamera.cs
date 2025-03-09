using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class killUICamera : MonoBehaviour
{
    private Camera mainCamera;
    private GameObject firstChild;
    
    // Start is called before the first frame update
    void Start()
    {
        // Find the main camera in the scene
        mainCamera = Camera.main;
        
        if (mainCamera != null && mainCamera.transform.childCount > 0)
        {
            // Get the first child of the main camera
            firstChild = mainCamera.transform.GetChild(0).gameObject;
            
            // Disable the first child
            firstChild.SetActive(false);
        }
        else
        {
            if (mainCamera == null)
            {
                Debug.LogWarning("Main camera not found!");
            }
            else
            {
                Debug.LogWarning("Main camera has no children!");
            }
        }
    }
    
    // When this object is destroyed, re-enable the first child of the main camera
    void OnDestroy()
    {
        // Re-enable the first child if it exists
        if (firstChild != null)
        {
            firstChild.SetActive(true);
        }
    }
}