using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StepSoundL : MonoBehaviour {

	public List <AudioClip> ACListL;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {

	}

	public void playSoundL() {
		int SoundNr = Random.Range (0, ACListL.Count);
		this.GetComponent <AudioSource> ().clip = ACListL [SoundNr];
		this.GetComponent <AudioSource> ().Play ();
	}

}