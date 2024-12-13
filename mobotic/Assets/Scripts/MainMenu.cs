using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;   // <-- TextMeshPro namespace

public class MainMenu : MonoBehaviour
{
    [Header("UI References")]
    public GameObject mainMenu;
    public GameObject LoadSceneMenu;
    public GameObject optionsMenu;
    public GameObject builderMenu;

    [Header("Button Generation")]
    public GameObject buttonPrefab;    // Prefab must have a Button + TextMeshProUGUI on child
    public Transform buttonParent;     // Parent transform (UI object under Canvas)
    public int numberOfVehicles = 4;   // Number of vehicle options

    public string gameSceneName = "MyGameScene";  // Single scene to load

    private void Start()
    {
        CreateVehicleButtons();
    }

    private void CreateVehicleButtons()
    {
        // (Optional) Clear any existing children
        foreach (Transform child in buttonParent)
        {
            Destroy(child.gameObject);
        }

        float baseY = 0f;      // Y position for the first button
        float ySpacing = 40f;  // Distance between buttons

        for (int i = 0; i < numberOfVehicles; i++)
        {
            GameObject newButton = Instantiate(buttonPrefab, buttonParent);
            newButton.name = $"VehicleButton_{i}";

            // If the button’s child is TextMeshProUGUI, set the label
            TMP_Text textComp = newButton.GetComponentInChildren<TMP_Text>();
            if (textComp != null)
            {
                textComp.text = $"Vehicle {i}";
            }

            // Position the button 40 units lower each iteration
            RectTransform rt = newButton.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, baseY - ySpacing * i);

            // Wire up the click event
            int vehicleIndex = i;
            Button btn = newButton.GetComponent<Button>();
            btn.onClick.AddListener(() =>
            {
                VehicleSelection.selectedVehicleIndex = vehicleIndex;
                SceneManager.LoadScene(gameSceneName);
            });
        }
    }

    public void PlayGame()
    {
        mainMenu.SetActive(false);
        LoadSceneMenu.SetActive(true);
    }

    public void Builder()
    {
        mainMenu.SetActive(false);
        builderMenu.SetActive(true);
    }

    public void Options()
    {
        mainMenu.SetActive(false);
        optionsMenu.SetActive(true);
    }

    public void Back()
    {
        mainMenu.SetActive(true);
        LoadSceneMenu.SetActive(false);
        builderMenu.SetActive(false);
        optionsMenu.SetActive(false);
    }

    public void QuitGame()
    {
        Debug.Log("Quit");
        Application.Quit();
    }
}