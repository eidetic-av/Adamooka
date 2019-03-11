using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RuntimeInspectorNamespace;
using Eidetic.Andamooka;
using Eidetic.Utility;
using Eidetic.Unity.Runtime;
using Eidetic.Unity.Utility;
using Midi;

[System.Serializable]
public class RodController : MidiTriggerController
{
    //
    // Runtime control properties
    //
    public bool AirSticksControlActive { get; set; } = false;
    public SerializableVector3 PositionScale { get; set; }
        = new SerializableVector3(1, 1, 1);
    public SerializableVector3 LeftHandOffset { get; set; }
        = new SerializableVector3(0, 0, 0);
    public SerializableVector3 RightHandOffset { get; set; }
        = new SerializableVector3(0, 0, 0);

    public AirSticks.VelocityMapping VelocityToSpawnSpeed { get; set; }
        = new AirSticks.VelocityMapping(AirSticks.Hand.Both)
        {
            MinimumValue = 1f,
            MaximumValue = 1f
        };
    public AirSticks.MotionMapping HandScale { get; set; }
        = new AirSticks.MotionMapping(AirSticks.Hand.Both)
        {
            Input = AirSticks.ControlType.Motion.RotationX,
            MinimumValue = 0.2f,
            MaximumValue = 1f
        };
    public AirSticks.VelocityMapping VelocityToScale { get; set; }
        = new AirSticks.VelocityMapping(AirSticks.Hand.Both)
        {
            MinimumValue = 0.5f,
            MaximumValue = 1f
        };
    public AirSticks.MotionMapping HandNoiseLeft { get; set; }
        = new AirSticks.MotionMapping(AirSticks.Hand.Left)
        {
            Input = AirSticks.ControlType.Motion.PositionY,
            MinimumValue = 0f,
            MaximumValue = 0.05f,
            ClampInputRange = true
        };
    public AirSticks.MotionMapping HandNoiseRight { get; set; }
        = new AirSticks.MotionMapping(AirSticks.Hand.Right)
        {
            Input = AirSticks.ControlType.Motion.PositionY,
            MinimumValue = 0f,
            MaximumValue = 0.05f,
            ClampInputRange = true
        };
    public AirSticks.MotionMapping OrbitLeft { get; set; }
        = new AirSticks.MotionMapping(AirSticks.Hand.Left)
        {
            Input = AirSticks.ControlType.Motion.PositionY,
            MinimumValue = 0f,
            MaximumValue = 0f
        };
    public AirSticks.MotionMapping OrbitRight { get; set; }
        = new AirSticks.MotionMapping(AirSticks.Hand.Right)
        {
            Input = AirSticks.ControlType.Motion.PositionY,
            MinimumValue = 0f,
            MaximumValue = 0f
        };

    float naturalMotionAmount = 0.01f;
    public float NaturalMotionAmount
    {
        get
        {
            return naturalMotionAmount;
        }
        set
        {
            LeftHand.SetNoiseStrength(value);
            RightHand.SetNoiseStrength(value);
            naturalMotionAmount = value;
        }
    }

    public override Channel MidiChannel { get; set; } = Channel.Channel12;
    public PitchRange ColorShiftRange { get; set; } = new PitchRange(Pitch.C1, Pitch.E1);
    public List<SerializableVector2> ColorVectors { get; set; } = new List<SerializableVector2>() {
        new SerializableVector2(1f, 0.5f),
        new SerializableVector2(1f, 0.5f),
        new SerializableVector2(1f, 0.5f),
        new SerializableVector2(1f, 0.5f),
        new SerializableVector2(1f, 0.5f)
    };

    public List<SerializableVector2> ColorTiling { get; set; } = new List<SerializableVector2>() {
        new SerializableVector2(0.4f, 0),
        new SerializableVector2(0.4f, 0),
        new SerializableVector2(0.4f, 0),
        new SerializableVector2(0.4f, 0),
        new SerializableVector2(0.4f, 0)
    };

    float colorShiftDamping = 30f;
    public float ColorShiftDamping
    {
        get
        {
            return colorShiftDamping;
        }
        set
        {
            if (value <= 1)
                colorShiftDamping = 1;
            else
                colorShiftDamping = value;
        }
    }
    int ActiveColorVectorIndex = 0;
    public override List<MidiTrigger> Triggers { get; set; }

    public bool ConstantEmission = false;

    Vector2 Opacity = new Vector2(0f, 0f);
    public float FadeOutDamping { get; set; } = 5f;

    //
    // Initialisation stuff
    //
    [SerializeField]
    ParticleSystem LeftHand;
    [SerializeField]
    ParticleSystem RightHand;
    [SerializeField]
    ParticleSystem Ribbon;

    void Start()
    {
        InitialiseMidi();

        Triggers = new List<MidiTrigger>()
        {
            new NoteOnTrigger(SelectActiveColorVector)
        };

        AirSticks.Left.NoteOn += (velocity)
            => Spawn(AirSticks.Hand.Left, velocity);
        AirSticks.Right.NoteOn += (velocity)
            => Spawn(AirSticks.Hand.Right, velocity);

        AirSticks.Left.NoteOff += ()
            => TurnOff(AirSticks.Hand.Left);
        AirSticks.Right.NoteOff += ()
            => TurnOff(AirSticks.Hand.Right);
    }

