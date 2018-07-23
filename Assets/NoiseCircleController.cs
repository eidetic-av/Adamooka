using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoiseCircleController : MonoBehaviour
{
    public static NoiseCircleController Instance;
    public ParticleSystem ParticleSystem;

    public float InitialRadius = 1.5f;
    public float CurrentMaxRadius = 1.5f;

    public int ParticleCount = 50;

    public List<HitPoint> HitPoints = new List<HitPoint>();

    public List<AnimationCurve> WeightCurves = new List<AnimationCurve>();
    public List<Vector2Int> AttackDecayMs = new List<Vector2Int>();
    public List<AnimationCurve> AttackResponses = new List<AnimationCurve>();
    public List<AnimationCurve> DecayResponses = new List<AnimationCurve>();
    public List<bool> Triggers = new List<bool>();

    void Start()
    {
        Instance = this;

        foreach(var curve in WeightCurves) {
            HitPoints.Add(new HitPoint() {
                WeightCurve = curve
            });
        }

        ParticleSystem.Emit(ParticleCount);
    }

    void Update()
    {
        UpdateHitPointVaraibles();
        UpdateHitPointEnvelopes();

        var main = ParticleSystem.main;
        main.maxParticles = ParticleCount;

        var particles = new ParticleSystem.Particle[ParticleCount];

        ParticleSystem.GetParticles(particles);

        if (ParticleSystem.particleCount != ParticleCount)
        {
            ParticleSystem.Clear();
            ParticleSystem.Emit(ParticleCount);
        }

        var subAngle = (2 * Mathf.PI) / ParticleCount;

        CurrentMaxRadius = InitialRadius;

        for (int i = 0; i < ParticleCount; i++)
        {
            var angle = subAngle * i;
            var x = Mathf.Cos(angle) * InitialRadius;
            var y = Mathf.Sin(angle) * InitialRadius;

            var curveOffset = 0f;

            foreach(var hitPoint in HitPoints) {
                var weight = hitPoint.WeightCurve.Evaluate((float)i / ParticleCount);
                curveOffset += hitPoint.CurveIntensity * weight;
            }

            var pos = particles[i].position = new Vector2(x + (x * curveOffset), y + (y * curveOffset));

            var radius = GetDistanceBetweenPoints(0, 0, pos.x, pos.y);
            if (radius > CurrentMaxRadius) CurrentMaxRadius = radius;
        }

        ParticleSystem.SetParticles(particles, ParticleCount);
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

    public float GetDistanceBetweenPoints(float x1, float y1, float x2, float y2) {
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

        public HitPoint()
        {
            NoiseCircleController.Instance.Triggers.Add(false);
        }

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
