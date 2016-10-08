using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class LowPassFilter : MonoBehaviour {

    public List<float> ax, ay, vx, vy, px, py, window, time, deltatime, filt_ax, filt_ay, filt_vx, filt_vy, filt_px, filt_py;
    public List<ExpectedStep> expectedStep;
    private static int maxlength = 300;

    private float avgfps;					// Average frames per second

    public static float sigma = 0.1f;
    private float sigma_samples = 0;		// Sigma expressed in samples
    public static float sigma_breadth = 6;	// How many sigmas should the window be wide
    private float filterlength = 0;			// Resulting complete window size (in samples)

    private float avgstep_x = 0;			// The calculated distance between step cycles
    private float avgstep_y = 0;			// The calculated distance between two steps (foot-strike to foot-strike)

    public static FilterUtilities acc_x_util = new FilterUtilities();
    public static FilterUtilities acc_y_util = new FilterUtilities();
    public static FilterUtilities vel_x_util = new FilterUtilities();
    public static FilterUtilities vel_y_util = new FilterUtilities();
    public static FilterUtilities pos_x_util = new FilterUtilities();
    public static FilterUtilities pos_y_util = new FilterUtilities();

    private StepManager stepManagerScript;

    public TextMesh displayRightLeft;

    // Use this for initialization
    void Start () {
        //timeleft = updateInterval;
        ax = new List<float>();
        ay = new List<float>();
        vx = new List<float>();
        vy = new List<float>();
        px = new List<float>();
        py = new List<float>();
        window = new List<float>();
        time = new List<float>();
        deltatime = new List<float>();
        filt_ax = new List<float>();
        filt_ay = new List<float>();
        filt_vx = new List<float>();
        filt_vy = new List<float>();
        filt_px = new List<float>();
        filt_py = new List<float>();
        expectedStep = new List<ExpectedStep>();

        acc_x_util.init();
        acc_y_util.init();
        vel_x_util.init();
        vel_y_util.init();
        pos_x_util.init();
        pos_y_util.init();

        stepManagerScript = GameObject.FindGameObjectWithTag("Human").GetComponent<StepManager>();
    }
	
	// Update is called once per frame
	void Update () {
        //Debug.Log(getFPS());
        //Read new values at beginning of frame
        Vector3 temp = OculusSensorData.rotatedAccVector;
        ax.Add(temp.x);
        ay.Add(temp.y);
        temp = OculusSensorData.rotatedVelVector;
        vx.Add(temp.x);
        vy.Add(temp.y);
        temp = OculusSensorData.posVector;
        px.Add(temp.x);
        py.Add(temp.y);
        time.Add(Time.time);
        deltatime.Add(Time.deltaTime);

        //delete old values at end of frame
        clipList(ax, maxlength);
        clipList(ay, maxlength);
        clipList(vx, maxlength);
        clipList(vy, maxlength);
        clipList(px, maxlength);
        clipList(py, maxlength);
        clipList(time, maxlength);
        clipList(deltatime, maxlength);
        clipList(filt_ax, maxlength);
        clipList(filt_ay, maxlength);
        clipList(filt_vx, maxlength);
        clipList(filt_vy, maxlength);
        clipList(filt_px, maxlength);
        clipList(filt_py, maxlength);

        //main work
        if (deltatime.Count >= maxlength) {
            //avgfps = deltatime.Average();
            sigma_samples = sigma / deltatime.Average();
            filterlength = sigma_breadth * sigma_samples;
            filterlength = Mathf.Ceil((filterlength + 1) / 2) * 2 - 1; // Round up filter size and force odd length
            for(int i = (int)-filterlength/2; i < filterlength/2; i++) {
                window.Add(Mathf.Exp(-i*i/(2 * sigma_samples * sigma_samples)));
            }
            
            float sum = 0;
            sum = window.Sum();
            window = window.Select(d => d / sum).ToList();
            //window = window.Select(d => d / window.Sum());

            filt_ax.Add( dotProduct(ax, window));
            filt_ay.Add( dotProduct(ay, window));
            filt_vx.Add( dotProduct(vx, window));
            filt_vy.Add( dotProduct(vy, window));
            filt_px.Add( dotProduct(px, window));
            filt_py.Add( dotProduct(py, window));

            acc_x_util.setValue(dotProduct(ax, window), FilterUtilities.Signal.Acc);
            acc_y_util.setValue(dotProduct(ay, window), FilterUtilities.Signal.Acc);
            vel_x_util.setValue(dotProduct(vx, window), FilterUtilities.Signal.Vel);
            vel_y_util.setValue(dotProduct(vy, window), FilterUtilities.Signal.Vel);
            pos_x_util.setValue(dotProduct(px, window), FilterUtilities.Signal.Pos);
            pos_y_util.setValue(dotProduct(py, window), FilterUtilities.Signal.Pos);

            avgstep_x = 0;
            avgstep_x += calculateAverageStep(acc_x_util.maxList, vel_x_util.maxList, pos_x_util.maxList);
            avgstep_x += calculateAverageStep(acc_x_util.minList, vel_x_util.minList, pos_x_util.minList);
            avgstep_x += calculateAverageStep(acc_x_util.risingZeroList, vel_x_util.risingZeroList, pos_x_util.risingZeroList);
            avgstep_x += calculateAverageStep(acc_x_util.fallingZeroList, vel_x_util.fallingZeroList, pos_x_util.fallingZeroList);
            avgstep_x = avgstep_x / 4;

            avgstep_y = 0;
            avgstep_y += calculateAverageStep(acc_y_util.maxList, vel_y_util.maxList, pos_y_util.maxList);
            avgstep_y += calculateAverageStep(acc_y_util.minList, vel_y_util.minList, pos_y_util.minList);
            avgstep_y += calculateAverageStep(acc_y_util.risingZeroList, vel_y_util.risingZeroList, pos_y_util.risingZeroList);
            avgstep_y += calculateAverageStep(acc_y_util.fallingZeroList, vel_y_util.fallingZeroList, pos_y_util.fallingZeroList);
            avgstep_y = avgstep_y / 4;
            
            if (acc_y_util.max == true) {
                acc_y_util.max = false;

                // check if left or right
                StepManager.StepState expStep = StepManager.StepState.Unknown;
                if (acc_x_util._state.Equals(FilterUtilities.LastFound.Maximum)) {
                    expStep = StepManager.StepState.Left;
                } else if (acc_x_util._state.Equals(FilterUtilities.LastFound.Minimum)) {
                    expStep = StepManager.StepState.Right;
                }
                //Debug.Log(expStep.ToString());

                float window_delay = sigma * sigma_breadth / 2;
                float t = Time.time + (avgstep_x / 2 + avgstep_y) / 2 - window_delay;
                if (t > 0) {
                    expectedStep.Add(new ExpectedStep { time = t, state = expStep });
                }
            }

            if (expectedStep.Count > 0) {
                ExpectedStep expected = expectedStep.First();
                if ((expected.time - Time.time) < 0) {
                    StepManager.StepState _state = expected.state;
                    displayRightLeft.text = _state.ToString();
                    expectedStep.RemoveAt(0);

                    // Update animation
                    if (_state.Equals(StepManager.StepState.Right)) {
                        stepManagerScript.stepStates[3].state = StepManager.StepState.Right;
                        stepManagerScript.stepChanged();
                    } else if (_state.Equals(StepManager.StepState.Left)) {
                        stepManagerScript.stepStates[3].state = StepManager.StepState.Left;
                        stepManagerScript.stepChanged();
                    }
                }
            }


            //Done filtering, now clear the window
            window.Clear();
        }

        // Show or hide debug text
        if (Input.GetKeyDown(KeyCode.M)) {
            displayRightLeft.gameObject.SetActive(!displayRightLeft.gameObject.activeSelf);
        }

    }

    private float dotProduct(List<float> original, List<float> window) {
        double temp = 0;
        for (int i = 0; i < window.Count; i++) {
            temp += original.ElementAt(original.Count - i - 1) * window.ElementAt(i);
        }
        return (float)temp;
    }

    private void clipList(List<float> list, int maxlength) {
        while (list.Count > maxlength) {
            list.RemoveAt(0);
        }
    }

    private float calculateAverageStep(List<float> list1, List<float> list2, List<float> list3) {
        float avgstep = 0;
        avgstep += acc_x_util.calculateAverageTime(list1);
        avgstep += vel_x_util.calculateAverageTime(list2);
        avgstep += pos_x_util.calculateAverageTime(list3);

        avgstep = avgstep / 3;
        return avgstep;
    }

    public class ExpectedStep {
        public float time { get; set; }
        public StepManager.StepState state { get; set; }
    }

}
