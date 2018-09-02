using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Midi;
using UnityEngine.PostProcessing;

public class IridescenceController : MonoBehaviour
{
    public bool Activate;

    public Material IridescenceMaterial;
    
    public Light SceneLight;
    [Range(0.001f, 10)]
    public float LightIntensity;
    private float PreviousLightIntensity;

    public Renderer ClearLayerRenderer;
    [Range(0, 1)]
    public float ClearLayerAlpha = 1f;
    private float PreviousClearLayerAlpha = 1f;

    /* CLASS COMPONENTS*/

    [Range(0f, 200f)]
    public float FilmStrength = 0.5f;
    [Range(1f, 1000f)]
    public float FilmFrequency = 0f;

    [Range(1f, 1000f)]
    public float NoiseMultiplier = 1f;
    [Range(0f, 1000f)]
    public float NoiseOffset = 0f;
    public bool AnimateNoiseOffset;
    public float NoiseOffsetAnimationRate = 0;

    [Range(1f, 1000f)]
    public float ColorNoiseMultiplier = 1f;
    [Range(0f, 1000f)]
    public float ColorOffset = 0f;
    public bool AnimateColorOffset;
    public float ColorOffsetAnimationRate = 0;

    [Range(-1f, 1f)]
    public float HueOffset;
    [HideInInspector()]
    public float CurrentHue;
    float Saturation = 0;

    [HideInInspector]
    public Renderer Renderer;

    bool ApplyToChildren = false;
    float SpecularPower = 1f;

    private bool DebugToConsole = true;

    
	
	public bool ColourBang = false;
	public AnimationCurve ColourBangCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
	public float ColourBangLength = 0.25f;
	float ColourBangStartTime;
    float StartingColourOffset;
	bool ChangingColour;


    void Start()
    {
        Renderer = GetComponent<Renderer>();

        if (SceneLight != null)
        {
            LightIntensity = SceneLight.intensity;
            PreviousLightIntensity = SceneLight.intensity;
        }

        if (ClearLayerRenderer != null)
        {
            ClearLayerAlpha = ClearLayerRenderer.sharedMaterial.GetColor("_Color").a;
            PreviousClearLayerAlpha = ClearLayerRenderer.sharedMaterial.GetColor("_Color").a;
        }
    }

    public void TriggerColourBang() {
        ColourBang = true;
    }

    void Update()
    {

		if (ColourBang) {
			ColourBangStartTime = Time.time;
            StartingColourOffset = ColorOffset;
			ChangingColour = true;
			ColourBang = false;
		}
		if (ChangingColour) {
			float position = (Time.time - ColourBangStartTime) / ColourBangLength;
			if (position > 1){
				position = 1;
				ChangingColour = false;
			} 
			float value = ColourBangCurve.Evaluate(position);
            ColorOffset = StartingColourOffset + value;
		}






        if (Activate)
        {
            gameObject.GetComponent<Renderer>().sharedMaterial = Instantiate(IridescenceMaterial);
            Activate = false;
            LightIntensity = 0.3f;
        }
        if (SceneLight != null && LightIntensity != PreviousLightIntensity)
        {
            SceneLight.intensity = LightIntensity;
            PreviousLightIntensity = LightIntensity;
        }
        if (ClearLayerRenderer != null && ClearLayerAlpha != PreviousClearLayerAlpha)
        {
            Color color = ClearLayerRenderer.sharedMaterial.GetColor("_Color");
            color.a = ClearLayerAlpha;
            ClearLayerRenderer.sharedMaterial.SetColor("_Color", color);
            PreviousClearLayerAlpha = ClearLayerAlpha;
        }


        UpdateAnimatedProperties();
        if (ApplyToChildren)
        {
            Renderer[] rds = GetComponentsInChildren<Renderer>();
            for (int i = 0; i < rds.Length; i++)
            {
                rds[i].material.SetFloat("_Thinfilm_Strength", FilmStrength);
                rds[i].material.SetFloat("_Thinfilm_Color_Freq", FilmFrequency);
                rds[i].material.SetFloat("_SpecPower", SpecularPower);
                rds[i].material.SetFloat("_NoiseMultiplier", NoiseMultiplier);
                rds[i].material.SetFloat("_NoiseOffset", NoiseOffset);
                rds[i].material.SetFloat("_ColorNoiseMultiplier", ColorNoiseMultiplier);
                rds[i].material.SetFloat("_ColorOffset", ColorOffset);
            }
        }
        else
        {
            SetMaterialsFloat("_Thinfilm_Strength", FilmStrength);
            SetMaterialsFloat("_Thinfilm_Color_Freq", FilmFrequency);
            SetMaterialsFloat("_SpecPower", SpecularPower);
            SetMaterialsFloat("_NoiseMultiplier", NoiseMultiplier);
            SetMaterialsFloat("_NoiseOffset", NoiseOffset);
            SetMaterialsFloat("_ColorNoiseMultiplier", ColorNoiseMultiplier);
            SetMaterialsFloat("_ColorOffset", ColorOffset);
        }
    }
    void UpdateAnimatedProperties()
    {
        if (AnimateColorOffset)
        {
            ColorOffset += (ColorOffsetAnimationRate * Time.deltaTime);
        }
        if (AnimateNoiseOffset)
        {
            NoiseOffset += (NoiseOffsetAnimationRate * Time.deltaTime);
        }
    }

    void SetLightSaturation(int ccValue)
    {
        if (SceneLight != null)
        {
            Saturation = Mathf.Pow(Map(ccValue, 0f, 127f, 0f, 1f), 0.6f);
        }
    }
    void SetLightHue(int ccValue)
    {
        if (SceneLight != null)
        {
            // get the color and change it's hue 
            // according to the cutoff
            float h, s, v;
            Color.RGBToHSV(SceneLight.color,
                out h, out s, out v);

            h = (ccValue / 127f) + HueOffset;
            if (h < 0) h = h + 1;
            CurrentHue = h;

            // and assign it to the light
            SceneLight.color = Color.HSVToRGB(h, s, 130 / 255f);
        }
    }

    void SetFilmFrequency(int ccValue)
    {
        FilmFrequency = Map(ccValue, 30, 90, 1, 30);
    }

    void SetFilmStrength(int ccValue)
    {
        FilmStrength = Map(Mathf.Pow((ccValue / 127f), 2), 30, 90, 20, 50);
    }
    void SetNoiseOffsetRate(int ccValue)
    {
        NoiseOffsetAnimationRate =
            (Mathf.Pow(Map(ccValue, 0, 127, 0, 1), 3)) * -50;
    }
    void SetMaterialsFloat(string name, float f)
    {
        Material[] mats = Renderer.materials;
        for (int i = 0; i < mats.Length; i++)
        {
            mats[i].SetFloat(name, f);
        }
    }


    public static float Map(float input, float oldMin, float oldMax, float newMin, float newMax)
    {
        return ((input - oldMin) / (oldMax - oldMin)) * (newMax - newMin) + newMin;
    }

    public static float PowerMap(float ccValue, float power, float newMin, float newMax)
    {
        return Map(CCPower(ccValue, power), 0, 1, newMin, newMax);
    }

    public static float CCPower(float ccValue, float power)
    {
        return Mathf.Pow((ccValue / 127f), power);
    }
}