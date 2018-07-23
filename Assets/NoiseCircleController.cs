using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseCircleController : MonoBehaviour
{

    public ParticleSystem ParticleSystem;

    public float Radius = 1.5f;

    public int ParticleCount = 50;

    public bool Trigger;
    public int AttackMs = 10;
    public int DecayMs = 500;
    public AnimationCurve AttackCurve = AnimationCurve.Linear(0, 0, 1, 1);
    public AnimationCurve DecayCurve= AnimationCurve.Linear(0, 1, 1, 0);

    private int EnvelopeStartTime;
    private EnvelopeState CurrentEnvelopeState;
    private float CurrentEnvelopeValue = 0f;

    private Vector2 CurveIntensity = Vector2.zero;
    public float IntensityMultiplier = 0f;
    public float BangDamping = 5f;

    public AnimationCurve WeightCurve;

    public bool CurveNoise = true;
    public float CurveNoiseIntensity = 1f;
    private float[] CurveNoiseValues;

    void Start()
    {
        ParticleSystem.Emit(ParticleCount);
    }

    void Update()
    {
        if (Trigger)
        {
            StartEnvelope();
        }
        else if (CurrentEnvelopeState != EnvelopeState.Off)
        {
            UpdateEnvelope();
            CurveIntensity.y = CurrentEnvelopeValue * IntensityMultiplier;
        }

        var main = ParticleSystem.main;
        main.maxParticles = ParticleCount;

        var particles = new ParticleSystem.Particle[ParticleCount];

        ParticleSystem.GetParticles(particles);

        if (ParticleSystem.particleCount != ParticleCount)
        {
            ParticleSystem.Clear();
            ParticleSystem.Emit(ParticleCount);
        }

        if (CurveIntensity.x != CurveIntensity.y)
        {
            CurveIntensity.x = CurveIntensity.x + (CurveIntensity.y - CurveIntensity.x) / BangDamping;
        }

        var subAngle = (2 * Mathf.PI) / ParticleCount;

        for (int i = 0; i < ParticleCount; i++)
        {
            var angle = subAngle * i;
            var x = Mathf.Cos(angle) * Radius;
            var y = Mathf.Sin(angle) * Radius;

            var weight = WeightCurve.Evaluate((float)i / ParticleCount);

            if (Trigger)
            {
                if (CurveNoise)
                {
                    if (CurveNoiseValues == null || CurveNoiseValues.Length != ParticleCount)
                    {
                        CurveNoiseValues = new float[ParticleCount];
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

            particles[i].position = new Vector2(x + (x * curveOffset), y + (y * curveOffset));
        }

        ParticleSystem.SetParticles(particles, ParticleCount);

        Trigger = false;
    }

    void StartEnvelope()
    {
        // Start time = current time in Ms
        EnvelopeStartTime = GetCurrentMs();

        // set state to attack
        CurrentEnvelopeState = EnvelopeState.Attack;

        Trigger = false;
    }

    void UpdateEnvelope()
    {
        switch (CurrentEnvelopeState)
        {
            case EnvelopeState.Attack:
                {
                    var envelopeTime = (GetCurrentMs() - EnvelopeStartTime) / (float)AttackMs;
                    if (envelopeTime >= 1) {
                        envelopeTime = 1;
                        CurrentEnvelopeState = EnvelopeState.Decay;
                        EnvelopeStartTime = GetCurrentMs();
                    }
                    CurrentEnvelopeValue = AttackCurve.Evaluate(envelopeTime);
                    break;
                }
            case EnvelopeState.Decay:
                {
                    var envelopeTime = (GetCurrentMs() - EnvelopeStartTime) / (float)DecayMs;
                    if (envelopeTime >= 1) {
                        envelopeTime = 1;
                        CurrentEnvelopeState = EnvelopeState.Off;
                        EnvelopeStartTime = 0;
                    }
                    CurrentEnvelopeValue = DecayCurve.Evaluate(envelopeTime);
                    break;
                }
        }
    }

    int GetCurrentMs()
    {
        return Mathf.RoundToInt(Time.time * 1000);
    }

    public enum EnvelopeState
    {
        Off, Attack, Decay
    }
}
