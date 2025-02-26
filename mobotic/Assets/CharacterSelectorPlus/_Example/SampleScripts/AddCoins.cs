using UnityEngine;
using System.Collections;

public class AddCoins : MonoBehaviour {

	/**
	 * This function add 5000 coins to the wallet for the test
	 */
	void Awake () {
		if(PlayerPrefs.GetInt ("money", 0) == 0){
			PlayerPrefs.SetInt ("money",5000);
		}
	}
}
