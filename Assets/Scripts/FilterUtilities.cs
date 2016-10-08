using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class FilterUtilities {

    public List<float> maxList;
    public List<float> minList;
    public List<float> risingZeroList;
    public List<float> fallingZeroList;

    private float lastValue1 = 0.0f;
    private float lastValue2 = 0.0f;
    private float lastTime = 0.0f;
    //private float nextTime = 0.0f;

    public bool max;

    public int maxlength = 5;

    public enum LastFound { Unknown, Minimum, Maximum, RisingZero, FallingZero }
    public enum Signal { Acc, Vel, Pos }
    public LastFound _state = LastFound.Unknown;

    //private float sigma, breadth;

    public void init() {
        maxList = new List<float>();
        minList = new List<float>();
        risingZeroList = new List<float>();
        fallingZeroList = new List<float>();

        max = false;
    }

    public void setValue(float currentValue, Signal signal) {
        float time = Time.time;
        //float sigma = LowPassFilter.sigma;
        //float sigma_breadth = LowPassFilter.sigma_breadth;

        //float window_delay = sigma * sigma_breadth / 2;

        // Check if maximum
        if (lastValue1 > lastValue2 && lastValue1 > currentValue) { // && lastValue2 > 0 && lastValue1 > 0 && currentValue > 0
            if (!signal.Equals(Signal.Pos)) {
                if (_state.Equals(LastFound.Unknown) || _state.Equals(LastFound.RisingZero)) {
                    maxList.Add(lastTime);
                    _state = LastFound.Maximum;

                    //nextTime = Time.time + calculateAverageTime(maxList) - window_delay;

                    max = true;
                }

            } else if(_state.Equals(LastFound.Unknown) || _state.Equals(LastFound.Minimum)) {
                maxList.Add(lastTime);
                _state = LastFound.Maximum;

                //nextTime = Time.time + calculateAverageTime(maxList) - window_delay;
            }
        }

        // Check if minimum
        if (lastValue2 > lastValue1 && lastValue1 < currentValue) {
            if (!signal.Equals(Signal.Pos)) {
                if (_state.Equals(LastFound.Unknown) || _state.Equals(LastFound.FallingZero)) {
                    minList.Add(lastTime);
                    _state = LastFound.Minimum;

                    //nextTime = Time.time + calculateAverageTime(minList) - window_delay;
                }

            } else if (_state.Equals(LastFound.Unknown) || _state.Equals(LastFound.Maximum)) {
                minList.Add(lastTime);
                _state = LastFound.Minimum;

                //nextTime = Time.time + calculateAverageTime(minList) - window_delay;
            }
        }

        // Check if rising zero
        if (lastValue2 < 0 && lastValue1 > 0) {
            if (!signal.Equals(Signal.Pos)) {
                if (_state.Equals(LastFound.Unknown) || _state.Equals(LastFound.Minimum)) {
                    risingZeroList.Add(lastTime);
                    _state = LastFound.RisingZero;

                    //nextTime = Time.time + calculateAverageTime(risingZeroList) - window_delay;
                }
            }
        }

        // Check if falling zero
        if (lastValue2 > 0 && currentValue < 0) {
            if (!signal.Equals(Signal.Pos)) {
                if (_state.Equals(LastFound.Unknown) || _state.Equals(LastFound.Maximum)) {
                    fallingZeroList.Add(lastTime);
                    _state = LastFound.FallingZero;

                    //nextTime = Time.time + calculateAverageTime(fallingZeroList) - window_delay;
                }
            }
        }

        clipList(maxList);
        clipList(minList);
        clipList(risingZeroList);
        clipList(fallingZeroList);

        lastTime = time;
        lastValue2 = lastValue1;
        lastValue1 = currentValue;
    }

    private void clipList(List<float> list) {
        while (list.Count > maxlength) {
            list.RemoveAt(0);
        }
    }

    public float calculateAverageTime(List<float> list) {
        float delta = 0.0f;
        for (int i = list.Count - 1; i > 0; i--) {
            delta += list.ElementAt(i) - list.ElementAt(i - 1);
        }
        delta = delta / (list.Count - 1);
        return delta;
    }
}
