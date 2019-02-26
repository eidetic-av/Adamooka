using System.Collections;
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
    public List<Vector2Int> AttackDecayMs = new List<Vector2Int>();
    public List<AnimationCurve> AttackResponses = new List<AnimationCurve>();
    public List<AnimationCurve> DecayResponses = new List<AnimationCurve>();
    public List<float> NoiseIntensities = new List<float>();
    public List<float> NoiseDamps = new List<float>();
    public List<bool> Triggers = new List<bool>();

    void Start()
    {
        Instance = this;

        foreach (var curve in WeightCurves)
        {
            HitPoints.Add(new HitPoint()
            {
                WeightCurve = curve
            });
        }

        BaseNoiseValues = new Vector2[ParticleCount];
    }

    void Update()
    {
        CheckVariables();
        UpdateHitPointVaraibles();
        UpdateHitPointEnvelopes();

        ParticleSystems.ForEach(particleSystem =>
        {
            if (StartSystem)
            {
                StartingSystem = true;
                StartSystemTime = Time.time;
                ParticleSystems.ForEach(ps => ps.Emit(ParticleCount));
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
                var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.trailMaterial.SetColor("_Color", new Color(position, position, position, 1));
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
                    position = 1;
                    StoppingSystem = false;
                }
                var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.trailMaterial.SetColor("_Color", new Color(1 - position, 1 - position, 1 - position, 1));
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
                    var weight = hitPoint.WeightCurve.Evaluate((float)i / ParticleCount);
                    curveOffset += hitPoint.CurveIntensity * weight;

                    // Apply individual hit noise
                    // Create the value
                    var newNoiseValue = (Random.value * 2) - 1;
                    hitPoint.NoiseValues.y = (newNoiseValue * (hitPoint.CurveIntensity * weight)) * hitPoint.NoiseIntensity;
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

                    var noisyX = pos.x + ((BaseNoiseValues[i].x * pos.x) * BaseNoiseIntensity);
                    var noisyY = pos.y + ((BaseNoiseValues[i].x * pos.y) * BaseNoiseIntensity);

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
            HitPoints[i].NoiseDamping = NoiseDamps[i];
            HitPoints[i].Trigger = Triggers[i];
        }
    }

    void UpdateHitPointEnvelopes()
    {
        foreach (var hitPoint in HitPoints)
        {
            if (hitPoint.Trigger)
            {
                hitPoint.StartEnvelope();
                Triggers[HitPoints.IndexOf(hitPoint)] = false;
            }
            else if (hitPoint.CurrentEnvelopeState != EnvelopeState.Off)
            {
                hitPoint.UpdateEnvelope();
            }
        }
    }

    public float GetDistanceBetweenPoints(float x1, float y1, float x2, float y2)
    {
        var xDiff = x2 - x1;
        var yDiff = y2 - y1;
        return Mathf.Sqrt(Mathf.Pow(xDiff, 2) + (Mathf.Pow(yDiff, 2)));
    }

    public enum EnvelopeState
    {
        Off, Attack, Decay
    }

    public class HitPoint
    {
        public bool Trigger;
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

        public float NoiseIntensity = 1f;
        public float NoiseDamping = 2f;
        public Vector2 NoiseValues = new Vector2(0f, 0f);

        public void StartEnvelope()
        {
            // Start time = current time in Ms
            EnvelopeStartTime = GetCurrentMs();

            // set state to attack
            CurrentEnvelopeState = EnvelopeState.Attack;

            Trigger = false;
        }

        public void UpdateEnvelope()
        {
            switch (CurrentEnvelopeState)
            {
                case EnvelopeState.Attack:
                    {
                        var envelopeTime = (GetCurrentMs() - EnvelopeStartTime) / (float)AttackMs;
                        if (envelopeTime >= 1)
                        {
                            envelopeTime = 1;
                            CurrentEnvelopeState = EnvelopeState.Decay;
                            EnvelopeStartTime = GetCurrentMs();
                        }
                        CurrentEnvelopeValue = AttackResponse.Evaluate(envelopeTime);
                        break;
                    }
                case EnvelopeState.Decay:
                    {
                        var envelopeTime = (GetCurrentMs() - EnvelopeStartTime) / (float)DecayMs;
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
