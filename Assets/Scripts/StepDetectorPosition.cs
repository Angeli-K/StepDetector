using UnityEngine;
using System.Collections;

public class StepDetectorPosition : MonoBehaviour {

	public TextMesh displayRightLeft;

	private float lastXValue1 = 0;
	private float lastXValue2 = 0;
	private float lastYValue1 = 0;
	private float lastYValue2 = 0;

	private bool isXMaximum = false;		// if true -> maximum, if false -> minimum

	private string step = "L - R";
	public static int state = 0;

    private StepManager stepManagerScript;

    // Use this for initialization
    void Start () {
        stepManagerScript = GameObject.FindGameObjectWithTag("Human").GetComponent<StepManager>();
    }
	
	// Update is called once per frame
	void Update () {
		Vector3 position = OculusSensorData.posVector;
		float y_value = position.y;
		float x_value = position.x;

		// Detect if the x value is a local maximum or minimum
		if(detectMaximum (x_value, lastXValue1, lastXValue2)) {
			isXMaximum = true;
		} else if(detectMinimum(x_value, lastXValue1, lastXValue2)) {
			isXMaximum = false;
		}

		// Detect if the y value is a local maximum or minimum
		if (detectMinimum(y_value, lastYValue1, lastYValue2)) {
			if(isXMaximum) {
				// When last x-peak was a local maximum
				step = "L";
				state = -1;

                stepManagerScript.stepStates[2].state = StepManager.StepState.Left;
                stepManagerScript.stepChanged();
			} else {
				// When last x-peak was a local minimum
				step = "R";
				state = +1;

                stepManagerScript.stepStates[2].state = StepManager.StepState.Right;
                stepManagerScript.stepChanged();
			}
		}

		lastXValue2 = lastXValue1;
		lastXValue1 = x_value;

		lastYValue2 = lastYValue1;
		lastYValue1 = y_value;


		// Display debug text
		if(displayRightLeft != null) {
			displayRightLeft.text = step;
		}

		// Show or hide debug text
		if(Input.GetKeyDown(KeyCode.M)) {
			displayRightLeft.gameObject.SetActive(!displayRightLeft.gameObject.activeSelf);
		}
	}

	/// <summary>
	/// Detect a peak (positive maximum).
	/// </summary>
	private bool detectMaximum(float currentValue, float lastValue1, float lastValue2) {
		bool result = false;

		if (lastValue1 > lastValue2 && lastValue1 > currentValue) {
			result = true;
		}

		return result;
	}
	
	/// <summary>
	/// Detect a minimum.
	/// </summary>
	private bool detectMinimum(float currentValue, float lastValue1, float lastValue2){
		bool result = false;

		if (lastValue1 < lastValue2 && lastValue1 < currentValue) {
			result = true;
		}

		return result;
	}

}
