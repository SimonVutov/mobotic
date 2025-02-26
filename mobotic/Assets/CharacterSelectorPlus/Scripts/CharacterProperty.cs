using UnityEngine;

public class CharacterProperty : MonoBehaviour {
	public string nameObj;
	public float price = 0;
	public static int CONVERSION_RATE = 500;

	void Awake(){
		transform.localScale = new Vector3 (0, 0, 0);
	}
}
