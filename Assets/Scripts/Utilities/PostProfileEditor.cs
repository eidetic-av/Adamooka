using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PostProcessing;

public class PostProfileEditor : MonoBehaviour {

    PostProcessingProfile Profile;

    int HueShiftAngle = 0;

	// Use this for initialization
	void Start () {
        Profile = gameObject.GetComponent<Camera>().GetComponent<PostProcessingBehaviour>().profile;
	}
	
	// Update is called once per frame
	void Update () {
        ColorGradingModel.Settings colorGrading = Profile.colorGrading.settings;
        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            colorGrading.basic.hueShift = HueShiftAngle;
        }
        Profile.colorGrading.settings = colorGrading;
	}

    public void RandomiseHue()
    {
        HueShiftAngle = Mathf.CeilToInt(Random.Range(0, 360) - 180);
    }
}
