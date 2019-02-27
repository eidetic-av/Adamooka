using Eidetic.Andamooka;
using Eidetic.Unity.Runtime;
using Midi;
using System.Collections.Generic;
using UnityEngine;
using RuntimeInspectorNamespace;

[System.Serializable]
public class VortexController : MidiTriggerController
{
    public override Channel MidiChannel { get; set; } = Channel.Channel14;

    float hiHatRotateAngle = 10f;
    public float HiHatRotateAngle
    {
        get
        {
            return hiHatRotateAngle;
        }
        set
        {
            HiHatController.RotationIncrement = value;
            hiHatRotateAngle = value;
        }
    }
    float hiHatVortexRadius = 2.4f;
    public float HiHatVortexRadius
    {
        get
        {
            return hiHatVortexRadius;
        }
        set
        {
            var shapeModule = HiHatController.ParticleSystem.shape;
            shapeModule.radius = value;
            hiHatVortexRadius = value;
        }
    }

    public AirSticks.MotionMapping HiHatParticleThickness {get; set;}
        = new AirSticks.MotionMapping(AirSticks.Hand.Left)
        {
            Input = AirSticks.ControlType.Motion.RotationX,
            MinimumValue = 0.3f,
            MaximumValue = 2f,
            InputRange = new SerializableVector2(-1, 1)
        };

    public AirSticks.MotionMapping HiHatZoomSpeed {get; set;}
        = new AirSticks.MotionMapping(AirSticks.Hand.Left)
        {
            Input = AirSticks.ControlType.Motion.PositionZ,
            MinimumValue = 10f,
            MaximumValue = 10f,
            FlipAxis = true
        };

    public override List<MidiTrigger> Triggers { get; set; }

    CircleParticleController HiHatController;
    void Start()
    {
        InitialiseMidi();
        Triggers = new List<MidiTrigger>()
        {
            new MidiTrigger(Pitch.Any, RotateHats)
        };
        HiHatController = GameObject.Find("HiHatLayer").GetComponent<CircleParticleController>();
    }

    void Update()
    {
        var hiHatMainModule = HiHatController.ParticleSystem.main;
        hiHatMainModule.startSpeed = HiHatZoomSpeed.Output;
        
        var hihatTrailModule = HiHatController.ParticleSystem.trails;
        hihatTrailModule.widthOverTrail = new ParticleSystem.MinMaxCurve(HiHatParticleThickness.Output, 0.1f);

    }

    [RuntimeInspectorButton("Toggle HiHat System", false, ButtonVisibility.InitializedObjects)]
    public void ToggleHiHatSystem()
    {
        if (HiHatController.ParticleSystem.IsAlive())
            HiHatController.EmitConstantly = false;
        else
            HiHatController.EmitConstantly = true;
    }

    [RuntimeInspectorButton("0: Rotate HiHat Vortex", false, ButtonVisibility.InitializedObjects)]
    void RotateHats() => HiHatController.BangRotation = true;

}