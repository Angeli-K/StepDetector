using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StepDetectorVelocity : MonoBehaviour {

	public TextMesh displayRightLeft;

	public StepManager.StepState _state = StepManager.StepState.Left;
	private StepManager.StepState _prevState = StepManager.StepState.Left;

	private Vector2 pospos = new Vector2(1, 1);
	private Vector2 posneg = new Vector2(1, -1);
	private Vector2 negpos = new Vector2(-1, 1);
	private	Vector2 negneg = new Vector2(-1, -1);
		
	private int validCounter = 0;
	public Vector2 signVelocity = Vector2.zero;
	private Vector2 lastSignVelocity = Vector2.zero;
	public Vector3 rotatedVelocity;

	private LinkedList<float> localMaxList_y = new LinkedList<float>();
	private LinkedList<float> localMinList_y = new LinkedList<float>();

	private LinkedList<float> localMaxList_x = new LinkedList<float>();
	private LinkedList<float> localMinList_x = new LinkedList<float>();

	private float negBorder_x = 0.0f;
	private float posBorder_x = 0.0f;
	public float min_x = 0.0f;
	public float max_x = 0.0f;


	public float negBorder_y = 0.0f;
	public float posBorder_y = 0.0f;
	public float min_y = 0.0f;
	public float max_y = 0.0f;
	
	public float fraction = 1.0f;

    private StepManager stepManagerScript;

    // Use this for initialization
    void Start () {
        stepManagerScript = GameObject.FindGameObjectWithTag("Human").GetComponent<StepManager>();
    }

	void Update () {
		//Vector3 rotatedVelocity = OculusSensorData.rotatedVelVector;

		Vector3 rawVelocity = OculusSensorData.rawVelVector;
		Quaternion angularVelocity = OculusSensorData.angularVelocity;

		//Compensate for the velocity from rotational head movements
		//The values are empirically measured
		rawVelocity.x = rawVelocity.x + 0.08f * angularVelocity.y;
		rawVelocity.y = rawVelocity.y - 0.15f * angularVelocity.x;

		rotatedVelocity = OculusSensorData.getRotatedVector (rawVelocity);

		// Split the data into two separate lists for each variable
		// and calculate the average and standard deviation of each list
		// to use as the threshold value to detect new steps
		if (rotatedVelocity.x > 0) {
			localMaxList_x.AddLast (rotatedVelocity.x);
		} else {
			localMinList_x.AddLast (rotatedVelocity.x);
		}
		while (localMaxList_x.Count > 150) {
			localMaxList_x.RemoveFirst();		
		}
		while (localMinList_x.Count > 150) {
			localMinList_x.RemoveFirst();		
		}
		if (localMaxList_x.Count > 0)
			max_x = localMaxList_x.Average() +  ListUtilities.Instance.calculateStandardDeviation (localMaxList_x.ToList ());
		if (localMinList_x.Count > 0)
			min_x = localMinList_x.Average() -  ListUtilities.Instance.calculateStandardDeviation (localMinList_x.ToList ());


		if (rotatedVelocity.y > 0) {
			localMaxList_y.AddLast (rotatedVelocity.y);
		} else {
			localMinList_y.AddLast (rotatedVelocity.y);
		}
		while (localMaxList_y.Count > 150) {
			localMaxList_y.RemoveFirst();		
		}
		while (localMinList_y.Count > 150) {
			localMinList_y.RemoveFirst();		
		}

		if (localMaxList_y.Count > 0)
			max_y = localMaxList_y.Average() + ListUtilities.Instance.calculateStandardDeviation (localMaxList_y.ToList ());
		if (localMinList_y.Count > 0)
			min_y = localMinList_y.Average() - ListUtilities.Instance.calculateStandardDeviation (localMinList_y.ToList ());


		negBorder_x = (fraction * min_x);
		posBorder_x = (fraction * max_x);
		if (rotatedVelocity.x < negBorder_x) {
			signVelocity.x = -1;
		} else if (rotatedVelocity.x > posBorder_x) {
			signVelocity.x = +1;
		} else {
			//signVelocity.x stays the same
		}

		negBorder_y = fraction * min_y;
		posBorder_y = fraction * max_y;
		if (rotatedVelocity.y < negBorder_y) {
			signVelocity.y = -1;
		} else if (rotatedVelocity.y > posBorder_y) {
			signVelocity.y = +1;
		} else {
			//signVelocity.y stays the same
		}
	
		if(!signVelocity.Equals(lastSignVelocity)) {
			stepStateFSM (signVelocity);
			lastSignVelocity = signVelocity;
		}

		// Show or hide debug text
		if(Input.GetKeyDown(KeyCode.M)) {
			displayRightLeft.gameObject.SetActive(!displayRightLeft.gameObject.activeSelf);
		}

	}
		
	/// <summary>
	/// Calculate the average maximum value to use as a threshold for step detection
	/// </summary>
	private void calculateMaximum(ref float maximum, ref float lastValue1, ref float lastValue2, float rotatedVelocityValue,
	                              ref LinkedList<float> localList, ref LinkedList<float> peakList){
		if (lastValue1 > lastValue2 && lastValue1 > rotatedVelocityValue && lastValue2 > 0 && lastValue1 > 0 && rotatedVelocityValue > 0) {
			localList.AddLast(lastValue1);
			getMaxPeakAverage(ref maximum, ref localList, ref peakList);
		}
		lastValue2 = lastValue1;
		lastValue1 = rotatedVelocityValue;
	}

	/// <summary>
	/// Calculate the average minimum value to use as a threshold for step detection
	/// </summary>
	private void calculateMinimum(ref float minimum, ref float lastValue1, ref float lastValue2, float rotatedVelocityValue,
	                              ref LinkedList<float> localList, ref LinkedList<float> peakList){
		if (lastValue1 < lastValue2 && lastValue1 < rotatedVelocityValue && lastValue2 < 0 && lastValue1 < 0 && rotatedVelocityValue < 0) {
			localList.AddLast(lastValue1);
			getMinPeakAverage(ref minimum, ref localList, ref peakList);
		}
		lastValue2 = lastValue1;
		lastValue1 = rotatedVelocityValue;
	}

	/// <summary>
	/// Find peaks: the element needs to be larger or equal to both the elements on its side.
	/// And return the average value of last 10 
	/// </summary>
	private void getMaxPeakAverage(ref float maximum, ref LinkedList<float> maxList, ref LinkedList<float> peakList) {
		if (maxList.Count > 3) {
			float x1 = maxList.ElementAt(0);
			float x2 = maxList.ElementAt(1);
			float x3 = maxList.ElementAt(2);

			if(x1 <= x2 && x2 >= x3) {
				peakList.AddLast(x2);
			}
			maxList.Remove(x1);
		}	

		if (peakList.Count > 60) {
			//maximum = peakList.Skip(peakList.Count - (peakList.Count - 10)).Average();
			List<float> list = peakList.Skip(peakList.Count - (peakList.Count - 60)).ToList();
			maximum = ListUtilities.Instance.calculateStandardDeviation(list);
		} else if (peakList != null && peakList.Count > 0) {
			//maximum = peakList.Average();
			maximum = ListUtilities.Instance.calculateStandardDeviation(peakList.ToList());
		}
	}

	/// <summary>
	/// Gets the minimum peak average.
	/// </summary>
	/// <returns>The minimum peak average.</returns>
	/// <param name="minList">Minimum list.</param>
	/// <param name="peakList">Peak list.</param>
	private void getMinPeakAverage(ref float minimum, ref LinkedList<float> minList, ref LinkedList<float> peakList) {
		if (minList.Count > 3) {
			float x1 = minList.ElementAt(0);
			float x2 = minList.ElementAt(1);
			float x3 = minList.ElementAt(2);
			
			if(x1 >= x2 && x2 <= x3) {
				peakList.AddLast(x2);
			}
			minList.Remove(x1);
		}
		
		if (peakList.Count > 60) {
			//minimum = peakList.Skip(peakList.Count - (peakList.Count - 10)).Average();
			List<float> list = peakList.Skip(peakList.Count - (peakList.Count - 60)).ToList();
			minimum = ListUtilities.Instance.calculateStandardDeviation(list);
		} else if (peakList != null && peakList.Count > 0) {
			//minimum = peakList.Average();
			minimum = ListUtilities.Instance.calculateStandardDeviation(peakList.ToList());
		}
		
	}

	private void stepStateFSM(Vector2 signVelocity) {
		switch(_state) {
		case StepManager.StepState.Left:
			if (signVelocity.Equals (negneg)) {
				validCounter++;
			} else {
				validCounter = 0;
			}
			_prevState = StepManager.StepState.Left;

			break;
		
		case StepManager.StepState.SwingOne:
//			if (signVelocity.Equals (negpos) && _prevState.Equals (StepManager.StepState.SwingTwo)) {
//				validCounter++;
//			} else if (signVelocity.Equals (posneg) && _prevState.Equals (StepManager.StepState.Left)) {
//				validCounter++;
//			} else {
//				validCounter = 0;
//			}
			_prevState = StepManager.StepState.SwingOne;

			break;
		
		case StepManager.StepState.SwingTwo:
//			if (signVelocity.Equals (pospos) && _prevState.Equals (StepManager.StepState.SwingOne)) {
//				validCounter++;
//			} else if (signVelocity.Equals (negneg) && _prevState.Equals (StepManager.StepState.Right)) {
//				validCounter++;
//			} else {
//				validCounter = 0;
//			}
			_prevState = StepManager.StepState.SwingTwo;

			break;
		
		case StepManager.StepState.Right:
			if (signVelocity.Equals (posneg)) {
				validCounter++;
			} else {
				validCounter = 0;
			}
			_prevState = StepManager.StepState.Right;

			break;
		}

		if (signVelocity.Equals (negpos)) {
			_state = StepManager.StepState.Left;
		} else if (signVelocity.Equals (negneg)) {
			_state = StepManager.StepState.SwingOne;
		} else if (signVelocity.Equals (posneg)) {
			_state = StepManager.StepState.SwingTwo;
		} else if (signVelocity.Equals (pospos)) {
			_state = StepManager.StepState.Right;
		}

		string step = "/";
		if (validCounter >= 4) {
			if (_state.Equals(StepManager.StepState.Left)){
				step = "Left " + validCounter;
                stepManagerScript.stepStates[1].state = StepManager.StepState.Left;
                stepManagerScript.stepChanged();
			} /*else if (_state.Equals(StepManager.StepState.SwingOne)){
				step = "   " + validCounter;
			} else if (_state.Equals(StepManager.StepState.SwingTwo)){
				step = "   " + validCounter;
			} */else if (_state.Equals(StepManager.StepState.Right)){
				step = "Right " + validCounter;

                stepManagerScript.stepStates[1].state = StepManager.StepState.Right;
                stepManagerScript.stepChanged();
			}

		}

		// Display debug text
		if (displayRightLeft != null) {
			displayRightLeft.text = step;
		}

	}

}
