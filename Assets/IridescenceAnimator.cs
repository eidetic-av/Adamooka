using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IridescenceAnimator : MonoBehaviour {

	public bool TriggerBangColorOffset = false;
	public AnimationCurve ColorOffsetAnimationCurve;
	public float ColorOffsetBangLength = 0.5f;
	public float ColorBangDebounce = 0.1f;
	bool AnimatingColorOffset;
	float ColorOffsetBangTime;


	IridescenceController Controller;

	void Start () {
		Controller = GetComponent<IridescenceController>();
	}
	
	// Update is called once per frame
	void Update () {

		if (TriggerBangColorOffset) {
			BangColorOffset();
			TriggerBangColorOffset = false;
		}


		if (AnimatingColorOffset) {
			var position = (Time.time - ColorOffsetBangTime) / ColorOffsetBangLength;
			if (position >= 1) {
				AnimatingColorOffset = false;
				position = 1;
			}
			Controller.ColorOffset += ColorOffsetAnimationCurve.Evaluate(position);
		}

	}

	public void BangColorOffset() {
		if (Time.time - ColorOffsetBangTime < ColorBangDebounce) return;
		AnimatingColorOffset = true;
		ColorOffsetBangTime = Time.time;
	}
}
