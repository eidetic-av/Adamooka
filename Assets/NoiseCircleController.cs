using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseCircleController : MonoBehaviour
{

    public ParticleSystem ParticleSystem;

    public float Radius = 1.5f;

    public bool Bang;
    private Vector2 CurveIntensity = Vector2.zero;
    public float IntensityMultiplier = 0f;
    public float BangDamping = 5f;

    public AnimationCurve WeightCurve;

    public bool CurveNoise = true;
    public float CurveNoiseIntensity = 1f;
    private float[] CurveNoiseValues;

    void Update()
    {
        var particles = new ParticleSystem.Particle[ParticleSystem.particleCount];
        ParticleSystem.GetParticles(particles);

        if (particles.Length <= 0) return;

        if (Bang)
        {
            CurveIntensity.y = IntensityMultiplier;
        }

        if (CurveIntensity.x != CurveIntensity.y)
        {
            CurveIntensity.x = CurveIntensity.x + (CurveIntensity.y - CurveIntensity.x) / BangDamping;
        }

        var subAngle = (2 * Mathf.PI) / particles.Length;

        for (int i = 0; i < particles.Length; i++)
        {
            var angle = subAngle * i;
            var x = Mathf.Cos(angle) * Radius;
            var y = Mathf.Sin(angle) * Radius;

            var weight = WeightCurve.Evaluate((float)i / particles.Length);

            if (Bang)
            {
                if (CurveNoise)
                {
                    if (CurveNoiseValues == null || CurveNoiseValues.Length != particles.Length)
                    {
                        CurveNoiseValues = new float[particles.Length];
                    }
                    CurveNoiseValues[i] = 1 + (Mathf.PerlinNoise(x, y) * CurveNoiseIntensity);
                }
            }

            float noiseScale = 1;

            if (CurveNoiseValues != null && CurveNoise)
            {
                noiseScale = CurveNoiseValues[i];
            }

            var curveOffset = CurveIntensity.x * weight;

            particles[i].position = new Vector2(x + (x * (curveOffset * noiseScale)), y + (y * (curveOffset * noiseScale)));
        }

        ParticleSystem.SetParticles(particles, particles.Length);

        Bang = false;
    }
}
