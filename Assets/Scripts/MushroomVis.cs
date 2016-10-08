using UnityEngine;
using System.Collections;

public class MushroomVis : MonoBehaviour {

	// Use this for initialization
	void Awake () {
		this.gameObject.SetActive(randomBoolean());

	}
	
	// Update is called once per frame
	void Update () {
	
	}

	private bool randomBoolean() {
		if (Random.value >= 0.5)
			return true;
		else
			return false;
	}
}
