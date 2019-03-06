using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic.Unity.Runtime;
using Eidetic.Unity.Utility;
using Midi;
using RuntimeInspectorNamespace;

public class CueController : MidiTriggerController
{
    public override Channel MidiChannel { get; set; } = Channel.Channel10;

    public override List<MidiTrigger> Triggers { get; set; }

    RodController Rods;
    RingController Ring;
    VortexController HiHatVortex;
    VortexController BasslineVortex;

    void Start()
    {
        InitialiseMidi();

        Rods = GameObject.Find("Rods").GetComponent<RodController>();
        Ring = GameObject.Find("Ring").GetComponent<RingController>();
        HiHatVortex = GameObject.Find("HiHatVortex").GetComponent<VortexController>();
        BasslineVortex = GameObject.Find("BasslineVortex").GetComponent<VortexController>();

        Triggers = new List<MidiTrigger>()
        {
            new MidiTrigger(Pitch.A1, ToggleRods),
            new MidiTrigger(Pitch.G1, ToggleConstantRods),
            new MidiTrigger(Pitch.ASharp1, ToggleRing),
            new MidiTrigger(Pitch.DSharp1, ToggleRingDrumSynth),
            new MidiTrigger(Pitch.CSharp1, ToggleHiHatVortex),
            new MidiTrigger(Pitch.C1, ToggleBasslineVortex),
            new MidiTrigger(Pitch.B1, ToggleBothVortexes)
        };
    }

    [RuntimeInspectorButton("0: Toggle Rods", false, ButtonVisibility.InitializedObjects)]
    public void ToggleRods() => Rods.AirSticksControlActive = !Rods.AirSticksControlActive;

    [RuntimeInspectorButton("1: Toggle Rods Constant State", false, ButtonVisibility.InitializedObjects)]
    public void ToggleConstantRods() => Rods.ConstantEmission = !Rods.ConstantEmission;

    [RuntimeInspectorButton("2: Toggle Ring", false, ButtonVisibility.InitializedObjects)]
    public void ToggleRing() => Ring.ToggleActive();

    [RuntimeInspectorButton("3: Toggle Ring Drum Synth", false, ButtonVisibility.InitializedObjects)]
    public void ToggleRingDrumSynth() => Ring.ToggleDrumSynth();

    [RuntimeInspectorButton("4: Toggle HiHat Vortex", false, ButtonVisibility.InitializedObjects)]
    public void ToggleHiHatVortex() => HiHatVortex.ToggleSystemActive();

    [RuntimeInspectorButton("5: Toggle Bassline Vortex", false, ButtonVisibility.InitializedObjects)]
    public void ToggleBasslineVortex() => BasslineVortex.ToggleSystemActive();

    [RuntimeInspectorButton("6: Toggle Both Vortexes", false, ButtonVisibility.InitializedObjects)]
    public void ToggleBothVortexes()
    {
        ToggleHiHatVortex();
        ToggleBasslineVortex();
    }

    void Update()
    {
        // hotkeys
        // InvokeOnKey(ToggleRods, KeyCode.Q, KeyCode.LeftControl);
        // InvokeOnKey(ToggleConstantRods, KeyCode.W, KeyCode.LeftControl);
        // InvokeOnKey(ToggleRing, KeyCode.E, KeyCode.LeftControl);
        // InvokeOnKey(ToggleRingDrumSynth, KeyCode.R, KeyCode.LeftControl);
        // InvokeOnKey(ToggleHiHatVortex, KeyCode.T, KeyCode.LeftControl);
        // InvokeOnKey(ToggleBasslineVortex, KeyCode.Y, KeyCode.LeftControl);
        // InvokeOnKey(ToggleBothVortexes, KeyCode.U, KeyCode.LeftControl);
    }
    // public static void InvokeOnKey(Action action, KeyCode keyCode, KeyCode? modifier = null)
    // {
    //     if (modifier == null || Input.GetKey((KeyCode)modifier))
    //         if (Input.GetKeyDown(keyCode))
    //             action.Invoke();
    // }
}
