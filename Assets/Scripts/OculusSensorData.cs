using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.VR;

public class OculusSensorData : MonoBehaviour {

	// Dynamic vectors
	public static Vector3 rawAccVector = Vector3.zero;
	public static Vector3 rotatedAccVector = Vector3.zero;
	
	public static Vector3 rawVelVector = Vector3.zero;
	public static Vector3 rotatedVelVector = Vector3.zero;

	public static Vector3 posVector = Vector3.zero;

	public static Quaternion angularVelocity = Quaternion.identity;

	public static Quaternion orientationQuat = Quaternion.identity;

	private static Matrix4x4 invertedRotation;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		//TODO: maybe transition to fixedUpdate?
		getOrientation ();
		invertedRotation = calculateInverseRotation ();
		getRotatedAccData (invertedRotation);
		getRotatedVelData (invertedRotation);
		getPosData ();
		getAngularVelocity ();
	}

	private static void getOrientation(){
        //orientationQuat = OVRManager.display.GetEyePose(OVREye.Left).orientation;
        orientationQuat = InputTracking.GetLocalRotation(VRNode.Head);
    }

	/// <summary>
	/// Calculates the inverse Rotation Matrix to convert from Oculus to World coordinates
	/// </summary>
	private static Matrix4x4 calculateInverseRotation(){
		Quaternion inverseRotation = Quaternion.Inverse(orientationQuat);
		Matrix4x4 inverseRotationMatrix = Matrix4x4.TRS (Vector3.zero, inverseRotation, Vector3.one);
		
		return inverseRotationMatrix;
	}

	/// <summary>
	/// Gets rotated accelerometer data.
	/// </summary>
	private static void getRotatedAccData(Matrix4x4 inverseRotationMatrix) {
		rawAccVector = OVRManager.display.acceleration;

		rotatedAccVector = inverseRotationMatrix.MultiplyVector(rawAccVector);
	}
	
	/// <summary>
	/// Gets rotated velocity data.
	/// </summary>
	private static void getRotatedVelData(Matrix4x4 inverseRotationMatrix) {
		rawVelVector = OVRManager.display.velocity;
		
		rotatedVelVector = inverseRotationMatrix.MultiplyVector(rawVelVector);
	}

	/// <summary>
	/// Gets rotated vector.
	/// </summary>
	public static Vector3 getRotatedVector(Vector3 vector) {
		return invertedRotation.MultiplyVector(vector);
	}
	
	/// <summary>
	/// Gets oculus position data.
	/// </summary>
	private static void getPosData(){
        //posVector = OVRManager.display.GetEyePose (OVREye.Left).position;
        posVector = InputTracking.GetLocalPosition(VRNode.Head); InputTracking.GetLocalPosition(VRNode.Head);
    }

    /// <summary>
    /// Gets the angular velocity of the head.
    /// </summary>
    private static void getAngularVelocity(){
		angularVelocity = OVRManager.display.angularVelocity;
	}	

}