using UnityEngine;
using System.Collections;

public class AnimationSync : MonoBehaviour {

    private static StepSoundL SkriptL;
    private static StepSoundR SkriptR;

    // Animation types
    public static AnimationType _animType = AnimationType.SmoothAnimation;
	public enum AnimationType {
		SmoothAnimation,
		HardAnimation
	}
	private static HumanControlScript humanControlScript;

	private void syncAnimation(string stepType){
		Debug.Log (stepType);
		if (stepType == "Left") {
			onLeftStep ();		
		} else if (stepType == "Right") {
			onRightStep();		
		}
	}
		
	// Use this for initialization
	void Start () {
		humanControlScript = this.GetComponent<HumanControlScript>();
        SkriptL = GameObject.FindGameObjectWithTag("FootL").GetComponent<StepSoundL>();
        SkriptR = GameObject.FindGameObjectWithTag("FootR").GetComponent<StepSoundR>();
    }

    // Update is called once per frame
    void Update () {
	
	}

	/// <summary>
	/// Play animation for the right step.
	/// </summary>
	public static void onRightStep() {
        if (_animType.Equals(AnimationType.SmoothAnimation)) {
			humanControlScript.playSmoothAnimation(HumanControlScript.StepType.RightStep);
		} else {
			humanControlScript.playHardAnimation(HumanControlScript.StepType.RightStep);
		}
	}
	
	/// <summary>
	/// Play animation for the left step.
	/// </summary>
	public static void onLeftStep() {
        if (_animType.Equals(AnimationType.SmoothAnimation)) {
			humanControlScript.playSmoothAnimation(HumanControlScript.StepType.LeftStep);
		} else {
			humanControlScript.playHardAnimation(HumanControlScript.StepType.LeftStep);
		}
	}

	/// <summary>
	/// Play sound on the right heel strike.
	/// </summary>
    public void RightHeelStrike() {
        Debug.Log("RightHeelStrike");
		SkriptR.playSoundR();
    }

	/// <summary>
	/// Play sound on the left heel strike.
	/// </summary>
    public void LeftHeelStrike() {
        Debug.Log("LeftHeelStrike");
		SkriptL.playSoundL();
    }
}
