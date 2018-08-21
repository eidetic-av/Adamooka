using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HeadController : MonoBehaviour {

	public static HeadController Instance;
	public Animator Animator;
	public bool TriggerKick = false;
	public bool TriggerSnare = false;
	public bool TriggerHats = false;

	void Start () {
		Instance = this;
	}
	
	// Update is called once per frame
	void Update () {
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

}
