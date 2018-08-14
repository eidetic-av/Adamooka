using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneLightController : MonoBehaviour {

    public static SceneLightController Instance;

    private Light Light;

    public Vector2 Value = Vector2.zero;
    public float ValueDamp = 1f;

    public bool ControlHueAndSaturationWithAirsticks = false;
    public float Hue = 0f;
    public float Saturation = 0f;

    void Start () {
        Instance = this;
        Light = gameObject.GetComponent<Light>();
	}
	
	void Update () {
        float hue, saturation, value;
        Color.RGBToHSV(Light.color, out hue, out saturation, out value);

		if (Value.x != Value.y)
        {
            Value.x = Value.x + (Value.y - Value.x) / ValueDamp;
            value = Value.x;
        }

        if (ControlHueAndSaturationWithAirsticks)
        {
            hue = Hue;
            saturation = Saturation;
        }

        Light.color = Color.HSVToRGB(hue, saturation, value);
	}

    public void SetValue(float startingValue, float endingValue, float damping)
    {
        Value.x = startingValue;
        Value.y = endingValue;
        ValueDamp = damping;
    }
}
