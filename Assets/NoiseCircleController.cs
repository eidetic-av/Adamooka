﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseCircleController : MonoBehaviour
{
    public static NoiseCircleController Instance;
    public List<ParticleSystem> ParticleSystems;
    public bool StartSystem = false;
    public float StartSystemLength = 5f;
    private float StartSystemTime;
    private bool StartingSystem = false;
    private int StartingSystemParticleCount = 0;
    public bool StopSystem = false;
    public float StopSystemLength = 8f;
    private float StopSystemTime;
    private bool StoppingSystem = false;

    public bool ExpandToRain = false;

    public float InitialRadius = 1.5f;
    public float CurrentMaxRadius = 1.5f;

    public int ParticleCount = 50;
    public bool CloseCircle = true;

    public bool EnableBaseNoise = false;
    public float BaseNoiseIntensity = 0f;
    public float BaseNoiseDamping = 2f;
    private Vector2[] BaseNoiseValues;

    public List<HitPoint> HitPoints = new List<HitPoint>();

    public List<AnimationCurve> WeightCurves = new List<AnimationCurve>();
    public List<AnimationCurve> SecondaryWeightCurves = new List<AnimationCurve>();
    public List<Vector2Int> AttackDecayMs = new List<Vector2Int>();
    public List<AnimationCurve> AttackResponses = new List<AnimationCurve>();
    public List<AnimationCurve> DecayResponses = new List<AnimationCurve>();
    public List<float> NoiseIntensities = new List<float>();
    public List<float> SecondaryNoiseIntensities = new List<float>();
    public List<float> NoiseDamps = new List<float>();
    public List<bool> Triggers = new List<bool>();
    public List<bool> NoteOffs = new List<bool>();

    public float NoiseAddition = 0f;

    public float SecondaryCurveInterpolation1 = 0f;
    public float SecondaryCurveInterpolation2 = 0f;

    void Start()
    {
        Instance = this;

        for (int i = 0; i < WeightCurves.Count; i++)
        {
            HitPoints.Add(new HitPoint()
            {
                WeightCurve = WeightCurves[i],
                    SecondaryWeightCurve = SecondaryWeightCurves[i]
            });
        }

        BaseNoiseValues = new Vector2[ParticleCount];
    }

    void Update()
    {
        CheckVariables();
        UpdateHitPointVaraibles();
        HitPoints.ForEach(hp => hp.UpdateEnvelope());

        ParticleSystems.ForEach(particleSystem =>
        {
            if (StartSystem)
            {
                StartingSystem = true;
                StartSystemTime = Time.time;
                ParticleSystems.ForEach(ps =>
                {
                    ps.Play();
                    ps.Emit(ParticleCount);
                });
                StartSystem = false;
            }
            if (StartingSystem)
            {
                var position = (Time.time - StartSystemTime) / StartSystemLength;
                if (position >= 1)
                {
                    position = 1;
                    StartingSystem = false;
                }
                var trailModule = particleSystem.trails;
                trailModule.colorOverTrail = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, position));
            }

            if (StopSystem)
            {
                StoppingSystem = true;
                StopSystemTime = Time.time;
                StopSystem = false;
            }
            if (StoppingSystem)
            {
                var position = (Time.time - StopSystemTime) / StopSystemLength;
                if (position >= 1)
                {
                    StoppingSystem = false;
                    ParticleSystems.ForEach(ps =>
                    {
                        var trailModule = ps.trails;
                        trailModule.colorOverTrail = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, 0));
                        ps.Stop();
                    });
                }
                else
                {
                    var trailModule = particleSystem.trails;
                    trailModule.colorOverTrail = new ParticleSystem.MinMaxGradient(new Color(1, 1, 1, 1 - position));
                }
            }

            var main = particleSystem.main;
            main.maxParticles = ParticleCount;

            var particles = new ParticleSystem.Particle[ParticleCount];

            particleSystem.GetParticles(particles);

            var subAngle = (2 * Mathf.PI) / ParticleCount;

            CurrentMaxRadius = InitialRadius;

            for (int i = 0; i < ParticleCount; i++)
            {
                var angle = subAngle * i;
                var x = Mathf.Cos(angle) * InitialRadius;
                var y = Mathf.Sin(angle) * InitialRadius;

                var curveOffset = 0f;

                foreach (var hitPoint in HitPoints)
                {
                    var index = HitPoints.IndexOf(hitPoint);

                    var weight = hitPoint.WeightCurve.Evaluate((float) i / ParticleCount);

                    var secondaryWeight = hitPoint.SecondaryWeightCurve.Evaluate((float) i / ParticleCount);

                    var interpolatedWeight = Mathf.Lerp(weight, secondaryWeight, SecondaryCurveInterpolation1);
                    if (index == 6 || index == 5)
                        interpolatedWeight = Mathf.Lerp(weight, secondaryWeight, SecondaryCurveInterpolation2);

                    curveOffset += hitPoint.CurveIntensity * interpolatedWeight;

                    // Apply individual hit noise
                    // Create the value
                    var newNoiseValue = (Random.value * 2) - 1;

                    var noiseIntensity = hitPoint.NoiseIntensity;
                    var secondaryNoiseIntensity = hitPoint.SecondaryNoiseIntensity;

                    var interpolatedNoiseIntensity = Mathf.Lerp(noiseIntensity, secondaryNoiseIntensity, SecondaryCurveInterpolation1);
                    if (index == 6 || index == 5)
                        interpolatedNoiseIntensity = Mathf.Lerp(noiseIntensity, secondaryNoiseIntensity, SecondaryCurveInterpolation2);

                    hitPoint.NoiseValues.y = (newNoiseValue * (hitPoint.CurveIntensity * weight)) * interpolatedNoiseIntensity;
                    // Damp it
                    hitPoint.NoiseValues.x =
                        hitPoint.NoiseValues.x + (hitPoint.NoiseValues.y - hitPoint.NoiseValues.x) / hitPoint.NoiseDamping;

                    // Add it to the curve offset
                    curveOffset += hitPoint.NoiseValues.x;
                }

                var pos = particles[i].position = new Vector2(x + (x * curveOffset), y + (y * curveOffset));

                var radius = GetDistanceBetweenPoints(0, 0, pos.x, pos.y);
                if (radius > CurrentMaxRadius) CurrentMaxRadius = radius;

                // Apply base circle noise
                if (EnableBaseNoise)
                {
                    // Create a new noise value between -1 and 1
                    var newNoiseValue = (Random.value * 2) - 1;
                    BaseNoiseValues[i].y = newNoiseValue;

                    // Damp it's movement
                    BaseNoiseValues[i].x =
                        BaseNoiseValues[i].x + (BaseNoiseValues[i].y - BaseNoiseValues[i].x) / BaseNoiseDamping;

                    var noisyX = pos.x + ((BaseNoiseValues[i].x * pos.x) * (BaseNoiseIntensity + NoiseAddition));
                    var noisyY = pos.y + ((BaseNoiseValues[i].x * pos.y) * (BaseNoiseIntensity + NoiseAddition));

                    pos = particles[i].position = new Vector2(noisyX, noisyY);
                }

                if (CloseCircle)
                {
                    if (i == ParticleCount - 1)
                    {
                        pos = particles[i].position = particles[i - 1].position;
                    }
                }
            }

            particleSystem.SetParticles(particles, ParticleCount);

            if (ExpandToRain)
            {
                var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.maxParticleSize = 0.003f;
                var trails = particleSystem.trails;
                trails.widthOverTrail = new ParticleSystem.MinMaxCurve(trails.widthOverTrail.constant - 0.01f);
                InitialRadius = InitialRadius + 0.05f;
                if (InitialRadius > 4)
                {
                    ExpandToRain = false;
                }
            }
        });

        if (Mathf.Abs(colourPosition.x - colourPosition.y) > Mathf.Epsilon)
        {
            colourPosition.x = colourPosition.x + (colourPosition.y - colourPosition.x) / ColourChangeDamping;
            var colourModule = ParticleSystems[1].colorOverLifetime;
            colourModule.enabled = true;
            var currentColour = Color.Lerp(DefaultColourTint, RecordColourTint, colourPosition.x);
            colourModule.color = currentColour;
        }
    }

    void CheckVariables()
    {
        if (BaseNoiseDamping < 1) BaseNoiseDamping = 1;
    }

    void UpdateHitPointVaraibles()
    {
        for (int i = 0; i < HitPoints.Count; i++)
        {
            HitPoints[i].WeightCurve = WeightCurves[i];
            HitPoints[i].AttackMs = AttackDecayMs[i].x;
            HitPoints[i].DecayMs = AttackDecayMs[i].y;
            HitPoints[i].AttackResponse = AttackResponses[i];
            HitPoints[i].DecayResponse = DecayResponses[i];
            HitPoints[i].NoiseIntensity = NoiseIntensities[i];
            HitPoints[i].SecondaryNoiseIntensity = SecondaryNoiseIntensities[i];
            HitPoints[i].NoiseDamping = NoiseDamps[i];
            HitPoints[i].NoteOn = Triggers[i];
            Triggers[i] = false;
            HitPoints[i].NoteOff = NoteOffs[i];
            NoteOffs[i] = false;
        }
    }

    public Color DefaultColourTint = Color.white;
    public Color RecordColourTint = Color.red;
    public float ColourChangeDamping = 4f;
    Vector2 colourPosition = new Vector2(0, 0);
    public void ToggleRecordColour()
    {
        if (colourPosition.y == 1)
            colourPosition.y = 0;
        else if (colourPosition.y == 0)
            colourPosition.y = 1;
    }

    public float GetDistanceBetweenPoints(float x1, float y1, float x2, float y2)
    {
        var xDiff = x2 - x1;
        var yDiff = y2 - y1;
        return Mathf.Sqrt(Mathf.Pow(xDiff, 2) + (Mathf.Pow(yDiff, 2)));
    }

    public enum EnvelopeState
    {
        Off,
        Attack,
        Decay
    }

    public class HitPoint
    {
        bool noteOn;
        public bool NoteOn;
        public bool NoteOff;
        public int AttackMs = 100;
        public int DecayMs = 500;
        public AnimationCurve AttackResponse = AnimationCurve.Linear(0, 0, 1, 1);
        public AnimationCurve DecayResponse = AnimationCurve.Linear(0, 1, 1, 0);

        private int EnvelopeStartTime;
        public EnvelopeState CurrentEnvelopeState;
        public float CurrentEnvelopeValue = 0f;

        public float CurveIntensity = 0f;
        public float IntensityMultiplier = 1f;

        public AnimationCurve WeightCurve = AnimationCurve.Linear(0, 1, 1, 0);
        public AnimationCurve SecondaryWeightCurve = AnimationCurve.Linear(0, 1, 1, 0);

        public float NoiseIntensity = 1f;
        public float SecondaryNoiseIntensity = 1f;
        public float NoiseDamping = 2f;
        public Vector2 NoiseValues = new Vector2(0f, 0f);

        public void UpdateEnvelope()
        {
            if (NoteOn)
            {
                // Start time = current time in Ms
                EnvelopeStartTime = GetCurrentMs();
                // set state to attack
                CurrentEnvelopeState = EnvelopeState.Attack;
                NoteOn = false;
            }
            else if (NoteOff)
            {
                EnvelopeStartTime = GetCurrentMs();
                CurrentEnvelopeState = EnvelopeState.Decay;
                NoteOff = false;
            }

            switch (CurrentEnvelopeState)
            {
                case EnvelopeState.Attack:
                    {
                        var envelopeTime = (GetCurrentMs() - EnvelopeStartTime) / (float) AttackMs;
                        if (envelopeTime >= 1)
                        {
                            envelopeTime = 1;
                            CurrentEnvelopeState = EnvelopeState.Off;
                        }
                        CurrentEnvelopeValue = AttackResponse.Evaluate(envelopeTime);
                        break;
                    }
                case EnvelopeState.Decay:
                    {
                        var envelopeTime = (GetCurrentMs() - EnvelopeStartTime) / (float) DecayMs;
                        if (envelopeTime >= 1)
                        {
                            envelopeTime = 1;
                            CurrentEnvelopeState = EnvelopeState.Off;
                            EnvelopeStartTime = 0;
                        }
                        CurrentEnvelopeValue = DecayResponse.Evaluate(envelopeTime);
                        break;
                    }
            }

            CurveIntensity = CurrentEnvelopeValue * IntensityMultiplier;
        }

        int GetCurrentMs()
        {
            return Mathf.RoundToInt(Time.time * 1000);
        }
    }
}