    //
    // Runtime loop
    //
    void Update()
    {
        if (ConstantEmission || AirSticks.Left.NoteIsOn || AirSticks.Right.NoteIsOn)
        {
            LeftHand.transform.position =
                AirSticks.Left.Position * PositionScale + LeftHandOffset;
            RightHand.transform.position =
                AirSticks.Right.Position * PositionScale + RightHandOffset;

            // Airstick to scale mapping
            LeftHand.transform.localScale = Vector3.one *
                HandScale.GetOutput(AirSticks.Hand.Left);
            RightHand.transform.localScale = Vector3.one *
                HandScale.GetOutput(AirSticks.Hand.Right);

            // Velocity to scale mapping
            LeftHand.transform.localScale = LeftHand.transform.localScale
                .Multiply(Vector3.one * AirSticks.Left.Velocity.Map(VelocityToScale));
            RightHand.transform.localScale = RightHand.transform.localScale
                .Multiply(Vector3.one * AirSticks.Right.Velocity.Map(VelocityToScale));

            // Orbit mapping
            var leftVelocityModule = LeftHand.velocityOverLifetime;
            leftVelocityModule.orbitalX = new ParticleSystem.MinMaxCurve(OrbitLeft.Output);
            leftVelocityModule.orbitalY = new ParticleSystem.MinMaxCurve(OrbitLeft.Output);
            leftVelocityModule.orbitalZ = new ParticleSystem.MinMaxCurve(OrbitLeft.Output);
            var rightVelocityModule = RightHand.velocityOverLifetime;
            rightVelocityModule.orbitalX = new ParticleSystem.MinMaxCurve(OrbitRight.Output);
            rightVelocityModule.orbitalY = new ParticleSystem.MinMaxCurve(OrbitRight.Output);
            rightVelocityModule.orbitalZ = new ParticleSystem.MinMaxCurve(OrbitRight.Output);

            // Noise mapping
            ApplyNoiseLeft();
            ApplyNoiseRight();

            // Color vector change
            Ribbon.GetTrailMaterial().SetTextureOffset("_MainTex", ColorVectors[ActiveColorVectorIndex]);
            Ribbon.GetTrailMaterial().SetTextureScale("_MainTex", ColorTiling[ActiveColorVectorIndex]);

            // Opacity
            if (Opacity.x != 0)
            {
                Opacity.x = Opacity.x + (Opacity.y - Opacity.x) / FadeOutDamping;
                if (Opacity.x <= 0.005f)
                {
                    Opacity.x = 0;
                    LeftHand.Clear();
                    LeftHand.Stop();
                    RightHand.Clear();
                    RightHand.Stop();
                    Ribbon.Clear();
                    Ribbon.Stop();
                }
                var ribbonColor = Ribbon.GetTrailMaterial().GetColor("_TintColor");
                Ribbon.GetTrailMaterial().SetColor("_TintColor",
                    new Color(ribbonColor.r, ribbonColor.g, ribbonColor.b, Opacity.x));
                var leftColor = LeftHand.GetMaterial().GetColor("_Color");
                LeftHand.GetMaterial().SetColor("_Color",
                    new Color(leftColor.r, leftColor.g, leftColor.b, Opacity.x));
                var rightColor = RightHand.GetMaterial().GetColor("_Color");
                RightHand.GetMaterial().SetColor("_Color",
                    new Color(leftColor.r, leftColor.g, leftColor.b, Opacity.x));
            }
        }
    }

    //
    // Actions
    //

    void Spawn(AirSticks.Hand hand, int velocity)
    {
        if (AirSticksControlActive)
        {
            Opacity = new Vector2(1f, 1f);
            if (!ConstantEmission)
            {
                // if in regular mode, the right hand turns the system on and off,
                // so if its the right hand, start the system
                if (hand == AirSticks.Hand.Right)
                {
                    if (!GetSystem(AirSticks.Hand.Left).isPlaying)
                    {   // if the opposing system is not already playing, restart it
                        GetSystem(AirSticks.Hand.Left).SetSimulationSpeed((velocity / 127f).Map(VelocityToSpawnSpeed));
                        GetSystem(AirSticks.Hand.Left).Restart();
                    }
                    MonoUtility.StartDelayedAction(this, () =>
                    {
                        GetSystem(AirSticks.Hand.Right).SetSimulationSpeed((velocity / 127f).Map(VelocityToSpawnSpeed));
                        GetSystem(AirSticks.Hand.Right).Restart();
                    }, 0.05f);
                }
                else if (hand == AirSticks.Hand.Left)
                {
                    // if the left hand is causing the note on, restart the left system
                    // to swap the colours ONLY if the right one is already on
                    if (AirSticks.Right.NoteIsOn)
                    {
                        GetSystem(AirSticks.Hand.Left).SetSimulationSpeed((velocity / 127f).Map(VelocityToSpawnSpeed));
                        GetSystem(AirSticks.Hand.Left).Restart();
                    }
                }
            }
            else
            {   // if in constant emission mode
                if (!GetOtherSystem(hand).isPlaying)
                {   // only restart the opposing system if its not already playing
                    GetOtherSystem(hand).SetSimulationSpeed((velocity / 127f).Map(VelocityToSpawnSpeed));
                    GetOtherSystem(hand).Restart();
                }
                MonoUtility.StartDelayedAction(this, () =>
                {
                    GetSystem(hand).SetSimulationSpeed((velocity / 127f).Map(VelocityToSpawnSpeed));
                    GetSystem(hand).Restart();
                }, 0.05f);
            }
        }
    }

