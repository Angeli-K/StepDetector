using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class ListUtilities{
	private static ListUtilities instance = null;
	public static ListUtilities Instance {
		get {
			if(instance == null) {
				instance = new ListUtilities();
			}
			return instance;
		}
	}

	/// <summary>
	/// Gets the average from sorted list.
	/// </summary>
	/// <returns>The average from sorted list.</returns>
	/// <param name="floatList">Float list.</param>
	public float getAverageFromSortedList(List<float> floatList) {
		if(floatList == null || floatList.Count == 0)
			return 0;
		
		// Sort the list
		List<float> sortedFloatList = new List<float>(floatList.OrderBy(x => x));
		
		// Remove outliers
		int listLength =  sortedFloatList.Count / 4;
		for(int i = 0; i < listLength; i++) {
			sortedFloatList.RemoveAt(0);							//remove first element
			sortedFloatList.RemoveAt(sortedFloatList.Count - 1);	//remove last element
		}
		
		float average = sortedFloatList.Average();
		return average;
	}

	/// <summary>
	/// Calculates a standard deviation.
	/// </summary>
	/// <returns>The standard deviation.</returns>
	/// <param name="peakList">Peak list.</param>
	public float calculateStandardDeviation(List<float> peakList) {
		float average = peakList.Average ();
		float sumOfSqueresOfDifferences = peakList.Select(val => (val - average) * (val - average)).Sum();
		float sd = Mathf.Sqrt (sumOfSqueresOfDifferences / peakList.Count);
		
		return sd;
	}
}
