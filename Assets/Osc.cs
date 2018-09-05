using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Osc : MonoBehaviour {

	public static Osc Instance;

	public int Breakdown = 0;

	public float BarPosition = 0;

	void Start () {
		Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetBreakdown(int state) {
		Breakdown = state;
	}

	public void SetBarPosition(float value) {
		BarPosition = value;
	}

}
