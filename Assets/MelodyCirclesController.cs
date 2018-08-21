using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MelodyCirclesController : MonoBehaviour {

	public static MelodyCirclesController Instance;
	public List<CircleController> CircleControllers;

	// Use this for initialization
	void Start () {
		Instance = this;
	}
	
	public void NoteOn(int index) {
		CircleControllers[index].NoteOnFlash = true;
	}
	
	public void NoteOff(int index) {
		CircleControllers[index].NoteOffFlash = true;
	}
}
