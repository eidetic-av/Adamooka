using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    public Material TrailMaterial;

    private Vector2 ColorValue = Vector2.zero;
    public float ColorDamp = 5f;
    public bool NoteOnFlash = false;
    public Vector2 NoteOnMinMax = new Vector2(0.6f, 1);
    public bool NoteOffFlash = false;
    public Vector2 NoteOffMinMax = new Vector2(1, 0);

    void Start()
    {
        ParticleSystem.Emit(ParticleCount);
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

        if (PositionWithAirSticks) {
           var stick = AirSticks.Left;
            if (Hand == AirSticks.Hand.Right) stick = AirSticks.Right;

            var x = (stick.EulerAngles.y * PositionScale.x) + PositionOffset.x;
            var y = (stick.Position.y * PositionScale.y) + PositionOffset.y;
            // var z = (stick.Position.z * PositionScale.z) + PositionOffset.z;
            var z = 0;

            transform.SetPositionAndRotation(new Vector3(x, y, z), Quaternion.Euler(0, 0, 0));
        }

        UpdateColor();
    }

    void UpdateColor() {

        if (NoteOnFlash) {
            ColorValue.x = NoteOnMinMax.y;
            ColorValue.y = NoteOnMinMax.x;
            NoteOnFlash = false;
        } else if (NoteOffFlash)  {
            ColorValue.x = NoteOffMinMax.y;
            ColorValue.y = NoteOffMinMax.x;
            NoteOffFlash = false;
        }

        float h, s, v;
        Color.RGBToHSV(TrailMaterial.GetColor("_Color"), out h, out s, out v);
        ColorValue.x = ColorValue.x + (ColorValue.y - ColorValue.x) / ColorDamp;
        v = ColorValue.x;
        var color = Color.HSVToRGB(h, s, v);
        TrailMaterial.SetColor("_Color", color);
    }

    void CheckVariables() {
    }
}
