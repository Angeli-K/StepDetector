using UnityEngine;
using System.Collections;

using System.Collections.Generic;

public class SimpleGenerator : MonoBehaviour {
	
	private Transform terrain;				// Terrain prefab

	private float z;

	public List<Transform> terrainList;		// List with terrain prefabs

	private int index;

	// Use this for initialization
	void Start () {
		z = 0;
		index = 0;
		//terrain = terrainList[0];
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void createNewTerrain() {
		float terrainLength = 2000;
		z += terrainLength;
		index++;

		// Instantiate one copy of terrain prefab
		terrain = terrainList[index == -1 ? 0 : index % terrainList.Count];
		Instantiate(terrain, new Vector3(0, 0, z), Quaternion.identity);
	}
}
