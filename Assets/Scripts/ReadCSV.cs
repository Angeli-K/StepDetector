using UnityEngine;
using System.Collections;
using System.IO;
using System.Collections.Generic;

public class ReadCSV : MonoBehaviour {

	public string filename;
	public int timeIdx, positionIdx, orientationIdx, accelerationIdx, velocityIdx;

	public GameObject cube;
	public GameObject accelObj;
	public GameObject velObj;

	private List<replayData> dataList;

	public int counter;

	// Use this for initialization
	void Start () {
		counter = 0;
		loadReplayData ();
	}
	
	// Update is called once per frame
	void Update () {
		if (counter >= dataList.Count)
			return;
		cube.transform.position = dataList [counter].position;
		cube.transform.rotation = dataList [counter].orientation;

		//Acceleration vector and velocity vector transforms go here

		counter++;
	}
		
	private void loadReplayData () {
		StreamReader reader = new StreamReader (File.OpenRead ("Measurements/" + filename + ".csv"));
		dataList = new List<replayData> ();
		
		// First line is a comment.
		string line = reader.ReadLine ();
		string[] values = line.Split (';');
		/*foreach(string s in values) {
			Debug.Log (s);
		}*/
		
		while (!reader.EndOfStream) {
			line = reader.ReadLine ();
			values = line.Split (';');
			
			dataList.Add (new replayData { time = float.Parse (values [timeIdx]),
				position = new Vector3 (float.Parse (values [positionIdx]),
					float.Parse (values [positionIdx + 1]), 
					float.Parse (values [positionIdx + 2])),
				orientation = new Quaternion (float.Parse (values [orientationIdx + 0]), 
					float.Parse (values [orientationIdx + 1]), 
					float.Parse (values [orientationIdx + 2]), 
					float.Parse (values [orientationIdx + 3])),
				acceleration = new Vector3 (float.Parse (values [accelerationIdx]),
					float.Parse (values [accelerationIdx + 1]),
					float.Parse (values [accelerationIdx + 2])),
				velocity = new Vector3 (float.Parse (values [velocityIdx]),
					float.Parse (values [velocityIdx + 1]),
					float.Parse (values [velocityIdx + 2]))
				
			});
		}
	}
}

public class replayData {
	public float time { get; set; }				// Simulation Time
	public Vector3 position { get; set; }		// Position
	public Quaternion orientation { get; set; }	// Orientation
	public Vector3 acceleration { get; set; }	// acceleration
	public Vector3 velocity { get; set; }		// velocity
}