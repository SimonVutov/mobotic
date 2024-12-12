using UnityEngine;
using UnityEngine.SceneManagement;

public class EscapeKeyListener : MonoBehaviour
{
    void Update()
    {
        // Check if the 'x' key is pressed
        if (Input.GetKeyDown(KeyCode.X))
        {
            // Load Scene 0
            SceneManager.LoadScene(0);
        }
    }
}