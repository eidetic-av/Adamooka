using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic.Unity.Utility;
using Utility;

public class OneFiveNineCircleController : MonoBehaviour {

    public static OneFiveNineCircleController Instance;

    public ParticleSystem ParticleSystem;

    public bool Beep = false;
    public float AlphaDamping = 4f;

    public Renderer NonagonRenderer;
    public bool FlashNonagon = false;
    private Vector2 NonagonAlpha = Vector2.zero;
    public float NonagonDamping = 4f;

    private Vector2 Alpha = Vector2.zero;

    public Vector2 AlphaAnimation = new Vector2(0.5f, 0f);

    public float KickLightDamping = 30f;
    public float KickLightOffDamping = 20f;
    public float MinLight = 0f;
    public float MaxLight = 1f;
    
    public bool SendTrackingToSceneLight = false;
    public Vector2 HueMapping = new Vector2(-1, 1);
    public Vector2 SaturationMapping = new Vector2(-1, 1);

    public bool SendSnareToParticles = false;
    private Vector2 SnareRadius = new Vector2(0.08f, 0.08f);
    private float SnareRadiusDamping = 1f;
    public Vector2 SnareMapping = new Vector2(0.8f, 100);
    public float SnareOnDamping = 3f;
    public float SnareOffDamping = 20f;
    private bool SnareAffectOn = false;
    public float SnareOnBeepMinimum = 0.6f;

    public bool Rotate = false;
    public float RotationAngle = 10f;
    public float RotationDamping = 5f;
    private Vector2 Rotation = Vector2.zero;
        

    void Start () {
        Instance = this;

        AirSticks.Right.NoteOn += KickOn;
        AirSticks.Right.NoteOff += KickOff;

        AirSticks.Left.NoteOn += SnareOn;
        AirSticks.Left.NoteOff += SnareOff;
    }
	
	void Update () {
		if (Beep)
        {
            Alpha.x = AlphaAnimation.x;
            if (!SnareAffectOn)
            {
                Alpha.y = AlphaAnimation.y;
            } else
            {
                Alpha.y = SnareOnBeepMinimum;
            }
            Beep = false;
        }
        if (FlashNonagon)
        {
            NonagonAlpha.x = 1f;
            NonagonAlpha.y = 0f;
            FlashNonagon = false;
        }

        if (Mathf.Abs(Alpha.y - Alpha.x) > 0)
        {
            Alpha.x = Alpha.x + (Alpha.y - Alpha.x) / AlphaDamping;
            var newColor = ParticleSystem.main.startColor.color;
            newColor.a = Alpha.x;

            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[ParticleSystem.particleCount];
            ParticleSystem.GetParticles(particles);
            for (int i= 0; i < particles.Length; i++)
            {
                particles[i].startColor = newColor;
            }
            ParticleSystem.SetParticles(particles, particles.Length);
            var mainModule = ParticleSystem.main;
            mainModule.startColor = newColor;
        }


        if (Mathf.Abs(NonagonAlpha.y - NonagonAlpha.x) > 0)
        {
            NonagonAlpha.x = NonagonAlpha.x + (NonagonAlpha.y - NonagonAlpha.x) / NonagonDamping;
            NonagonRenderer.material.SetColor("_TintColor", Color.HSVToRGB(1, 0, NonagonAlpha.x));
        }

        
        if (SendTrackingToSceneLight)
        {
            var hue = AirSticks.Right.Position.x.Map(-1, 1, HueMapping.x, HueMapping.y);
            var saturation = AirSticks.Right.Position.z.Map(-1, 1, SaturationMapping.x, SaturationMapping.y);

            SceneLightController.Instance.Hue = hue;
            SceneLightController.Instance.Saturation = saturation;

        }

        if (SendSnareToParticles)
        {
            if (Mathf.Abs(SnareRadius.x - SnareRadius.y) > 0)
            {
                SnareRadius.x = SnareRadius.x + (SnareRadius.y - SnareRadius.x) / SnareRadiusDamping;

                var mainModule = ParticleSystem.main;
                mainModule.startSizeY = SnareRadius.x;
            }
        }

        if (Rotate)
        {
            Rotation.y += RotationAngle;
            Rotate = false;
        }

        if (Mathf.Abs(Rotation.x - Rotation.y) > 0)
        {
            Rotation.x = Rotation.x + (Rotation.y - Rotation.x) / RotationDamping;
            transform.eulerAngles = new Vector3(0, 0, Rotation.x);
        }
    }

    void DoRotate(float angle, float damping)
    {
        Rotate = true;
        RotationAngle = angle;
        RotationDamping = damping;
    }

    void KickOn()
    {
        if (SendTrackingToSceneLight)
        SceneLightController.Instance.SetValue(MaxLight, MinLight, KickLightDamping);
    }

    void KickOff()
    {
        if (SendTrackingToSceneLight)
        {
            var currentValue = SceneLightController.Instance.Value.x;
            SceneLightController.Instance.SetValue(currentValue, 0, 10f);
        }
    }



    void SnareOn()
    {
        if (SendSnareToParticles)
        {
            SnareRadius.x = SnareMapping.x;
            SnareRadius.y = SnareMapping.y;
            SnareRadiusDamping = SnareOnDamping;
            SnareAffectOn = true;
        }
    }

    void SnareOff()
    {
        if (SendSnareToParticles)
        {
            SnareRadius.x = SnareMapping.y;
            SnareRadius.y = SnareMapping.x;
            SnareRadiusDamping = SnareOffDamping;
            SnareAffectOn = false;
        }
    }
}
