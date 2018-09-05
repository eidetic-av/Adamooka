using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class PatternController : MonoBehaviour {

	public static PatternController Instance;

	public ParticleSystem ParticleSystem;
	
	public bool SpeedBang = false;
	public AnimationCurve SpeedBangCurve = AnimationCurve.EaseInOut(0, 5, 1, 1);
	public float SpeedBangLength = 1f;
	float SpeedBangStartTime;
	bool ChangingSpeed;

	void Start () {
		Instance = this;
	}
	
	void Update () {

		if (SpeedBang) {
			SpeedBangStartTime = Time.time;
			ChangingSpeed = true;
			SpeedBang = false;
		}
		if (ChangingSpeed) {
			float position = (Time.time - SpeedBangStartTime) / SpeedBangLength;
			if (position > 1){
				position = 1;
				ChangingSpeed = false;
			} 
			float value = SpeedBangCurve.Evaluate(position);
			var mainModule = ParticleSystem.main;
			mainModule.simulationSpeed = value;
		}

	}

	public void TriggerSpeedBang() {
		if (Osc.Instance.Breakdown == 0)
			SpeedBang = true;
	}
}
