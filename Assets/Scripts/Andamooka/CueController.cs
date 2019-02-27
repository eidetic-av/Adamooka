using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic.Unity.Runtime;
using Midi;
using RuntimeInspectorNamespace;

public class CueController : MidiTriggerController
{
    public override Channel MidiChannel { get; set; } = Channel.Channel1;

    public override List<MidiTrigger> Triggers { get; set; }

	RodController Rods;
	RingController Ring;
  VortexController Vortex;

    void Start()
    {
		InitialiseMidi();

		Rods = GameObject.Find("Rods").GetComponent<RodController>();
		Ring = GameObject.Find("Ring").GetComponent<RingController>();
    Vortex = GameObject.Find("Vortex").GetComponent<VortexController>();

		Triggers = new List<MidiTrigger>()
		{
			new MidiTrigger(Pitch.A1, ToggleRods),
			new MidiTrigger(Pitch.G1, ToggleConstantRods),
			new MidiTrigger(Pitch.ASharp1, ToggleRing),
			new MidiTrigger(Pitch.DSharp1, ToggleRingDrumSynth),
			new MidiTrigger(Pitch.CSharp1, ToggleHiHatVortex)
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
    public void ToggleHiHatVortex() => Vortex.ToggleHiHatSystem();
}
