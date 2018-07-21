using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyAirsticks : MonoBehaviour {

    public bool DummyEnabled = false;

    [Range(-1, 1)]
    public float LeftXRotation = 0;
    [Range(-1, 1)]
    public float LeftYRotation = 0;
    [Range(-1, 1)]
    public float LeftZRotation = 0;

    [Range(-1, 1)]
    public float RightXRotation = 0;
    [Range(-1, 1)]
    public float RightYRotation = 0;
    [Range(-1, 1)]
    public float RightZRotation = 0;

    void Update () {
		if (DummyEnabled)
        {
            AirSticks.Left.EulerAngles = new Vector3(LeftXRotation, LeftYRotation, LeftZRotation);
            AirSticks.Right.EulerAngles = new Vector3(RightXRotation, RightYRotation, RightZRotation);
        }
	}
}
