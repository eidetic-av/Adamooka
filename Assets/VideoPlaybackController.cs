﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlaybackController : MonoBehaviour {

	public bool StartPlayback = false;
	public bool StopPlayback = false;

	public List<float> Cues = new List<float>();

	public int Cue;
	public bool Jump = false;

	VideoPlayer VideoPlayer;

	void Start () {
		VideoPlayer = GetComponent<VideoPlayer>();
		VideoPlayer.Prepare();
	}
	
	// Update is called once per frame
	void Update () {
		if (StartPlayback) {
			VideoPlayer.Stop();
			VideoPlayer.Play();
			StartPlayback = false;
		}	
		if (StopPlayback) {
			VideoPlayer.Stop();
			StopPlayback = false;
		}	
		if (Jump) {
			JumpToCue(Cue);
			Jump = false;
		}
	}

	public void SetTime(float timeInSeconds) {
		if (!VideoPlayer.canSetTime) return;
		VideoPlayer.time = timeInSeconds;
	}

	public void JumpToCue(int cueNumber) {
		if (Cues.Count < cueNumber + 1){ 
			Debug.Log("No such cue");
			return;
		}
		SetTime(Cues[cueNumber]);
	}
}
