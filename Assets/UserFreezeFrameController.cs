using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserFreezeFrameController : MonoBehaviour {

	public static UserFreezeFrameController Instance;

	public FreezeFramer TargetCameraFreezeFramer;
	public GameObject BaseOutputQuad;
	public bool HideBaseOuput = false;
	public bool ShowBaseOutput = false;
	private bool HidingBaseOutput = false;
	private bool ProcessingHiddenBase = false;

    public Vector3 PositionOffset = Vector3.zero;

	public bool Generate = false;

	public bool FadeOutAfterCapture = true;
	public float FadeLength = .5f;
	List<GameObject> QuadsToFade = new List<GameObject>();
	List<float> FadeTimes = new List<float>();

	void Start () {
		Instance = this;
	}
	
	void Update () {

		// if we just processed a frame that was captured with a hidden base quad,
		// re-hide the base output that we revealed to take the capture,
		// and reset it's transform
		if (HidingBaseOutput && ProcessingHiddenBase) {
			BaseOutputQuad.transform.SetPositionAndRotation(
				new Vector3(0, 0, 10f),
				BaseOutputQuad.transform.rotation);
			ProcessingHiddenBase = false;
			HideBaseOuput = true;
		}

		if (HideBaseOuput) {
			var baseOutputMaterial = BaseOutputQuad.GetComponent<Renderer>().material;
			var color = baseOutputMaterial.GetColor("_TintColor");
			color.a = 0;
			baseOutputMaterial.SetColor("_TintColor", color);
			HidingBaseOutput = true;
			HideBaseOuput = false;
		}
		if (ShowBaseOutput) {
			var baseOutputMaterial = BaseOutputQuad.GetComponent<Renderer>().material;
			var color = baseOutputMaterial.GetColor("_TintColor");
			color.a = 1;
			baseOutputMaterial.SetColor("_TintColor", color);
			HidingBaseOutput = false;
			ShowBaseOutput = false;
		}

		if (Generate) {

			// if the output is hidden, show it to capture the frame
			// but move it behind the other layers so it doesn't flash
			if (HidingBaseOutput) {
				var baseOutputMaterial = BaseOutputQuad.GetComponent<Renderer>().material;
				var color = baseOutputMaterial.GetColor("_TintColor");
				color.a = 1;
				baseOutputMaterial.SetColor("_TintColor", color);
				BaseOutputQuad.transform.SetPositionAndRotation(
					new Vector3(0, 0, 10.5f),
					BaseOutputQuad.transform.rotation);
				ProcessingHiddenBase = true;
			}

			var freezeFrameQuad = Instantiate<GameObject>(Resources.Load<GameObject>("FreezeFrameQuad"));
			freezeFrameQuad.transform.parent = gameObject.transform;

			// instance material
			var renderer = freezeFrameQuad.GetComponent<Renderer>();
			renderer.material = new UnityEngine.Material(renderer.material);

			TargetCameraFreezeFramer.SetOutput(freezeFrameQuad);
			TargetCameraFreezeFramer.Capture = true;

			if (FadeOutAfterCapture) {
				QuadsToFade.Add(freezeFrameQuad);
				FadeTimes.Add(Time.time);
			}

			Generate = false;
		}


		if (QuadsToFade.Count > 0) {
			for (int i = 0; i < QuadsToFade.Count; i++) {
				var quad = QuadsToFade[i];
				var fadeTime = FadeTimes[i];
				var fadePosition = (Time.time - fadeTime) / FadeLength;
				if (fadePosition < 1) {
					var alphaValue = 1 - fadePosition;
					var material = quad.GetComponent<Renderer>().material;
					var color = material.GetColor("_Color");
					color.a = alphaValue;
					material.SetColor("_Color", color);
				} else {
					Destroy(quad);
				}
			}
		}
	}
}
