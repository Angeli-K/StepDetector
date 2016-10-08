using UnityEngine;
using System.Collections;

public class TerrainScript : MonoBehaviour {

	private SimpleGenerator simpleGenerator;

	private bool terrainCreated;

	// Use this for initialization
	void Start () {
		simpleGenerator = GameObject.Find ("SimpleGenerator").GetComponent<SimpleGenerator>();
		terrainCreated = false;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	void OnTriggerEnter(Collider other) {
//		Debug.Log ("OnTriggerEnter: " + other.tag);
		if(!terrainCreated && other.tag.Equals("Human")) {
			terrainCreated = true;
			simpleGenerator.createNewTerrain();
		}
	}

	void OnTriggerExit(Collider other) {
//		Debug.Log ("OnTriggerExit: " + other.tag);
		if(other.tag.Equals("Human")) {
			StartCoroutine(destroyTerrain());
		}
	}
	
	IEnumerator destroyTerrain() {
		yield return new WaitForFixedUpdate();
		Destroy(this.gameObject);
	}
}
