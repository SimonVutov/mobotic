using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public enum Swipe { None, Up, Down, Left, Right, Click };

public class CharacterSelector : MonoBehaviour
{

	public delegate void CharacterSelection(string characterName);
	public static event CharacterSelection OnCharacterSelected;

	public delegate void PurchaseCharacter(CharacterProperty characterProperty);
	public static event PurchaseCharacter OnPurchaseCharacter;

	// Add delegate for vehicle spawning
	public delegate void VehicleSpawn(GameObject vehiclePrefab, Vector3 position, Quaternion rotation);
	public static event VehicleSpawn OnVehicleSpawn;

	// Add delegate for returning to menu
	public delegate void ReturnToMenu();
	public static event ReturnToMenu OnReturnToMenu;

	public RectTransform panel;
	public RectTransform center;
	public GameObject[] prefab;

	// Add a secondary prefab array for the actual vehicles to spawn
	public GameObject[] vehiclePrefabs;

	public GameObject btnFree;
	public GameObject btnPrice;

	public Text txtName;

	public Text txtGeneralCash;
	public Text txtPriceCash;
	public Text txtPriceSold;

	private float[] distance;

	private bool dragging = false;
	private bool checkJoystick = true;

	private int minButtonNum;
	private int currentSelectedPly = -1;

	public float objectScale = 1.7f;
	public int charactersDistance = 300;

	Swipe swipeDirection = Swipe.None;

	// Add a public Vector3 for the spawn point
	public Vector3 vehicleSpawnPoint = Vector3.zero;

	// Reference to the existing camera controller
	public CameraController cameraController;

	// Reference to the UI camera
	public Camera uiCamera;

	void Awake()
	{
		// Find the camera controller if not assigned
		if (cameraController == null)
		{
			cameraController = FindObjectOfType<CameraController>();
			if (cameraController == null)
			{
				Debug.LogWarning("Camera controller not found in the scene. Camera will not follow spawned vehicles.");
			}
		}
	}

	/*
    * Display the current money amount
    */
	void OnEnable()
	{
		txtGeneralCash.text = "" + PlayerPrefs.GetInt("money", 0);
	}

	void Start()
	{
		string selecterPlayer = PlayerPrefs.GetString("SelectedPlayer", "none");
		if (!selecterPlayer.Equals("none")) OnCharacterSelected(selecterPlayer);

		// Validate the vehiclePrefabs array
		if (vehiclePrefabs == null || vehiclePrefabs.Length == 0)
		{
			Debug.LogWarning("Vehicle prefabs array is not set up. Vehicle spawning will not work.");
		}
		else if (vehiclePrefabs.Length != prefab.Length)
		{
			Debug.LogWarning("Vehicle prefabs array length (" + vehiclePrefabs.Length +
							") does not match display prefabs array length (" + prefab.Length +
							"). Please ensure they have the same number of elements.");
		}

		distance = new float[prefab.Length];
		for (int i = 0; i < prefab.Length; i++)
		{
			prefab[i] = Instantiate(prefab[i], center.position, Camera.main.transform.rotation) as GameObject;
			prefab[i].transform.SetParent(panel.transform);
			Vector3 pos = prefab[i].GetComponent<RectTransform>().anchoredPosition;
			pos.x += (i * charactersDistance);
			prefab[i].GetComponent<RectTransform>().anchoredPosition = pos;
		}
		Vector2 newPosition = new Vector2((float)PlayerPrefs.GetInt("SelectedScroll", (int)panel.anchoredPosition.x), panel.anchoredPosition.y);
		panel.anchoredPosition = newPosition;
	}

