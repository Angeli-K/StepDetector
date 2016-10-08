using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class StepSoundR : MonoBehaviour {

	public List <AudioClip> ACListR;

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void playSoundR () {
		int SoundNr = Random.Range (0, ACListR.Count);
		this.GetComponent <AudioSource> ().clip = ACListR [SoundNr];
		this.GetComponent <AudioSource> ().Play ();
	}

}
