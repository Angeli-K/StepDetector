using UnityEngine;
using System.Collections;

public class StepManager : MonoBehaviour {

	public StepState _state = StepState.Unknown;
	public enum StepState {Unknown, Left, SwingOne, SwingTwo, Right}

	public StepItem[] stepStates = new StepItem[4];

	// Vote between different step detectors
	private float accWeight = 	  0.1f;
	private float velWeight =	  0.1f;
	private float posWeight =	  0.7f;	// Position based step-detector performs best
    private float lowPassWeight = 0.1f;

    public static float voteLeft;
	public static float voteRight;
	private static float lastValue;
	private static StepState lastSync; 

	// Debug variables
	public static bool animationSync = false;
	public static bool calcSpeedTime = false;

    public TextMesh displayRightLeft;

    // Use this for initialization
    void Start () {
		stepStates[0] = new StepItem {state = StepState.Unknown, weight = accWeight};
		stepStates[1] = new StepItem {state = StepState.Unknown, weight = velWeight};
		stepStates[2] = new StepItem {state = StepState.Unknown, weight = posWeight};
        stepStates[3] = new StepItem {state = StepState.Unknown, weight = lowPassWeight};
    }
	
	// Update is called once per frame
	void Update () {

        // Show or hide debug text
        if (Input.GetKeyDown(KeyCode.M)) {
            displayRightLeft.gameObject.SetActive(!displayRightLeft.gameObject.activeSelf);
        }

    }

	public void stepChanged() {
		lastValue = voteLeft;
		voteLeft = 0;
		voteRight = 0;
//		animationSync = false;
//		calcSpeedTime = false;

		for(int i = 0; i < stepStates.Length; i++) {
			if(stepStates[i].state.Equals(StepState.Left)) {
				voteLeft += stepStates[i].weight;
			} else if(stepStates[i].state.Equals (StepState.Right)) {
				voteRight += stepStates[i].weight;
			}
		}

		if(voteRight >= 0.7f && voteRight > voteLeft && lastSync != StepState.Right) {
            AnimationSync.onRightStep();
			animationSync = true;
			lastSync = StepState.Right;

            _state = StepState.Right;

        } else if(voteLeft >= 0.7f && voteLeft > voteRight && lastSync != StepState.Left) {
            AnimationSync.onLeftStep();
            animationSync = true;
			lastSync = StepState.Left;

            _state = StepState.Left;
        }

		if (voteLeft != lastValue) {
			StepSpeed.Instance.calcAverageTime();
			calcSpeedTime = true;
		}

        // Display debug text
        if (displayRightLeft != null) {
            displayRightLeft.text = _state.ToString();
        }

    }

}

public class StepItem {
	public float weight { get; set; }
	public StepManager.StepState state { get; set; }
}
