using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class SingerController : MonoBehaviour {

	public bool Play = false;

	Animation Animation;

	void Start () {
		Animation = GetComponent<Animation>();	
	}
	
	// Update is called once per frame
	void Update () {
		if (Play) {
			Animation.Stop();
			Animation.Play();
			Play = false;
		}
	}
}
