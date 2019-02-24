using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic.Andamooka;

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
        if (Input.GetKeyDown(KeyCode.A))
            DummyEnabled = !DummyEnabled;

		if (DummyEnabled)
        {
            AirSticks.Left.EulerAngles = new Vector3(LeftXRotation, LeftYRotation, LeftZRotation);
            AirSticks.Right.EulerAngles = new Vector3(RightXRotation, RightYRotation, RightZRotation);

            AirSticks.Left.Position = new Vector3(LeftXPosition, LeftYPosition, LeftZPosition);
            AirSticks.Right.Position = new Vector3(RightXPosition, RightYPosition, RightZPosition);

            if (Input.GetKeyDown(KeyCode.LeftArrow))
                AirSticks.Left.TriggerNoteOn();
            if (Input.GetKeyDown(KeyCode.RightArrow))
                AirSticks.Right.TriggerNoteOn();
            if (Input.GetKeyUp(KeyCode.LeftArrow))
                AirSticks.Left.TriggerNoteOff();
            if (Input.GetKeyUp(KeyCode.RightArrow))
                AirSticks.Right.TriggerNoteOff();
        }
	}
}
