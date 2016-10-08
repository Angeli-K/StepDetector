using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System;
using System.Linq;

public class RecordData : MonoBehaviour {

	public bool recordFPS;
	public bool recordAcc;
	public bool recordVelocity;
	public bool recordPosition;
	public bool recordStepDetector;
	public bool recordReplay;

    public bool recordFilt;

    // Manual step markers (1: left step, 4: right step)
    private int rightMinusLeft = 0;

	// Variables to record accelerometer data
	private StringBuilder sbAcc;
	private StepDetectorAcceleration stepDetectorScript;

	// Variables to record velocity data
	private StringBuilder sbVelocity;
	private StepDetectorVelocity stepDetectorVelocityScript;

	// Variables to record position data
	private StringBuilder sbPos;

	// Variables to record Step Detector data
	private StringBuilder sbStep;

	// Variables to record Replay data
	private StringBuilder sbReplay;

    private StringBuilder sbFilt;
    private LowPassFilter lowPassFilterScript;

    // Time String
    private string TimeAndDate;

    private StepManager stepManagerScript;

    private string path = "Measurements/";
    // Use this for initialization
    void Start () {
		//Set time and date string
		TimeAndDate = System.DateTime.Now.ToString ("yyyy-MM-dd_HH-mm");
		Debug.Log (TimeAndDate);

        if (!Directory.Exists(path)) {
            Directory.CreateDirectory(path);
        }

		if(recordAcc) {
			stepDetectorScript = GameObject.FindGameObjectWithTag("Human").GetComponent<StepDetectorAcceleration>();
		}

		if(recordVelocity) {
			stepDetectorVelocityScript = GameObject.FindGameObjectWithTag("Human").GetComponent<StepDetectorVelocity>();
		}

        if (recordFilt) {
            lowPassFilterScript = GameObject.FindGameObjectWithTag("Human").GetComponent<LowPassFilter>();
        }

        stepManagerScript = GameObject.FindGameObjectWithTag("Human").GetComponent<StepManager>();

    }
	
	// Update is called once per frame
	void Update () {

        //int rightMinusLeft = Convert.ToInt16(Input.GetKeyDown(KeyCode.RightArrow)) - Convert.ToInt16(Input.GetKeyDown(KeyCode.LeftArrow));
        
        if (Input.GetKeyDown(KeyCode.RightArrow)){
            rightMinusLeft = 4;
        }
        if (Input.GetKeyDown(KeyCode.LeftArrow)) {
            rightMinusLeft = 1;
        }

		if(recordAcc) {
			Vector3 rawAccVector = OculusSensorData.rawAccVector;
			Vector3 rotatedAccVector = OculusSensorData.rotatedAccVector;
			saveAccDataToStringBuilder(rawAccVector, rotatedAccVector, rightMinusLeft);
		}

		if(recordVelocity) {
			Vector3 newVelocity = stepDetectorVelocityScript.rotatedVelocity;
			Vector3 rawVelocity = OculusSensorData.rawVelVector;
			Vector3 rotatedVelocityVector = OculusSensorData.rotatedVelVector;
			Quaternion angularVelocityQuaternion = OculusSensorData.angularVelocity; 
			Vector3 signVelocity = stepDetectorVelocityScript.signVelocity;
			float posBorder_y = stepDetectorVelocityScript.posBorder_y;
			float negBorder_y = stepDetectorVelocityScript.negBorder_y;
			float min_x = stepDetectorVelocityScript.min_x;
			float max_x = stepDetectorVelocityScript.max_x;
			float min_y = stepDetectorVelocityScript.min_y;
			float max_y = stepDetectorVelocityScript.max_y;
			saveVelDataToStringBuilder(rawVelocity, rotatedVelocityVector, newVelocity, angularVelocityQuaternion, signVelocity,
			                           min_x, max_x, min_y, max_y, 
			                           (((int) stepDetectorVelocityScript._state)/50f), 
			                           posBorder_y, negBorder_y,
			                           rightMinusLeft);
		}

		if(recordPosition) {
			Quaternion orientation = OVRManager.tracker.GetPose().orientation;
			savePosDataToStringBuilder(OculusSensorData.posVector, orientation, StepDetectorPosition.state, rightMinusLeft);
		}

		if(recordStepDetector) {
			StepItem[] steps = stepManagerScript.stepStates;
			saveStepDataToStringBuilder(rightMinusLeft, (int)steps[0].state, (int)steps[1].state, (int)steps[2].state, (int)steps[3].state,
                                        StepManager.voteLeft, StepManager.voteRight, 
			                            Convert.ToInt16(StepManager.animationSync), Convert.ToInt16(StepManager.calcSpeedTime));
			StepManager.calcSpeedTime = false;
			StepManager.animationSync = false;
		}
		if (recordReplay) {
			Vector3 position = OculusSensorData.posVector;
			Quaternion orientation = OVRManager.tracker.GetPose().orientation;
			Vector3 acceleration = OculusSensorData.rawAccVector;
			Vector3 velocity = OculusSensorData.rawVelVector;
			saveReplayDataToStringBuilder(position, orientation, acceleration, velocity,rightMinusLeft);
		}

        if (recordFilt) {
            List<float> filt_ax = lowPassFilterScript.filt_ax;
            List<float> filt_ay = lowPassFilterScript.filt_ay;
            if (filt_ax.Count > 0 && filt_ay.Count > 0) {
                float filtx = filt_ax.Last();
                float filty = filt_ay.Last();
                Vector3 rotatedAccVector = OculusSensorData.rotatedAccVector;
                saveFilteredData(rotatedAccVector.x, rotatedAccVector.y, filtx, filty);
            }
        }
	}

