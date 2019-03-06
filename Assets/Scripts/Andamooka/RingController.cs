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

    [RuntimeInspectorButton("1: Kick B", false, ButtonVisibility.InitializedObjects)]
    public void KickB() =>
        CircleController.Triggers[1] = true;

    [RuntimeInspectorButton("2: Snare A", false, ButtonVisibility.InitializedObjects)]
    public void SnareA() =>
        CircleController.Triggers[6] = true;

    [RuntimeInspectorButton("3: Snare B", false, ButtonVisibility.InitializedObjects)]
    public void SnareB() =>
        CircleController.Triggers[5] = true;

    [RuntimeInspectorButton("4: Hi-Hat", false, ButtonVisibility.InitializedObjects)]
    public void HiHat() =>
        CircleController.Triggers[2] = true;

    //
    // Drum synth, not sure if we are using this:
    //
    public bool DrumSynthActive = false;

    [RuntimeInspectorButton("5: Drum synth kick", false, ButtonVisibility.InitializedObjects)]
    public void DrumSynthKick(int velocity)
    {
        if (DrumSynthActive)
            CircleController.Triggers[7] = true;
    }

    [RuntimeInspectorButton("6: Drum synth snare", false, ButtonVisibility.InitializedObjects)]
    public void DrumSynthSnare(int velocity)
    {
        if (DrumSynthActive)
            CircleController.Triggers[8] = true;
    }

    public void ToggleDrumSynth()
    {
        if (!DrumSynthActive)
        {
            AirSticks.Left.NoteOn += DrumSynthKick;
            AirSticks.Right.NoteOn += DrumSynthSnare;
            DrumSynthActive = true;
        }
        else
        {
            AirSticks.Left.NoteOn -= DrumSynthKick;
            AirSticks.Right.NoteOn -= DrumSynthSnare;
            DrumSynthActive = false;
        }
    }
}
