using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseCircleController : MonoBehaviour
{

    public ParticleSystem ParticleSystem;

    public float Radius = 1.5f;

    public bool Bang;
    public Vector2Int IndicesRange = new Vector2Int(30, 50);
    private Vector2 ScalePosition = Vector2.one;
    public float ScaleMultiplier = 1f;
    public float BangDamping = 5f;

    public AnimationCurve WeightCurve;

    void Update()
    {
        if (Bang)
        {
            ScalePosition.y = ScaleMultiplier;
            Bang = false;
        }

        if (ScalePosition.x != ScalePosition.y)
        {
            ScalePosition.x = ScalePosition.x + (ScalePosition.y - ScalePosition.x) / BangDamping;
        }

        var particles = new ParticleSystem.Particle[ParticleSystem.particleCount];
        ParticleSystem.GetParticles(particles);

        if (particles.Length <= 0) return;

        var subAngle = (2 * Mathf.PI) / particles.Length;

        for (int i = 0; i < particles.Length; i++)
        {
            var angle = subAngle * i;
            var x = Mathf.Cos(angle) * Radius;
            var y = Mathf.Sin(angle) * Radius;

            var scale = 1f;
            var weight = 1f;
            var offset = 0f;

            if (i >= IndicesRange.x && i <= IndicesRange.y)
            {
                float subIndex = (i - IndicesRange.x) + 1;
                float range = (IndicesRange.y - IndicesRange.x);
                scale = ScalePosition.x;
                weight = WeightCurve.Evaluate(subIndex / range);
                offset = scale * weight;
            }

            particles[i].position = new Vector2(x + offset, y + offset);
        }

        ParticleSystem.SetParticles(particles, particles.Length);

    }
}
