using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utility;
using Eidetic.Andamooka;

public class CircleController : MonoBehaviour
{
    public ParticleSystem ParticleSystem;

    public float BaseRadius = 1.5f;
    public float RadiusMultiplier = 1f;
    public float RadiusOffset = 0.5f;
    public Vector2 Radius = new Vector2(1.5f, 1.5f);
    public float RadiusDamp = 5f;

    public bool PositionWithAirSticks = false;
    public AirSticks.Hand Hand = AirSticks.Hand.Left;
    public Vector3 PositionScale = Vector3.one;
    public Vector3 PositionOffset = Vector3.zero;

    public int ParticleCount = 50;

    private Vector2 WidthValue = Vector2.zero;
    public float ColorDamp = 5f;
    // public Vector4 ColorDampMapping = new Vector4(0, 1, 15f, 30f);
    public bool NoteOnFlash = false;
    public Vector2 NoteOnMinMax = new Vector2(0.6f, 1);
    public bool NoteOffFlash = false;
    public Vector2 NoteOffMinMax = new Vector2(1, 0);

    public bool DampPosition = false;
    public float PositionDampRate = 5f;
    Vector3 DampedPosition = Vector3.zero;

    Material MelodyMaterial;

    public bool RotateWithAirsticks = true;

    public bool FadeIn = false;
    bool FadingIn = false;
    public float FadeInLength = 20f;
    float FadeInStartTime;
    public float WidthMultiplier = 1f;


    void Start()
    {
        ParticleSystem.Emit(ParticleCount);
        MelodyMaterial = new Material(Resources.Load("MelodyMaterial") as Material);

        var renderer = ParticleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.trailMaterial = MelodyMaterial;
    }

    void Update()
    {
        CheckVariables();

        var main = ParticleSystem.main;
        main.maxParticles = ParticleCount;

        var particles = new ParticleSystem.Particle[ParticleCount];

        ParticleSystem.GetParticles(particles);

        // ParticleCount - 2 here so that it joins up with itself
        var subAngle = (2 * Mathf.PI) / (ParticleCount - 2);

        Radius.y = (BaseRadius * RadiusMultiplier) + RadiusOffset;

        Radius.x = Radius.x + (Radius.y - Radius.x) / RadiusDamp;

        for (int i = 0; i < ParticleCount; i++)
        {
            var angle = subAngle * i;
            var x = Mathf.Cos(angle) * Radius.x;
            var y = Mathf.Sin(angle) * Radius.x;

            var pos = particles[i].position = new Vector2(x, y);
        }

        ParticleSystem.SetParticles(particles, ParticleCount);

        if (PositionWithAirSticks)
        {
            var stick = AirSticks.Left;
            if (Hand == AirSticks.Hand.Right) stick = AirSticks.Right;

            var x = (stick.EulerAngles.y * PositionScale.x) + PositionOffset.x;
            var y = (stick.Position.y * PositionScale.y) + PositionOffset.y;
            // var z = (stick.Position.z * PositionScale.z) + PositionOffset.z;
            var z = 0;

            if (!DampPosition)
                transform.position = new Vector3(x, y, z);
            else
            {
                if (PositionDampRate == 0) PositionDampRate = 1;
                DampedPosition.x = DampedPosition.x + (x - DampedPosition.x) / PositionDampRate;
                DampedPosition.y = DampedPosition.y + (y - DampedPosition.y) / PositionDampRate;
                DampedPosition.z = DampedPosition.z + (z - DampedPosition.z) / PositionDampRate;
                transform.position = DampedPosition;
            }
        }

        if (RotateWithAirsticks) {
            var stick = AirSticks.Left;
            if (Hand == AirSticks.Hand.Right) stick = AirSticks.Right;

            // ColorDamp = stick.Position.y.Map(ColorDampMapping.x, ColorDampMapping.y, ColorDampMapping.z, ColorDampMapping.w);

            var x = (stick.EulerAngles.y * PositionScale.x) + PositionOffset.x;
            var y = (stick.Position.y * PositionScale.y) + PositionOffset.y;
            // var z = (stick.Position.z * PositionScale.z) + PositionOffset.z;
            var z = 0;
            

            if (!DampPosition)
                ParticleSystem.gameObject.transform.rotation = Quaternion.Euler(new Vector3(y, x, z));
            else
            {
                if (PositionDampRate == 0) PositionDampRate = 1;
                DampedPosition.x = DampedPosition.x + (y - DampedPosition.x) / PositionDampRate;
                DampedPosition.y = DampedPosition.y + (x - DampedPosition.y) / PositionDampRate;
                DampedPosition.z = DampedPosition.z + (z - DampedPosition.z) / PositionDampRate;
                
                ParticleSystem.gameObject.transform.rotation = Quaternion.Euler(DampedPosition);
            }
        }

        UpdateColor();
    }

    void UpdateColor()
    {

        if (NoteOnFlash)
        {
            WidthValue.x = NoteOnMinMax.y;
            WidthValue.y = NoteOnMinMax.x;
            NoteOnFlash = false;
        }
        else if (NoteOffFlash)
        {
            WidthValue.x = NoteOffMinMax.y;
            WidthValue.y = NoteOffMinMax.x;
            NoteOffFlash = false;
        }

        // float h, s, v;
        // Color.RGBToHSV(TrailMaterial.GetColor("_Color"), out h, out s, out v);
        WidthValue.x = WidthValue.x + (WidthValue.y - WidthValue.x) / ColorDamp;
        // v = ColorValue.x;
        // var color = Color.HSVToRGB(h, s, v);
        // var color = new Color(1, 1, 1, WidthValue.x);
        // MelodyMaterial.color = color;

        if (FadeIn) {
            FadingIn = true;
            FadeInStartTime = Time.time;
            FadeIn = false;
        }
        if (FadingIn) {
            var position = (Time.time - FadeInStartTime) / FadeInLength;
            if (position >= 1) {
                position = 1;
                FadingIn = false;
            }
            WidthMultiplier = position;
        }

        var trailsModule = ParticleSystem.trails;
        trailsModule.widthOverTrail = new ParticleSystem.MinMaxCurve(WidthValue.x * WidthMultiplier);
    }

    void CheckVariables()
    {
    }
}