	void Update()
	{

		//calculate the relative distance
		for (int i = 0; i < prefab.Length; i++)
		{
			distance[i] = Mathf.Abs(center.transform.position.x - prefab[i].transform.position.x);
		}

		float minDistance = Mathf.Min(distance);

		// Aplly the scale to object
		for (int a = 0; a < prefab.Length; a++)
		{
			if (minDistance == distance[a])
			{
				minButtonNum = a;
				if (minButtonNum != currentSelectedPly)
				{
					lookAtPrice(minButtonNum);
					scaleButtonCenter(minButtonNum);
					currentSelectedPly = minButtonNum;
					txtName.text = prefab[minButtonNum].GetComponent<CharacterProperty>().nameObj;
				}
			}
		}

		// if the users aren't dragging the lerp function is called on the prefab
		if (!dragging)
		{
			LerpToBttn(currentSelectedPly * (-charactersDistance));
		}

		//move the selection of the player using joystick or keyboard
		checkJoystickKeyboardInput();

		// Check for Escape or X key to return to menu
		if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.X))
		{
			ReturnToMenuAndDeleteVehicle();
		}

	}

	/*
	 *  Lerp the nearest prefab to center 
	 */
	void LerpToBttn(int position)
	{
		float newX = Mathf.Lerp(panel.anchoredPosition.x, position, Time.deltaTime * 7f);
		Vector2 newPosition = new Vector2(newX, panel.anchoredPosition.y);
		panel.anchoredPosition = newPosition;
	}

	/*
	 * Set the scale of the prefab on center to 2, other to 1
	 */
	public void scaleButtonCenter(int minButtonNum)
	{
		for (int a = 0; a < prefab.Length; a++)
		{
			if (a == minButtonNum)
			{
				StartCoroutine(ScaleTransform(prefab[a].transform, prefab[a].transform.localScale, new Vector3(objectScale, objectScale, objectScale)));
			}
			else
			{
				StartCoroutine(ScaleTransform(prefab[a].transform, prefab[a].transform.localScale, new Vector3(1f, 1f, 1f)));
			}
		}
	}

	/*
	 * If the prefab is not free, show the price button
	 */
	public void lookAtPrice(int minButtonNum)
	{
		CharacterProperty chrProperty = prefab[minButtonNum].GetComponent<CharacterProperty>();
		if (chrProperty.price == 0 || PlayerPrefs.GetInt(chrProperty.name, -1) == 7)
		{
			btnFree.SetActive(true);
			btnPrice.SetActive(false);
		}
		else
		{
			btnFree.SetActive(false);
			btnPrice.SetActive(true);
			txtPriceCash.text = "" + ((int)CharacterProperty.CONVERSION_RATE * chrProperty.price);
			txtPriceSold.text = chrProperty.price + " €";
		}
	}

	/*
	 * Courutine for change the scale
	 */
	IEnumerator ScaleTransform(Transform transformTrg, Vector3 initScale, Vector3 endScale)
	{
		float completeTime = 0.2f;//How much time will it take to scale
		float currentTime = 0.0f;
		bool done = false;

		txtGeneralCash.color = new Color(255, 255, 255); // reset color to white

		while (!done)
		{
			float percent = Mathf.Abs(currentTime / completeTime);
			if (percent >= 1.0f)
			{
				percent = 1;
				done = true;
			}
			transformTrg.localScale = absV3(Vector3.Lerp(initScale, endScale, percent));
			currentTime += Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
	}

	/*
	 * Called by the canvas, set dragging to true for preventing lerp when users are dragging
	 */
	public void StartDrag()
	{
		dragging = true;
	}

	/*
	 * Called by the canvas, set dragging to true for preventing lerp when users are dragging
	 */
	public void EndDrag()
	{
		dragging = false;
	}

	/*
	 * Called when character is selected, it change the player model
	 */
	public void CharacterSelected()
	{
		string nameSelected = prefab[currentSelectedPly].GetComponent<CharacterProperty>().name;
		nameSelected = nameSelected.Split('(')[0];
		PlayerPrefs.SetString("SelectedPlayer", nameSelected);
		PlayerPrefs.SetInt("SelectedScroll", (int)panel.anchoredPosition.x);
		if (OnCharacterSelected != null)
		{
			OnCharacterSelected(nameSelected);
		}

		// Spawn the selected vehicle
		SpawnSelectedVehicle();
	}

	/*
     * Spawns the selected vehicle in the game world
     */
	public void SpawnSelectedVehicle()
	{
		if (currentSelectedPly >= 0 && currentSelectedPly < prefab.Length)
		{
			// Check if the vehiclePrefabs array is properly set up
			if (vehiclePrefabs == null || vehiclePrefabs.Length == 0)
			{
				Debug.LogError("Vehicle prefabs array is not set up. Please assign vehicle prefabs in the inspector.");
				return;
			}

			// Check if the arrays have matching lengths
			if (vehiclePrefabs.Length != prefab.Length)
			{
				Debug.LogError("Vehicle prefabs array length does not match display prefabs array length. Please ensure they have the same number of elements.");
				return;
			}

			// Get the display prefab to check properties
			GameObject displayPrefab = prefab[currentSelectedPly];
			CharacterProperty chrProperty = displayPrefab.GetComponent<CharacterProperty>();

			// Check if the character is unlocked before spawning
			if (PlayerPrefs.GetInt(chrProperty.name, -1) > 0 || chrProperty.price == 0)
			{
				// Use the public Vector3 for spawn position
				Vector3 spawnPosition = vehicleSpawnPoint;

				// Get the corresponding vehicle prefab to spawn
				GameObject vehiclePrefabToSpawn = vehiclePrefabs[currentSelectedPly];

				// Store the spawned vehicle reference
				GameObject spawnedVehicle = null;

				// Trigger the spawn event
				if (OnVehicleSpawn != null)
				{
					OnVehicleSpawn(vehiclePrefabToSpawn, spawnPosition, Quaternion.identity);
					// Note: We can't directly get the spawned vehicle reference when using the event
				}
				else
				{
					// Direct spawn if no listeners are registered
					spawnedVehicle = Instantiate(vehiclePrefabToSpawn, spawnPosition, Quaternion.identity);

					// Set the camera target if camera controller is assigned
					if (cameraController != null && spawnedVehicle != null)
					{
						// Use the existing camera controller's target property
						cameraController.target = spawnedVehicle.transform;
						Debug.Log("Camera target set to spawned vehicle: " + spawnedVehicle.name);
					}
					else if (cameraController == null)
					{
						Debug.LogWarning("Camera controller not assigned. Cannot set camera target.");
					}
				}

				// Disable the UI camera when driving
				if (uiCamera != null)
				{
					uiCamera.enabled = false;
					Debug.Log("UI camera disabled for driving");
				}

				Debug.Log("Vehicle spawned: " + chrProperty.nameObj);
			}
			else
			{
				Debug.Log("Cannot spawn vehicle: " + chrProperty.nameObj + " - Not unlocked yet!");
			}
		}
	}

	/*
	 * Called when player try to buy character with cash
	 */
	public void buyCharacterWithCash()
	{
		CharacterProperty chrProperty = prefab[minButtonNum].GetComponent<CharacterProperty>();
		int cashNedeed = (int)(CharacterProperty.CONVERSION_RATE * chrProperty.price);
		int totalCash = int.Parse(txtGeneralCash.text);
		if (cashNedeed <= totalCash)
		{
			totalCash -= cashNedeed;
			txtGeneralCash.text = "" + totalCash;

			btnFree.SetActive(true);
			btnPrice.SetActive(false);

			PlayerPrefs.SetInt(chrProperty.name, 7);
			PlayerPrefs.SetInt("money", totalCash);
		}
		else
		{
			txtGeneralCash.color = new Color(255, 0, 0);
		}
	}

	/*
	 * Change to use the desired payment method
	 */
	public void buyCharacterWithPayment()
	{
		CharacterProperty chrProperty = prefab[minButtonNum].GetComponent<CharacterProperty>();
		if (OnPurchaseCharacter != null)
		{
			OnPurchaseCharacter(chrProperty);
		}
	}

	/**
	 * Unlock the character and save the purchase
	 */
	public void paymentConfirmed(CharacterProperty chrProperty)
	{
		PlayerPrefs.SetInt(chrProperty.name, 7);
		btnFree.SetActive(true);
		btnPrice.SetActive(false);
	}

	/**
	 * Abs on Vector3
	 */
	private Vector3 absV3(Vector3 v3)
	{
		return new Vector3(Mathf.Abs(v3.x), Mathf.Abs(v3.y), Mathf.Abs(v3.z));
	}

	/*
	 * Check and switch player selection from a generic joystick or keyboard
	 */
	void checkJoystickKeyboardInput()
	{
		//Joystick Input
		if (Input.GetAxis("Horizontal") == 0) checkJoystick = true;
		if (checkJoystick)
		{
			if (Input.GetAxis("Horizontal") > 0)
			{
				swipeDirection = Swipe.Right; // gets right
				checkJoystick = false;
			}
			if (Input.GetAxis("Horizontal") < 0)
			{
				swipeDirection = Swipe.Left; // gets left
				checkJoystick = false;
			}

			if (swipeDirection == Swipe.Right)
			{
				Vector2 newPosition = new Vector2(panel.anchoredPosition.x - charactersDistance, panel.anchoredPosition.y);
				panel.anchoredPosition = newPosition;
			}
			else if (swipeDirection == Swipe.Left)
			{
				Vector2 newPosition = new Vector2(panel.anchoredPosition.x + charactersDistance, panel.anchoredPosition.y);
				panel.anchoredPosition = newPosition;
			}
			swipeDirection = Swipe.None;
		}
	}

	/*
	 * Count the number of unlocked characters
	 */
	int countUnlockedCharacters()
	{
		int unlockedCharacters = 0;
		for (int i = 0; i < prefab.Length; i++)
		{
			CharacterProperty chrProperty = prefab[i].GetComponent<CharacterProperty>();
			if (PlayerPrefs.GetInt(chrProperty.name, -1) > 0 || chrProperty.price == 0)
			{
				unlockedCharacters++;
			}
		}
		return unlockedCharacters;
	}

	/*
	 * Returns to the menu and deletes any spawned vehicle
	 */
	public void ReturnToMenuAndDeleteVehicle()
	{
		// Enable the UI camera when returning to menu
		if (uiCamera != null)
		{
			uiCamera.enabled = true;
			Debug.Log("UI camera enabled for menu");
		}

		// Trigger the return to menu event
		if (OnReturnToMenu != null)
		{
			OnReturnToMenu();
		}

		// Find and activate the menu canvas
		ManagerSelector menuManager = FindObjectOfType<ManagerSelector>();
		if (menuManager != null)
		{
			menuManager.ShowMenu();
		}
		else
		{
			Debug.LogWarning("ManagerSelector not found. Cannot show menu.");
		}
	}
}
