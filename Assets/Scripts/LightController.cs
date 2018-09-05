using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class LightController : MonoBehaviour
{

    public Light Light;

    public bool RotateHue = false;
    public float HueRotationSpeed = 0.01f;

    public bool SetXPositionWithBarPosition = false;
    public Vector2 XPositionMinMax = new Vector2(0, 1);

    bool ActivatingSetXPosition;
    bool DeactivatingSetXPosition;
    Color Color;

	float ControllerRotation = 0;

    void Start()
    {
        Color = Light.color;
    }

    void Update()
    {
        HueRotate();
        Light.color = Color;

        if (SetXPositionWithBarPosition || ActivatingSetXPosition)
        {
            SetXPosition(Osc.Instance.BarPosition);
        }
        else
        {
            var localPosition = Light.transform.localPosition;
            localPosition.x = 0f;
            Light.transform.localPosition = localPosition;
        }
    }

    void HueRotate()
    {
        if (!RotateHue) return;
        float h, s, v;
        Color.RGBToHSV(Light.color, out h, out s, out v);
        h += HueRotationSpeed;
        Color = Color.HSVToRGB(h, s, v);
    }

    public void SetXPosition(float value)
    {
        value = value.Map(0, 1, XPositionMinMax.x, XPositionMinMax.y);
        if (!ActivatingSetXPosition)
        {
            var localPosition = Light.transform.localPosition;
            localPosition.x = value;
            Light.transform.localPosition = localPosition;
        } else {
			if (value > -0.125 && value < 0.125)
            {
                SetXPositionWithBarPosition = true;
                ActivatingSetXPosition = false;
            }
		}
        if (DeactivatingSetXPosition)
        {
            if (value > -0.125 && value < 0.125)
            {
                SetXPositionWithBarPosition = false;
                DeactivatingSetXPosition = false;
            }
        }
    }

	public void SetControllerRotation(float rotation) {
		var diff = ControllerRotation - rotation;
		var objectRotation = transform.localEulerAngles;
		objectRotation.z = objectRotation.z + (diff*360);
		transform.localRotation = Quaternion.Euler(objectRotation);
		ControllerRotation = rotation;
	}

    public void ActivateBarPositionToX()
    {
        ActivatingSetXPosition = true;
    }

    public void DeactivateBarPositionToX()
    {
        DeactivatingSetXPosition = true;
    }



}
