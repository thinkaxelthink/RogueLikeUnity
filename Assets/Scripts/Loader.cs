using UnityEngine;
using System.Collections;

public class Loader : MonoBehaviour {

	public GameObject gameManager;

	// Use this for initialization
	void Awake () {
		// use instance var from GM
		if (GameManager.instance == null) {
			Instantiate (gameManager);
		}
	}
}