	void OnDisable() {
//#if !UNITY_EDITOR
		Debug.Log("Save to csv");
		string filePath;

		if(recordAcc) {
			// Save acceleration data
			filePath = path + "accData_" + TimeAndDate + ".csv";
			saveToCSV(filePath, sbAcc);
		}

		if(recordVelocity) {
			filePath = path + "velocityData_" + TimeAndDate + ".csv";
			saveToCSV(filePath, sbVelocity);
		}

		if(recordPosition){
			filePath = path + "positionData_" + TimeAndDate +  ".csv";
			saveToCSV(filePath, sbPos);
		}

		if(recordStepDetector){
			filePath = path + "stepDetector_" + TimeAndDate + ".csv";
			saveToCSV(filePath, sbStep);
		}
		if(recordReplay){
			filePath = path + "replay_"+TimeAndDate+".csv";
			saveToCSV(filePath, sbReplay);
		}

        if(recordFilt) {
            filePath = path + "filtered_ax_"+TimeAndDate+".csv";
            saveToCSV(filePath, sbFilt);
        }
//#endif
    }

	private void saveAccDataToStringBuilder(Vector3 rawAccVector, Vector3 rotatedAccVector, int rightMinusLeft) {

		int stepState = (int)stepDetectorScript._state;

		string line = Time.time + ";" + rawAccVector.x + ";" + rawAccVector.y + ";" + rawAccVector.z + ";" +
						rotatedAccVector.x + ";" + rotatedAccVector.y + ";" + rotatedAccVector.z + ";" + 
						stepState + ";" + stepDetectorScript.stepThreshold + ";" + 
						stepDetectorScript.averageLeft + ";" + stepDetectorScript.averageRight + ";" +
						rightMinusLeft;

		if(sbAcc == null) {
			sbAcc = new StringBuilder();
			sbAcc.AppendLine("time;raw_x_Acc;raw_y_Acc;raw_z_Acc;" +
							 "rotated_x_Acc;rotated_y_Acc;rotated_z_Acc;" +
							 "stepState;Threshold;" +
							 "averageLeft;averageRight;" +
			   				 "rightMinusLeft");
		}

		sbAcc.AppendLine(line);
	}

