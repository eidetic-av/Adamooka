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

        var subAngle = (2 * Mathf.PI) / ParticleCount;

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
    }

    void CheckVariables() {
    }
}
