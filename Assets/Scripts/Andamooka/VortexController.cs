using System;
using Eidetic.Andamooka;
using Eidetic.Unity.Runtime;
using Eidetic.Utility;
using Midi;
using System.Collections.Generic;
using UnityEngine;
using RuntimeInspectorNamespace;

[System.Serializable]
public class VortexController : MidiTriggerController
{
    public override Channel MidiChannel { get; set; } = Channel.Channel14;

    public float RotateAngle { get; set; } = 12f;
    float rotationDamping = 2f;
    public float RotationDamping
    {
        get
        {
            return rotationDamping;
        }
        set
        {
            if (value <= 1)
                rotationDamping = 1;
            else
                rotationDamping = value;
        }
    }
    public float InnerRadius { get; set; } = .8f;
    public float OuterRadius { get; set; } = 1.3f;
    public int BaseEmissionCount { get; set; } = 3;
    public PitchRange PitchRange { get; set; } = new PitchRange(Pitch.C1, Pitch.F1);
    public float PitchAngleOffset { get; set; } = 0f;
    public int PitchEmissionCountAddition { get; set; } = 2;
    float particleLifetime = 1f;
    public float ParticleLifetime
    {
        get
        {
            return particleLifetime;
        }
        set
        {
            var mainModule = ParticleSystem.main;
            mainModule.startLifetime = value;
            particleLifetime = value;
        }
    }
    int maxParticles = 500;
    public int MaxParticles
    {
        get
        {
            return maxParticles;
        }
        set
        {
            var mainModule = ParticleSystem.main;
            mainModule.maxParticles = value;
            maxParticles = value;
        }
    }

    public List<SerializableColor> Colors { get; set; } = new List<SerializableColor>() { Color.white };

    public AirSticks.MotionMapping ParticleThickness { get; set; }
        = new AirSticks.MotionMapping(AirSticks.Hand.Left)
        {
            Input = AirSticks.ControlType.Motion.RotationX,
            MinimumValue = 0.3f,
            MaximumValue = 2f,
            InputRange = new SerializableVector2(-1, 1)
        };

    public AirSticks.MotionMapping ZoomSpeed { get; set; }
        = new AirSticks.MotionMapping(AirSticks.Hand.Left)
        {
            Input = AirSticks.ControlType.Motion.PositionZ,
            MinimumValue = 0f,
            MaximumValue = 0f,
            FlipAxis = true
        };

    public override List<MidiTrigger> Triggers { get; set; }

    public bool Active = false;
    ParticleSystem ParticleSystem;
    private Vector2 Rotation = Vector2.zero;

    void Start()
    {
        InitialiseMidi();
        Triggers = new List<MidiTrigger>()
        {
            new NoteOnTrigger(RotateSystem)
        };
        ParticleSystem = gameObject.GetComponentInChildren<ParticleSystem>();
    }

    void Update()
    {
        if (ParticleSystem.IsAlive())
        {
            var mainModule = ParticleSystem.main;
            mainModule.startSpeed = ZoomSpeed.Output;

            var trailModule = ParticleSystem.trails;
            trailModule.widthOverTrail = new ParticleSystem.MinMaxCurve(ParticleThickness.Output);

            Rotation.x = Rotation.x + (Rotation.y - Rotation.x) / RotationDamping;
            var euler = ParticleSystem.transform.rotation.eulerAngles;
            euler.z = Rotation.x;
            var quaternion = Quaternion.Euler(euler);
            ParticleSystem.transform.SetPositionAndRotation(Vector3.zero, quaternion);
        }
    }

    [RuntimeInspectorButton("Toggle HiHat System", false, ButtonVisibility.InitializedObjects)]
    public void ToggleHiHatSystem() => Active = !Active;

    [RuntimeInspectorButton("0: Rotate HiHat Vortex", false, ButtonVisibility.InitializedObjects)]
    void RotateSystem() => RotateSystem(Pitch.F2, 127);
    void RotateSystem(Pitch pitch, int velocity)
    {
        if (Active)
        {
            Rotation.y += RotateAngle;

            var pitchValue = pitch.Normalize(PitchRange);

            var shapeModule = ParticleSystem.shape;
            shapeModule.radius = pitchValue.Map(0f, 1f, InnerRadius, OuterRadius);

            var ringRotation = shapeModule.rotation;
            ringRotation.z = pitchValue.Map(0f, 1f, 0f, ((float)PitchRange.Size) * PitchAngleOffset);
            shapeModule.rotation = ringRotation;

            var mainModule = ParticleSystem.main;
            if (Colors.Count != 0)
            {
                var colorIndex = 0;
                if (PitchRange.GetPosition(pitch) >= Colors.Count)
                    colorIndex = Colors.Count - 1;
                else
                    colorIndex = PitchRange.GetPosition(pitch).Map(0, PitchRange.Size, 0, Colors.Count - 1);

                mainModule.startColor = new ParticleSystem.MinMaxGradient(Colors[colorIndex]);
            }

            var additionalEmission = ((PitchRange.GetPosition(pitch)) * PitchEmissionCountAddition);
            if (additionalEmission < 0) additionalEmission = 0;

            var emissionCount = BaseEmissionCount + additionalEmission;

            ParticleSystem.Emit(emissionCount);
        }
    }
}

// Todo: Move this!
[System.Serializable]
public class PitchRange
{
    public Pitch Minimum { get; set; }
    public Pitch Maximum { get; set; }
    public int Size => Maximum.NoteNumber() - Minimum.NoteNumber();
    public PitchRange(Pitch minimum, Pitch maximum)
    {
        Minimum = minimum;
        Maximum = maximum;
    }
    public int GetPosition(Pitch pitch) => pitch.NoteNumber() - Minimum.NoteNumber();
}

public static class PitchExtensions
{
    public static float Normalize(this Pitch pitch, PitchRange pitchRange, bool clamped = true)
    {
        if (clamped)
        {
            if (pitch.NoteNumber() < pitchRange.Minimum.NoteNumber())
                pitch = pitchRange.Minimum;
            else if (pitch.NoteNumber() > pitchRange.Maximum.NoteNumber())
                pitch = pitchRange.Maximum;
        }
        return ((float)pitch.NoteNumber()).Map((float)pitchRange.Minimum.NoteNumber(), (float)pitchRange.Maximum.NoteNumber(), 0f, 1f);
    }
    public static float Map(this float input, float minimumInput, float maximumInput, float minimumOutput, float maximumOutput)
    {
        return ((input - minimumInput) / (maximumInput - minimumInput)) * (maximumOutput - minimumOutput) + minimumOutput;
    }
    public static int Map(this int input, int minimumInput, int maximumInput, int minimumOutput, int maximumOutput)
    {
        return (int)Math.Round(((input - minimumInput) / (float)(maximumInput - minimumInput)) * (maximumOutput - minimumOutput) + minimumOutput);
    }
}

[System.Serializable]
public class SerializableColor
{
    public float[] colorStore = new float[4] { 1F, 1F, 1F, 1F };
    public Color Color
    {
        get { return new Color(colorStore[0], colorStore[1], colorStore[2], colorStore[3]); }
        set { colorStore = new float[4] { value.r, value.g, value.b, value.a }; }
    }

    //makes this class usable as Color, Color normalColor = mySerializableColor;
    public static implicit operator Color(SerializableColor instance)
    {
        return instance.Color;
    }

    //makes this class assignable by Color, SerializableColor myColor = Color.white;
    public static implicit operator SerializableColor(Color color)
    {
        return new SerializableColor { Color = color };
    }
}