    void TurnOff(AirSticks.Hand hand)
    {
        if (!ConstantEmission)
        {
            // if we're in regular mode, only turn off with the right hand
            if (hand == AirSticks.Hand.Right)
            {
                Opacity.x = 1f;
                Opacity.y = 0f;
            }
        }
        else
        {
            // if we're in constant emission mode, both hands have to be off to turn the system off
            if (!AirSticks.Left.NoteIsOn && !AirSticks.Right.NoteIsOn)
            {
                Opacity.x = 1f;
                Opacity.y = 0f;
            }
        }
    }

    void SelectActiveColorVector(Pitch pitch, int velocity) =>
        ActiveColorVectorIndex = Mathf.RoundToInt(pitch.Normalize(ColorShiftRange).Map(0, 1, 0, ColorVectors.Count - 1));

    //
    // Inner methods
    //
    ParticleSystem GetSystem(AirSticks.Hand hand)
    {
        if (hand == AirSticks.Hand.Left)
            return this.LeftHand;
        else if (hand == AirSticks.Hand.Right)
            return this.RightHand;
        else throw new System.ArgumentException();
    }
    ParticleSystem GetOtherSystem(AirSticks.Hand hand)
    {
        if (hand == AirSticks.Hand.Left)
            return this.RightHand;
        else if (hand == AirSticks.Hand.Right)
            return this.LeftHand;
        else throw new System.ArgumentException();
    }

    Vector3[] LeftPositionsPreNoise;
    Vector3[] LastLeftRandomVectors;
    Vector3[] RightPositionsPreNoise;
    Vector3[] LastRightRandomVectors;

    void ApplyNoiseLeft()
    {
        var multiplier = HandNoiseLeft.Output;
        var system = GetSystem(AirSticks.Hand.Left);
        var particles = new ParticleSystem.Particle[system.particleCount];
        system.GetParticles(particles);

        // Remove the noise of the previous frame
        // Otherwise the particles lose their shape
        LeftPositionsPreNoise = particles.Select((particle, i) =>
        {
            if (LastLeftRandomVectors != null && i < LastLeftRandomVectors.Length)
                particle.position -= LastLeftRandomVectors[i];
            return particle.position;
        }).ToArray();

        var randomVectors = new Vector3[system.particleCount];

        for (int i = 0; i < particles.Length; i++)
        {
            randomVectors[i] = GenerateRandomVector3(multiplier) * HandScale.GetOutput(AirSticks.Hand.Left);
            particles[i].position = LeftPositionsPreNoise[i] + randomVectors[i];
        }

        LastLeftRandomVectors = randomVectors;

        system.SetParticles(particles, particles.Length);
    }
    void ApplyNoiseRight()
    {
        var multiplier = HandNoiseRight.Output;
        var system = GetSystem(AirSticks.Hand.Right);
        var particles = new ParticleSystem.Particle[system.particleCount];
        system.GetParticles(particles);

        // Remove the noise of the previous frame
        // Otherwise the particles lose their shape
        RightPositionsPreNoise = particles.Select((particle, i) =>
        {
            if (LastRightRandomVectors != null && i < LastRightRandomVectors.Length)
                particle.position -= LastRightRandomVectors[i];
            return particle.position;
        }).ToArray();

        var randomVectors = new Vector3[system.particleCount];

        for (int i = 0; i < particles.Length; i++)
        {
            randomVectors[i] = GenerateRandomVector3(multiplier) * HandScale.GetOutput(AirSticks.Hand.Right);
            particles[i].position = RightPositionsPreNoise[i] + randomVectors[i];
        }

        LastRightRandomVectors = randomVectors;

        system.SetParticles(particles, particles.Length);
    }

    Vector3 GenerateRandomVector3(float multiplier = 1f)
    {
        return new Vector3(
            UnityEngine.Random.Range(-1f, 1f) * multiplier,
            UnityEngine.Random.Range(-1f, 1f) * multiplier,
            UnityEngine.Random.Range(-1f, 1f) * multiplier
        );
    }
}

// Todo: Move this somewhere!
public static class MonoUtility
{
    public static void StartDelayedAction(MonoBehaviour monoBehaviour, Action action, float delayTime)
    {
        monoBehaviour.StartCoroutine(DelayedAction(action, delayTime));
    }

    public static IEnumerator DelayedAction(Action action, float delayTime)
    {
        yield return new WaitForSeconds(delayTime);
        action.Invoke();
    }
}