	private void saveVelDataToStringBuilder(Vector3 rawVelVector, Vector3 rotatedVelVector, Vector3 compVelocity, Quaternion angularVelocity, Vector3 signVelocity, 
	                                        float min_x, float max_x, float min_y, float max_y, 
	                                        float _state, float posBorder_y, float negBorder_y,
	                                        int rightMinusLeft) {
		string line = Time.time + ";" + rawVelVector.x + ";" + rawVelVector.y + ";" + rawVelVector.z + ";" +
				rotatedVelVector.x + ";" + rotatedVelVector.y + ";" + rotatedVelVector.z + ";" +
				compVelocity.x + ";" + compVelocity.y + ";" + compVelocity.z + ";" +
				angularVelocity.x + ";" + angularVelocity.y + ";" + angularVelocity.z + ";" +
				signVelocity.x + ";" + signVelocity.y + ";" + signVelocity.z + ";" +
				min_x + ";" + max_x + ";" + min_y + ";" + max_y + ";" + _state + ";" + posBorder_y + ";" + negBorder_y + ";" +
				rightMinusLeft;

		if(sbVelocity == null) {
			sbVelocity = new StringBuilder();
			sbVelocity.AppendLine("Time;x_vel_raw; y_vel_raw; z_vel_raw;" +
								  "x_vel_rot; y_vel_rot; z_vel_rot;" +
			                      "x_vel_comp; y_vel_comp; z_vel_comp;" +
			                      "x_angVel; y_angVel; z_angVel; " +
			                      "x_sign_vel;y_sign_vel;z_sign_vel;" +
			                      "minX;maxX;minY;maxY;" +
			                      "stepstate; posBorder_y; negBorder_y;" +
			                      "rightMinusleft");
		}

		sbVelocity.AppendLine(line);
	}

	private void savePosDataToStringBuilder(Vector3 posData, Quaternion orientation,int state, int rightMinusLeft) {
				
		string line = Time.time +";"+
						posData.x + ";" + posData.y + ";" + posData.z + ";" +
						orientation.x + ";" + orientation.y + ";" + orientation.z + ";" + orientation.w + ";" +
						state + ";" + 
						rightMinusLeft;

		if(sbPos == null) {
			sbPos = new StringBuilder();
			sbPos.AppendLine("time;"+
			                 "x_pos;y_pos;z_pos;" +
			                 "quat_x; quat_y; quat_z; quat_w;"+
			                 "stepState;" +
			                 "rightMinusLeft");
		}

		sbPos.AppendLine(line);
	}

	private void saveStepDataToStringBuilder(int rightMinusLeft, int acc, int vel, int pos, int low,
	                                         float voteLeft, float voteRight,
	                                         int animationSync, int calculateStepTime) {
		string line = Time.time + ";" + rightMinusLeft + ";" +
				acc + ";" + vel + ";" + pos + ";" + low + ";" +
				voteLeft + ";" + voteRight + ";" +
				animationSync + ";" + calculateStepTime;

		if (sbStep == null) {
			sbStep = new StringBuilder();
			sbStep.AppendLine("time;rightMinusLeft;accState;velState;posState;lowState;voteLeft;voteRight;" +
			                  "AnimationSync;calcStepTime");
		}

		sbStep.AppendLine(line);
	}

	private void saveReplayDataToStringBuilder(Vector3 position, Quaternion orientation, Vector3 acceleration, Vector3 velocity, int rightMinusLeft) {
		string line = Time.time + ";" + 
						position.x + ";" + position.y + ";" + position.z + ";" +
						orientation.x + ";" + orientation.y + ";" + orientation.z + ";" + orientation.w + ";" +
						acceleration.x + ";" + acceleration.y + ";" + acceleration.z + ";" +
				velocity.x + ";" + velocity.y + ";" + velocity.z + ";" +rightMinusLeft;
		
		if (sbReplay == null) {
			sbReplay = new StringBuilder();
			sbReplay.AppendLine("time;pos_x;pos_y;pos_z;quat_x;quat_y;quat_z;quat_w;acc_x;acc_y;acc_z;vel_x;vel_y;vel_z;rightMinusLeft");
		}
		
		sbReplay.AppendLine(line);
	}

    private void saveFilteredData(float original_x, float original_y, float filt_x, float filt_y) {
        float next_time = 0;

        string line = Time.time + ";" + original_x + ";" + original_y + ";" + filt_x + ";" + filt_y + ";" + next_time;

        if (sbFilt == null) {
            sbFilt = new StringBuilder();
            sbFilt.AppendLine("time;original_x;original_y;filt_x;filt_y;next_time");
        }

        sbFilt.AppendLine(line);
    }

    private void saveToCSV(string filePath, StringBuilder sb) {
        if (sb != null) {
            File.WriteAllText(filePath, sb.ToString());
        }
	}

}
