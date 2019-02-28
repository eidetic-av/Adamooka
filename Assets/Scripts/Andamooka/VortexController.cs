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

    public float RotateAngle { get; set; } = 10f;
    float innerRadius = 2.4f;
    public float InnerRadius
    {
        get
        {
            return innerRadius;
        }
        set
        {
            var shapeModule = ParticleSystem.shape;
            shapeModule.radius = value;
            innerRadius = value;
        }
    }

    public int EmissionCount { get; set; } = 100;
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

    public float RotationDamping = 2f;
    private Vector2 Rotation = Vector2.zero;

    void Start()
    {
        InitialiseMidi();
        Triggers = new List<MidiTrigger>()
        {
            new MidiTrigger(Pitch.Any, RotateHats)
        };
        ParticleSystem = GameObject.Find("HiHatVortexParticles").GetComponent<ParticleSystem>();
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
    void RotateHats()
    {
        if (Active)
        {
            Rotation.y += RotateAngle;
            ParticleSystem.Emit(EmissionCount);
        }
    }

}