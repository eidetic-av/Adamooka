using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RuntimeInspectorNamespace;
using Eidetic.Andamooka;
using Eidetic.Unity.Runtime;
using Eidetic.Unity.Utility;
using Midi;

[System.Serializable]
public class RingController : MidiTriggerController
{

    //
    // Runtime control properties
    //
    public bool ControlWithAirSticks { get; set; } = true;
    float ringScale = 0.8f;
    public float RingScale
    {
        get
        {
            return ringScale;
        }
        set
        {
            ringScale = value;
            CircleController.InitialRadius = ringScale;
            CircleController.CurrentMaxRadius = ringScale;
        }
    }
    public override Channel MidiChannel { get; set; } = Channel.Channel9;
    public override List<MidiTrigger> Triggers { get; set; }

    //
    // Initialisation
    //
    NoiseCircleController CircleController;
    void Start()
    {
        InitialiseMidi();
        CircleController = GameObject.Find("NoiseCircleController")
            .GetComponent<NoiseCircleController>();
        Triggers = new List<MidiTrigger>()
        {
            new MidiTrigger(Pitch.E2, KickA),
            new MidiTrigger(Pitch.ASharp2, KickB),
            new MidiTrigger(Pitch.C3, SnareA),
            new MidiTrigger(Pitch.D3, SnareB),
            new MidiTrigger(Pitch.GSharp2, HiHat)
        };
        NoteOffTriggers = new List<NoteOffTrigger>() {
            new NoteOffTrigger(Pitch.E2, KickAOff),
            new NoteOffTrigger(Pitch.ASharp2, KickBOff),
            new NoteOffTrigger(Pitch.C3, SnareAOff),
            new NoteOffTrigger(Pitch.D3, SnareBOff),
            new NoteOffTrigger(Pitch.GSharp2, HiHatOff)
        };
    }

    bool Active = false;
    [RuntimeInspectorButton("Toggle Active", false, ButtonVisibility.InitializedObjects)]
    public void ToggleActive()
    {
        Active = !Active;
        if (Active)
            CircleController.StartSystem = true;
        else
            CircleController.StopSystem = true;
    }

    [RuntimeInspectorButton("0: Kick A", false, ButtonVisibility.InitializedObjects)]
    public void KickA() =>
        CircleController.Triggers[4] = true;

    [RuntimeInspectorButton("0: Kick A Off", false, ButtonVisibility.InitializedObjects)]
    public void KickAOff() => CircleController.NoteOffs[4] = true;

    [RuntimeInspectorButton("1: Kick B", false, ButtonVisibility.InitializedObjects)]
    public void KickB() =>
        CircleController.Triggers[1] = true;

    [RuntimeInspectorButton("1: Kick B Off", false, ButtonVisibility.InitializedObjects)]
    public void KickBOff() =>
        CircleController.NoteOffs[1] = true;

    [RuntimeInspectorButton("2: Snare A", false, ButtonVisibility.InitializedObjects)]
    public void SnareA() =>
        CircleController.Triggers[6] = true;

    [RuntimeInspectorButton("2: Snare A Off", false, ButtonVisibility.InitializedObjects)]
    public void SnareAOff() =>
        CircleController.NoteOffs[6] = true;

    [RuntimeInspectorButton("3: Snare B", false, ButtonVisibility.InitializedObjects)]
    public void SnareB() =>
        CircleController.Triggers[5] = true;

    [RuntimeInspectorButton("3: Snare B Off", false, ButtonVisibility.InitializedObjects)]
    public void SnareBOff() =>
        CircleController.NoteOffs[5] = true;

    [RuntimeInspectorButton("4: Hi-Hat", false, ButtonVisibility.InitializedObjects)]
    public void HiHat() =>
        CircleController.Triggers[2] = true;

    [RuntimeInspectorButton("4: Hi-Hat Off", false, ButtonVisibility.InitializedObjects)]
    public void HiHatOff() =>
        CircleController.NoteOffs[2] = true;

    //
    // Drum synth
    //
    public bool DrumSynthActive = false;

    public AirSticks.MotionMapping SynthKickManipulation { get; set; }
        = new AirSticks.MotionMapping(AirSticks.Hand.Right)
        {
            Input = AirSticks.ControlType.Motion.PositionZ,
            MinimumValue = 1,
            MaximumValue = 1
        };

    public AirSticks.MotionMapping SynthSnareManipulation { get; set; }
        = new AirSticks.MotionMapping(AirSticks.Hand.Left)
        {
            Input = AirSticks.ControlType.Motion.PositionZ,
            MinimumValue = 1,
            MaximumValue = 1
        };

    bool DrumSynthKickIsOn = false;
    bool DrumSynthSnareIsOn = false;

    public void DrumSynthKickOn(int velocity)
    {
        if (DrumSynthActive)
        {
            CircleController.Triggers[7] = true;
            DrumSynthKickIsOn = true;
        }
    }

    public void DrumSynthKickOff()
    {
        if (DrumSynthActive)
        {
            CircleController.HitPoints[7].CurrentEnvelopeState =
                NoiseCircleController.EnvelopeState.Decay;
            DrumSynthKickIsOn = false;
        }
    }


    public void DrumSynthSnareOn(int velocity)
    {
        if (DrumSynthActive)
        {
            CircleController.Triggers[8] = true;
            DrumSynthSnareIsOn = true;
        }
    }

    public void DrumSynthSnareOff()
    {
        if (DrumSynthActive)
        {
            CircleController.HitPoints[8].CurrentEnvelopeState =
                NoiseCircleController.EnvelopeState.Decay;
            DrumSynthSnareIsOn = false;
        }
    }

    void Update()
    {
        if (DrumSynthActive)
        {
            if (DrumSynthKickIsOn)
            {
                CircleController.NoiseAddition = SynthKickManipulation.Output;
            }
            else if (DrumSynthSnareIsOn)
            {
                CircleController.NoiseAddition = SynthSnareManipulation.Output;
            }
            else
            {
                CircleController.NoiseAddition = 0;
            }
        }
    }

    public void ToggleDrumSynth()
    {
        if (!DrumSynthActive)
        {
            AirSticks.Left.NoteOn += DrumSynthKickOn;
            AirSticks.Left.NoteOff += DrumSynthKickOff;
            AirSticks.Right.NoteOn += DrumSynthSnareOn;
            AirSticks.Right.NoteOff += DrumSynthSnareOff;
            DrumSynthActive = true;
        }
        else
        {
            AirSticks.Left.NoteOn -= DrumSynthKickOn;
            AirSticks.Left.NoteOff -= DrumSynthKickOff;
            AirSticks.Right.NoteOn -= DrumSynthSnareOn;
            AirSticks.Right.NoteOff -= DrumSynthSnareOff;
            DrumSynthActive = false;
        }
    }
}
