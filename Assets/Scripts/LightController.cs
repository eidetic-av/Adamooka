using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;

public class LightController : MonoBehaviour
{

    public static List<LightController> Instances = new List<LightController>();

    public Light Light;

    public bool RotateHue = false;
    public float HueRotationSpeed = 0.01f;

    public bool SetXPositionWithBarPosition = false;
    public Vector2 XPositionMinMax = new Vector2(0, 1);
    public bool SinX = false;
    float Phase = 0;

    bool ActivatingSetXPosition;
    bool DeactivatingSetXPosition;
    Color Color;

    float ControllerRotation = 0;

    public bool OverrideColorActive = false;
    float OverrideColorHue = 0f;
    float OverrideDiff;
    bool ToggleOverrideColor = false;

    public bool PulseLightActive = false;
    public AnimationCurve PulseCurve;
    float DefaultLightIntensity;
    float PulseStartingIntensity;
    bool TogglePulse;
    bool TogglePulseSpeed;
    bool BarPulse = true;
    bool DeactivatePulse = false;

    void Start()
    {
        Instances.Add(this);
        Color = Light.color;
        PulseStartingIntensity = Light.intensity;
        DefaultLightIntensity = Light.intensity;
    }

    void Update()
    {
        if (ToggleOverrideColor)
        {
            OverrideColorActive = !OverrideColorActive;
            float h, s, v;
            Color.RGBToHSV(Light.color, out h, out s, out v);
            OverrideColorHue = h;
            ToggleOverrideColor = false;
        }

        if (TogglePulse)
        {
            PulseLightActive = !PulseLightActive;
            if (!PulseLightActive) DeactivatePulse = true;
            else
                PulseStartingIntensity = Light.intensity;
            TogglePulse = false;
        }

        if (TogglePulseSpeed)
        {
            BarPulse = !BarPulse;
            TogglePulseSpeed = false;
        }

        if (PulseLightActive || DeactivatePulse)
        {
            var position = 0f;
            if (BarPulse)
            {
                position = PulseCurve.Evaluate(Osc.Instance.BarPosition);
            }
            else
            {
                position = PulseCurve.Evaluate(Osc.Instance.BeatPosition);
            }
            // if (PulseBeats) position = PulseCurve.Evaluate(Osc.Instance.)
            Light.intensity = PulseStartingIntensity * position;

            if (DeactivatePulse)
            {
                if ((Light.intensity > PulseStartingIntensity - (PulseStartingIntensity * 0.05f))
                && (Light.intensity < PulseStartingIntensity + (PulseStartingIntensity * 0.05f)))
                {
                    DeactivatePulse = false;
                }
            }
        }

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
        if (!OverrideColorActive)
        {
            if (!RotateHue) return;
            float h, s, v;
            Color.RGBToHSV(Light.color, out h, out s, out v);
            h += HueRotationSpeed;
            Color = Color.HSVToRGB(h, s, v);
        }
        else
        {
            float h, s, v;
            Color.RGBToHSV(Light.color, out h, out s, out v);
            h = OverrideColorHue;
            Color = Color.HSVToRGB(h, s, v);
        }
    }

    public void SetXPosition(float value)
    {
        if (SinX)
        {
            value = Mathf.Sin(value * Mathf.PI);
            value = value.Map(-1, 1, XPositionMinMax.x, XPositionMinMax.y);
        }
        else
        {
            value = value.Map(0, 1, XPositionMinMax.x, XPositionMinMax.y);
        }
        if (!ActivatingSetXPosition)
        {
            var localPosition = Light.transform.localPosition;
            localPosition.x = value;
            Light.transform.localPosition = localPosition;
        }
        else
        {
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

    public void SetControllerRotation(float rotation)
    {
        var diff = ControllerRotation - rotation;
        var objectRotation = transform.localEulerAngles;
        objectRotation.z = objectRotation.z + (diff * 360);
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

    public void ActivateColorOverride()
    {
        foreach (var lightController in Instances)
        {
            lightController.OverrideColorActive = true;
        }
    }

    public void DeactivateColorOverride()
    {
        foreach (var lightController in Instances)
        {
            lightController.OverrideColorActive = true;
        }
    }

    public void ToggleColorOverride()
    {
        ToggleOverrideColor = true;
    }

    public void SetOverrideColor(float inputValue)
    {
        OverrideColorHue = inputValue;
    }

    public void TogglePulseLight()
    {
        TogglePulse = true;
    }

    public void TogglePulseLightSpeed()
    {
        TogglePulseSpeed = true;
    }

}
