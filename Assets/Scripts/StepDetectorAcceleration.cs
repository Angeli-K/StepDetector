using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;


public class StepDetectorAcceleration : MonoBehaviour {

	public TextMesh displayRightLeft;

	// Variables to detect steps on low level
	private bool stepActive = false;
	private float xStepNow = 0;								// Stores the value of x at start of detected step

	public float stepThreshold = 2;							// Treshold y-value to detect beginning of a step

	private List<float> leftXList = new List<float>();		// To store x-value when left step is detected
	private List<float> rightXList = new List<float>();		// To store x-value when right step is detected

    private StepManager stepManagerScript;

    // FSM states
    public StepManager.StepState _state = StepManager.StepState.Unknown;

	// Use this for initialization
	void Start () {
		rightXList.Add(1f);
		leftXList.Add(-1f);

        stepManagerScript = GameObject.FindGameObjectWithTag("Human").GetComponent<StepManager>();
    }
	
	// Update is called once per frame
	void Update () {
		Vector3 rotatedAccVector = OculusSensorData.rotatedAccVector;

		if(rotatedAccVector.y > stepThreshold) {	
			if(!stepActive) {
				xStepNow = rotatedAccVector.x;
				stepStateFSM();
				calculateThresholdFlag = true;
			}
			stepActive = true;
		}

		if(rotatedAccVector.y < -0.5f) {
			stepActive = false;
		}
		if(calculateThresholdFlag) {
			stepThreshold = calculateThreshold(rotatedAccVector.y);		
		}

		// Show or hide debug text
		if(Input.GetKeyDown(KeyCode.M)) {
			displayRightLeft.gameObject.SetActive(!displayRightLeft.gameObject.activeSelf);
		}

	}

 	private void stepStateFSM() {
		_state = checkSimilarity(xStepNow);

		// Display debug text
		if (displayRightLeft != null)
			displayRightLeft.text = _state.ToString ();

		// Correct step (play hard or smooth animation)
		if(_state.Equals(StepManager.StepState.Right)) {
            stepManagerScript.stepStates[0].state = StepManager.StepState.Right;
            stepManagerScript.stepChanged();
		} else if(_state.Equals(StepManager.StepState.Left)) {
            stepManagerScript.stepStates[0].state = StepManager.StepState.Left;
            stepManagerScript.stepChanged();
		}
	}

	public float averageLeft, averageRight;
	private StepManager.StepState checkSimilarity(float currentX) {
		averageLeft = ListUtilities.Instance.getAverageFromSortedList(leftXList);
		averageRight =  ListUtilities.Instance.getAverageFromSortedList(rightXList);

		float differenceToLeft = Mathf.Abs(currentX - averageLeft);
		float differenceToRight = Mathf.Abs(currentX - averageRight);

		if(differenceToLeft < differenceToRight) {
			leftXList.Add(currentX);
			return StepManager.StepState.Left;
		} else if(differenceToLeft > differenceToRight) {
			rightXList.Add(currentX);
			return StepManager.StepState.Right;
		} else {
			return StepManager.StepState.Unknown;
		}

	}
		
	private List<float> ThresholdList = new List<float>();	// List of threshold values
	private float max = 0;									// Maximum y value on first rise in step detection
	private float min = 0;									// Minimum y value on falling edge after max detection
	private float last = 0;									// Last y value
	public bool calculateThresholdFlag = false;

	private float calculateThreshold(float current_acc_y){

		float newThreshold;

		if(max == 0 || min == 0) {

			if(current_acc_y < last && max == 0) {
					max = last;
			}
			if(current_acc_y > last && max != 0) {
					min = last;		
			}

			last = current_acc_y;

		} else {
			newThreshold = (max + min) / 2;
			ThresholdList.Add(newThreshold);

			calculateThresholdFlag = false;
			max = 0;
			min = 0;
		}

		while(ThresholdList.Count > 32) {
			ThresholdList.RemoveAt(0);		
		}

		if(ThresholdList == null || ThresholdList.Count == 0)
			return 0;

		return ThresholdList.Average();
	}
}

