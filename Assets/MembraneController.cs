using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MembraneController : MonoBehaviour
{
    public static MembraneController Instance;
    public ParticleSystem ParticleSystem;

    public float BaseRadius = 1.5f;
    public float RadiusOffset = 0.5f;
    public Vector2 Radius = new Vector2(1.5f, 1.5f);
    public float RadiusDamp = 5f;

    public int ParticleCount = 50;

    public Material TrailMaterial;

    private Vector2 ColorValue = Vector2.zero;
    public float ColorDamp = 5f;
    public bool FlashColor = false;
    public Vector2 FlashMinMax = new Vector2(0, 1);

    void Start()
    {
        Instance = this;

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

        var noiseCircle = NoiseCircleController.Instance;
        Radius.y = Mathf.Clamp(noiseCircle.CurrentMaxRadius, BaseRadius, 5000) + RadiusOffset;

        Radius.x = Radius.x + (Radius.y - Radius.x) / RadiusDamp;

        for (int i = 0; i < ParticleCount; i++)
        {
            var angle = subAngle * i;
            var x = Mathf.Cos(angle) * Radius.x;
            var y = Mathf.Sin(angle) * Radius.x;

            var pos = particles[i].position = new Vector2(x, y);
        }

        ParticleSystem.SetParticles(particles, ParticleCount);

        UpdateColor();
    }

    void UpdateColor() {

        if (FlashColor) {
            ColorValue.x = FlashMinMax.y;
            ColorValue.y = FlashMinMax.x;
            FlashColor = false;
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
