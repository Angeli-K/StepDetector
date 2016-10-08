using UnityEngine;
using System.Collections;

public class ResetOR : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(Input.GetKeyDown(KeyCode.R)) {
			OVRManager.display.RecenterPose ();	
		}

		if(Input.GetKeyDown(KeyCode.Escape)) {
			Application.Quit();
		}
	}
}
