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

	public void GoToPreset() {
		CircleControllers.ForEach(controller => {
			// turn off airsticks control
			controller.RotateWithAirsticks = false;
			// move to position
			var index = CircleControllers.IndexOf(controller);
			Transform transform = null;
			switch(index) {
				case 0:
					transform = controller.ParticleSystem.transform;
					transform.position = new Vector3(0.7f, -0.5f, 0);
					transform.rotation = Quaternion.Euler(-45f, -90f, 0f);
					break;
				case 4:
					transform = controller.ParticleSystem.transform;
					transform.position = new Vector3(-0.7f, -0.5f, 0);
					transform.rotation = Quaternion.Euler(-45f, 90f, 0f);
					break;
				case 5:
					transform = controller.ParticleSystem.transform;
					transform.position = new Vector3(-0.7f, 0.5f, 0);
					transform.rotation = Quaternion.Euler(45f, 90f, 0f);
					break;
				case 6:
					transform = controller.ParticleSystem.transform;
					transform.position = new Vector3(0.7f, -0.5f, 0);
					transform.rotation = Quaternion.Euler(45f, 90f, 0f);
					break;
				case 7:
					transform = controller.ParticleSystem.transform;
					transform.position = new Vector3(0.7f, 0.5f, 0);
					transform.rotation = Quaternion.Euler(45f, -90f, 0f);
					break;
				case 8:
					transform = controller.ParticleSystem.transform;
					transform.position = new Vector3(-0.7f, 0.5f, 0);
					transform.rotation = Quaternion.Euler(45f, 90f, 0f);
					break;
			}
		});
		// turn off the scene light
		// GameObject.Find("SceneLight").GetComponent<Light>().enabled = false;
		// start them getting sucked into the tunnel
		GameObject.Find("ParticleScene").GetComponent<FieldOfViewAnimator>().Start = true;
	}
}
