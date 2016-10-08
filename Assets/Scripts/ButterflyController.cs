using UnityEngine;
using System.Collections;

public class ButterflyController : MonoBehaviour {

	private bool flyAway;
	private float speed = 3.0f;

	// Use this for initialization
	void Start () {
		flyAway = false;
	}
	
	// Update is called once per frame
	void Update () {

		if(flyAway) {
			transform.Translate(Vector3.forward * Time.deltaTime * speed, Space.Self);
			transform.Translate(Vector3.up * Time.deltaTime, Space.World);

			StartCoroutine(destroyButterfly());
		}
	
	}

	void OnTriggerEnter(Collider other) {
		if(other.tag.Equals("Human")) {
			flyAway = true;
            this.GetComponent<Collider>().enabled = false;
        }
	}

	IEnumerator destroyButterfly() {
		yield return new WaitForSeconds(20);
		Destroy(this.gameObject);
	}
}
