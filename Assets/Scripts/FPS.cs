using UnityEngine;
using System.Collections;

public class FPS : MonoBehaviour {

    // FPS variables
    private static float updateInterval = 0.5F;
    private static float accum = 0;                // FPS accumulated over the interval
    private static int frames = 0;                 // Frames drawn over the interval
    private static float timeleft;                 // Left time for current interval

    // Use this for initialization
    void Start () {
        timeleft = updateInterval;
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(getFPS());
    }

    public static float getFPS() {
        // Calculate current FPS
        timeleft -= Time.deltaTime;
        accum += Time.timeScale / Time.deltaTime;
        ++frames;

        float fps = 0.0f;

        // Interval ended - save FPS and start new interval
        if (timeleft <= 0.0) {
            fps = accum / frames;
        }

        //timeleft = updateInterval;
        //accum = 0.0F;
        //frames = 0;

        return fps;
    }
}
