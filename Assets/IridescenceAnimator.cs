using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class IridescenceAnimator : MonoBehaviour {

	public bool TriggerBangColorOffset = false;
	public AnimationCurve ColorOffsetAnimationCurve;
	public float ColorOffsetBangLength = 0.5f;
	public float ColorBangDebounce = 0.1f;
	bool AnimatingColorOffset;
	float ColorOffsetBangTime;

	public bool AnimateFilmFrequency = false;
	public Vector2 FilmFrequencyMinMax = new Vector2(1, 10);
	public float FilmFrequencyAnimationSpeed = 0.1f;


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

		if (AnimateFilmFrequency) {
			Controller.FilmFrequency = Mathf.Sin(Time.time * FilmFrequencyAnimationSpeed).Map(-1 ,1, FilmFrequencyMinMax.x, FilmFrequencyMinMax.y);
		}

	}

	public void BangColorOffset() {
		if (Time.time - ColorOffsetBangTime < ColorBangDebounce) return;
		AnimatingColorOffset = true;
		ColorOffsetBangTime = Time.time;
	}
}
