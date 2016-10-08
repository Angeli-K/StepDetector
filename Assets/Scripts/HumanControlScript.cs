using UnityEngine;
using System.Collections;

public class HumanControlScript : MonoBehaviour {

	public enum StepType {
		LeftStep,
		RightStep
	}

	private Animator anim;					// A reference to the animator on the character

	public float animSpeed;					// A public setting for overall animator animation speed

	private int originalXPosition = 250;

	private StepType lastStep;				// Last step (right or left)

	public bool testAnimation;
	private bool move;

	public float minZ = 0;					// starting position
	public float maxZ = 0;					// ending position

	void Start () {
		// initialising reference variables
		anim = GetComponent<Animator> ();

		// Randomized starting position between 100 and 990, transforming it to minZ and defining maxZ as ending position
		transform.localPosition = new Vector3 (originalXPosition, transform.localPosition.y, Random.Range (700, 990));
		minZ = transform.localPosition.z;
		maxZ = minZ + 1000;
	}

	void FixedUpdate () {
		if (transform.localPosition.z > maxZ)
			Application.Quit ();                     // Ends the Application automatically

		// Start walking
		if (testAnimation) {
			// Start walking when space is pressed
			if (Input.GetKeyDown ("space")) {
				anim.enabled = true;
				anim.speed = animSpeed;				// Set the speed of our animator to the public variable 'animSpeed'
				move = true;
			}

			if (move) {
				transform.Translate (new Vector3 (0, 0, 3) * Time.deltaTime * animSpeed);
			}
		} else {
			// Get animation speed per second
			anim.speed = StepSpeed.Instance.averageStepsPerSecond;
			transform.Translate (new Vector3 (0, 0, 3) * Time.deltaTime * anim.speed);
		}

		// Human should walk straight forward
		if (transform.localPosition.x != originalXPosition) {
			transform.localPosition = new Vector3 (originalXPosition, transform.localPosition.y, transform.localPosition.z);
		}
	}

	/// <summary>
	/// Interrupt the animation to correct the step.
	/// </summary>
	/// <param name="rightOrLeft">Right or left step.</param>
	public void playHardAnimation (StepType rightOrLeft) {
		anim.Play (rightOrLeft.ToString (), -1, 0);
	}

	/// <summary>
	/// Try to interpolate between left and right step.
	/// </summary>
	/// <param name="rightOrLeft">Right or left step.</param>
	public void playSmoothAnimation (StepType rightOrLeft) {
		if (lastStep.Equals (rightOrLeft)) {
			anim.SetTrigger ("CorrectStep");
		} else {
			if (rightOrLeft.Equals (StepType.LeftStep)) {
				anim.SetTrigger (StepType.LeftStep.ToString ());
				anim.ResetTrigger (StepType.RightStep.ToString ());
			} else if (rightOrLeft.Equals (StepType.RightStep)) {
				anim.SetTrigger (StepType.RightStep.ToString ());
				anim.ResetTrigger (StepType.LeftStep.ToString ());
			}

		}

		// Save last step (left or right)
		lastStep = rightOrLeft;
	}

}