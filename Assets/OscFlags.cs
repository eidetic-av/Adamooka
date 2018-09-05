using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscFlags : MonoBehaviour {

	public static OscFlags Instance;

	public int Breakdown = 0;

	void Start () {
		Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

	public void SetBreakdown(int state) {
		Breakdown = state;
	}

}
