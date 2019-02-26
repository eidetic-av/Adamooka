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

    void Start()
    {
		InitialiseMidi();

		Rods = GameObject.Find("Rods").GetComponent<RodController>();

		Triggers = new List<MidiTrigger>()
		{
			new MidiTrigger(Pitch.G1, ConstantRods)
		};
    }

    [RuntimeInspectorButton("0: ConstantRods", false, ButtonVisibility.InitializedObjects)]
    public void ConstantRods() => Rods.ConstantEmission = !Rods.ConstantEmission;
}
