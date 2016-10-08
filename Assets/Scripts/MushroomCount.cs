using UnityEngine;
using System.Collections;

public class MushroomCount : MonoBehaviour {

	public int mushroomN = 0;

	private float minZ;
	private float maxZ; 

	// Use this for initialization
	void Start () {
		HumanControlScript HCSkript = GameObject.FindGameObjectWithTag ("Human").GetComponent < HumanControlScript > ();
		minZ = HCSkript.minZ;
		maxZ = HCSkript.maxZ;

		GameObject[] mushroomList = GameObject.FindGameObjectsWithTag ("Fliegenpilz");
		foreach ( GameObject mushroom in mushroomList) {
			if(minZ <= mushroom.transform.localPosition.z && mushroom.transform.localPosition.z <= maxZ)
				mushroomN ++;
		}

		Debug.Log(mushroomN);
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
