using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class JordanRotator : MonoBehaviour {

	public static JordanRotator Instance;

	public bool Enabled = false;
	public bool InitialCaptured = false;
	Vector3 InitialPos;
	public float YAmount = 0f;
	public float XAmount = 0f;
	public float XSpeed = 15f;
	public float YSpeed = 3.5f;

	public float PosAmount = 1f;

	Transform UserTransform;


	public bool Reset = false;

	void Start () {
		Instance = this;
		// UserTransform = GameObject.Find("Users").transform;
	}
	
	// Update is called once per frame
	void Update () {
		if (Enabled) {
			// if (!InitialCaptured) {
			// 	InitialPos = UserTransform.position;
			// 	InitialCaptured = true;
			// }
			var xRotation = (Mathf.Sin(Time.time / XSpeed) * XAmount) * PosAmount;
			var yRotation = (Mathf.Sin(Time.time / YSpeed) * (YAmount / 30)) * PosAmount;

			// transform.rotation = Quaternion.Euler(XAmount, yRotation, 0);
			UserMeshVisualizer.Instance.NewRotationSlider = yRotation;

			// var xPos = InitialPos.x + (Mathf.Sin(Time.time / XSpeed) * PosAmount);
			// UserTransform.position = new Vector3(xPos, InitialPos.y, InitialPos.z);
		}
		if (Reset) {
			// UserTransform.rotation = Quaternion.Euler(0, 0, 0);
			// UserTransform.position = InitialPos;
			UserMeshVisualizer.Instance.NewRotationSlider = 0;
			UserMeshVisualizer.Instance.DoRotate = false;
			UserMeshVisualizer.Instance.StopRotateAnimation = true;
			Reset = false;
		}
	}
}
