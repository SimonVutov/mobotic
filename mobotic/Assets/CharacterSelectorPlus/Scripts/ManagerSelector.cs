using UnityEngine;

public class ManagerSelector : MonoBehaviour {
	
	private Canvas canvas;
	
	void Start(){
		canvas = gameObject.GetComponent<Canvas>();
		canvas.enabled = false;
		transform.Find ("Panel").gameObject.SetActive (false);

	}

	public void clickCanvas(){
		canvas.enabled = !canvas.enabled;
		GameObject panel = transform.Find ("Panel").gameObject;
		panel.SetActive (canvas.enabled);
	}
}