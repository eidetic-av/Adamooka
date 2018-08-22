using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class HeadController : MonoBehaviour {

	public static HeadController Instance;

	SkinnedMeshRenderer HeadRenderer;

	public bool Show = false;
	public bool Hide = false;
	public bool Intro = false;
	bool DoingIntro = false;
	public Transform ActiveTransform;
	public Vector3 IntroPosition = Vector3.zero;
	public Vector3 IntroRotation = Vector3.zero;
	[Range(0, 1)]
	public float IntroSlider = 0f;
	float IntroSliderLerp = 0f;
	public float IntroDamping = 100f;
	public Animator Animator;
	public MeshRenderer FaceOutputRenderer;
	Material FaceOutputMaterial;

	public bool FadeIn = false;
	public float FadeInLength = 3f;
	public bool FadeOut = false;
	public float FadeOutLength = 3f;
	bool FadingIn;
	float FadeInStartTime;
	bool FadingOut;
	float FadeOutStartTime;
	public float Transparency = 0f;
	public bool TriggerKick = false;
	public bool TriggerSnare = false;
	public bool TriggerHats = false;

	void Start () {
		Instance = this;
		HeadRenderer = GetComponentInChildren<SkinnedMeshRenderer>();
		FaceOutputMaterial = FaceOutputRenderer.material;
	}
	
	// Update is called once per frame
	void Update () {

		var xPos = Mathf.Lerp(IntroPosition.x, -20, IntroSlider);
		var yPos = Mathf.Lerp(IntroPosition.y, 0, IntroSlider);
		var zPos = Mathf.Lerp(IntroPosition.z, 0, IntroSlider);
		var position = new Vector3(xPos, yPos, zPos);

		var xRot = Mathf.Lerp(IntroRotation.x, 0, IntroSlider);
		var yRot = Mathf.Lerp(IntroRotation.y, 0, IntroSlider);
		var zRot = Mathf.Lerp(IntroRotation.z, 0, IntroSlider);
		var rotation = Quaternion.Euler(xRot, yRot, zRot);

		ActiveTransform.position = position;
		ActiveTransform.rotation = rotation;

		if (Intro) {
			Show = true;
			DoingIntro = true;
			IntroSliderLerp = 1;
			Intro = false;
		}

		if (Hide) {
			HeadRenderer.enabled = false;
			Hide = false;
		}
		if (Show) {
			HeadRenderer.enabled = true;
			Show = false;
		}
		if (TriggerKick) {
			Kick();
			TriggerKick = false;
		}
		if (TriggerSnare) {
			Snare();
			TriggerSnare = false;
		}
		if (TriggerHats) {
			Hats();
			TriggerHats = false;
		}

		if (FadeIn) {
			FadeInStartTime = Time.time;
			FadingIn = true;
			FadeIn = false;
		}
		if (FadingIn) {
			Transparency = (Time.time - FadeInStartTime) / FadeInLength;
			if (Transparency >= 1){
				Transparency = 1;
				FadingIn = false;
			}
		}

		if (FadeOut) {
			FadeOutStartTime = Time.time;
			FadingOut = true;
			FadeOut = false;
		}
		if (FadingOut) {
			Transparency = 1 - ((Time.time - FadeOutStartTime) / FadeOutLength);
			if (Transparency <= 0){
				Transparency = 0;
				FadingOut = false;
			}
		}

		FaceOutputMaterial.SetColor("_Color", new Color(1, 1, 1, Transparency));

	}

	public void Kick() {
		var state = Animator.GetCurrentAnimatorStateInfo(0);
		var playingKick = state.IsName("Kick");
		var playingKickRepeat = state.IsName("KickRepeat");
		var playingSnare = state.IsName("Snare") || state.IsName("SnareRepeat");
		if (!playingKick && !playingKickRepeat) {
			Animator.Play("Kick", 0, 0f);
		} else if (playingKick) {
			Animator.CrossFade("KickRepeat", 0.05f, 0, 0.115f);
		} else if (playingKickRepeat) {
			Animator.CrossFade("Kick", 0.05f, 0, 0.115f);
		} else if (playingSnare) {
			Animator.CrossFade("Snare", 0.05f, 0, 0.052f);
		}
	}

	public void Snare() {
		var state = Animator.GetCurrentAnimatorStateInfo(0);
		var playingSnare = state.IsName("Snare");
		var playingSnareRepeat = state.IsName("SnareRepeat");
		var playingKick = state.IsName("Kick") || state.IsName("KickRepeat");
		if (!playingSnare && !playingSnareRepeat) {
			Animator.Play("Snare", 0, 0f);
		} else if (playingSnare) {
			Animator.CrossFade("SnareRepeat", 0.05f, 0, 0.052f);
		} else if (playingSnareRepeat) {
			Animator.CrossFade("Snare", 0.05f, 0, 0.052f);
		} else if (playingKick) {
			Animator.CrossFade("Snare", 0.25f, 0, 0.012f);
		}
	}

	public void Hats() {
		var state = Animator.GetCurrentAnimatorStateInfo(0);
		var playingHats = state.IsName("Hats");
		var playingHatsRepeat = state.IsName("HatsRepeat");
		var playingSnare = state.IsName("Snare") || state.IsName("SnareRepeat");
		var playingKick = state.IsName("Kick") || state.IsName("KickRepeat");
		if (!playingHats && !playingHatsRepeat) {
			Animator.CrossFade("Hats", 0.05f, 0, 0f);
		} else if (playingHats) {
			Animator.CrossFade("HatsRepeat", 0.05f, 0, 0.052f);
		} else if (playingHatsRepeat) {
			Animator.CrossFade("Hats", 0.05f, 0, 0.052f);
		} else if (playingKick) {
			Animator.CrossFade("Hats", 0.05f, 0, 0.012f);
		} else if (playingSnare) {
			Animator.CrossFade("Hats", 0.05f, 0, 0.012f);
		}
	}

	public void Beatbox(int animationNumber) {
		if (animationNumber > 5) return;
		Animator.CrossFade("Beatbox" + animationNumber, 0.1f, 0, 0f);
	}

}
