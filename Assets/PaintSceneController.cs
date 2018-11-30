using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PaintSceneController : MonoBehaviour {

	public static PaintSceneController Instance;
	public Kino.Feedback PaintFeedback;

	public bool DrawFullTrails = false;
	public bool DisableTrails = false;
	public bool SuckTrails = false;
	public bool EnableOutputQuad = false;
	public bool DisableOutputQuad = false;

	GameObject OutputQuad;

	void Start () {
		Instance = this;
		OutputQuad = GameObject.Find("PaintOutputQuad");
	}
	
	// Update is called once per frame
	void Update () {
		if (DrawFullTrails) {
			PaintFeedback.enabled = true;
			PaintFeedback.color = Color.HSVToRGB(0, 0, 0.99f);
			PaintFeedback.scale = 1f;
			DrawFullTrails = false;
		}
		if (DisableTrails) {
			PaintFeedback.enabled = false;
			DisableTrails = false;
		}
		if (SuckTrails) {
			PaintFeedback.enabled = true;
			PaintFeedback.color = Color.HSVToRGB(0, 0, 1f);
			PaintFeedback.scale = 0.95f;
			DrawFullTrails = false;
			SuckTrails = false;
		}
		if (EnableOutputQuad) {
			if (OutputQuad != null)	
				OutputQuad.SetActive(true);
			EnableOutputQuad = false;
		}
		if (DisableOutputQuad) {
			if (OutputQuad != null)	
				OutputQuad.SetActive(false);
			DisableOutputQuad = false;
		}
	}
}
