using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class StepSpeed{
	private float lastStepTime = 1;
	private float averageTime = 0;
	public float averageStepsPerSecond = 0;
	private List<float> averageTimeList;					// To calulate the average time that is needed for one step

	private static StepSpeed instance = null;
	public static StepSpeed Instance {
		get {
			if(instance == null) {
				instance = new StepSpeed();
				instance.init();
			}
			return instance;
		}
	}

	private void init() {
		averageTimeList = new List<float>();
		lastStepTime = 0;
	}


	/// <summary>
	/// Calculates the average time that was needed for one step.
	/// Should be called every time a foot strikes
	/// </summary>
	public void calcAverageTime() {
		float currStepTime = Time.time;
		
		float currDuration = currStepTime - lastStepTime;
		
		if (currDuration < 5 && currDuration > 0.3f) {
			averageTimeList.Add(currDuration);
			averageTime =  ListUtilities.Instance.getAverageFromSortedList(averageTimeList);
			lastStepTime = currStepTime;
			if(averageTime != 0) {
				averageStepsPerSecond = 1/(2*averageTime);
			}
		}
		while (averageTimeList.Count > 20) {
			averageTimeList.RemoveAt(0);		
		}
	}

}
