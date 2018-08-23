using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class FieldOfViewAnimator : MonoBehaviour
{
    public Camera Camera;
    public float Length;
    public float StartFOV;
    public float EndFOV;
    public bool Start;
    bool Animating;

    float AnimationStartTime;

    void Update()
    {
        if (Start)
        {
            AnimationStartTime = Time.time;
            Animating = true;
            Start = false;
        }
        if (Animating)
        {
            var position = (Time.time - AnimationStartTime) / Length;
			if (position >= 1) {
				position = 1;
				Animating = false;
			}
			Camera.fieldOfView = position.Map(0, 1, StartFOV, EndFOV);
        }
    }
}
