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
public class RingController : RuntimeController
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
    public Channel MidiChannel { get; set; } = Channel.Channel9;
    public List<MidiTrigger> Triggers { get; set; }

    //
    // Initialisation
    //
    NoiseCircleController CircleController;
    void Start()
    {
        CircleController = GameObject.Find("NoiseCircleController")
            .GetComponent<NoiseCircleController>();
        Triggers = new List<MidiTrigger>() {
            new MidiTrigger(Pitch.F2, KickA),
            new MidiTrigger(Pitch.ASharp2, KickB),
            new MidiTrigger(Pitch.G2, SnareA),
            new MidiTrigger(Pitch.CSharp3, SnareB),
            new MidiTrigger(Pitch.A2, HiHat)
        };

        MidiEventDispatcher.Instance.InputDevice.NoteOn += RouteRingMidi;
    }

    void RouteRingMidi(NoteOnMessage noteOnMessage) =>
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            if (!ControlWithAirSticks) return;
            if (!noteOnMessage.Channel.Equals(MidiChannel)) return;
            Triggers.SingleOrDefault(trigger =>
                    trigger.Note.Equals(noteOnMessage.Pitch))?.Action.Invoke();
        });

    [RuntimeInspectorButton("KickA", false, ButtonVisibility.InitializedObjects)]
    public void KickA() =>
        CircleController.Triggers[4] = true;

    [RuntimeInspectorButton("KickB", false, ButtonVisibility.InitializedObjects)]
    public void KickB() =>
        CircleController.Triggers[1] = true;

    [RuntimeInspectorButton("SnareA", false, ButtonVisibility.InitializedObjects)]
    public void SnareA() =>
        CircleController.Triggers[6] = true;

    [RuntimeInspectorButton("SnareB", false, ButtonVisibility.InitializedObjects)]
    public void SnareB() =>
        CircleController.Triggers[5] = true;

    [RuntimeInspectorButton("HiHat", false, ButtonVisibility.InitializedObjects)]
    public void HiHat() =>
        CircleController.Triggers[2] = true;

    [System.Serializable]
    public class MidiTrigger
    {
        public Pitch Note;

        [NonSerialized]
        public Action Action;
        
        public MidiTrigger(Pitch note, Action action)
        {
            Note = note;
            Action = action;
        }
    }
}
