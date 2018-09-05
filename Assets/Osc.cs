using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Osc : MonoBehaviour {

	public static Osc Instance;

	public float BeatPosition {get; set;} = 0;
	public float BarPosition {get; set;} = 0;
	public int Breakdown {get; set;} = 0;

	void Start () {
		Instance = this;
	}

}
