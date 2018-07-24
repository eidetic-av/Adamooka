using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DummyAirsticks : MonoBehaviour {

    public bool DummyEnabled = false;
    public bool ControlXYPositionWithMouse = false;
    public bool ControlXYRotationWithMouse = false;

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
    [Range(-1, 1)]
    public float LeftXPosition = 0;
    [Range(-1, 1)]
    public float LeftYPosition = 0;
    [Range(-1, 1)]
    public float LeftZPosition = 0;

    [Range(-1, 1)]
    public float RightXPosition = 0;
    [Range(-1, 1)]
    public float RightYPosition = 0;
    [Range(-1, 1)]
    public float RightZPosition = 0;

    void Update () {
		if (DummyEnabled)
        {

            if (ControlXYPositionWithMouse) {
                // LeftXPosition = Input.mousePosition.x
            }

            AirSticks.Left.EulerAngles = new Vector3(LeftXRotation, LeftYRotation, LeftZRotation);
            AirSticks.Right.EulerAngles = new Vector3(RightXRotation, RightYRotation, RightZRotation);

            AirSticks.Left.Position = new Vector3(LeftXPosition, LeftYPosition, LeftZPosition);
            AirSticks.Right.Position = new Vector3(RightXPosition, RightYPosition, RightZPosition);
        }
	}
}
