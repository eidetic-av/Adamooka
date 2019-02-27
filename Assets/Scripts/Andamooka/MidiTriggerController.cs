using System;
using Midi;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Eidetic.Unity.Runtime;

using UnityEngine;

[System.Serializable]
public abstract class MidiTriggerController : RuntimeController
{
    // these properties might need to be re-thought
    public abstract Channel MidiChannel { get; set; }
    public abstract List<MidiTrigger> Triggers { get; set; }

    public void InitialiseMidi()
    {
        MidiEventDispatcher.Instance.InputDevice.NoteOn += RouteMidi;
    }

    void RouteMidi(NoteOnMessage noteOnMessage)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            if (!noteOnMessage.Channel.Equals(MidiChannel)) return;
            Triggers.Where(trigger => 
                        trigger.Note.Equals(noteOnMessage.Pitch) || trigger.Note.Equals(Pitch.Any))?.ToList()
                    .ForEach(trigger => trigger.Action.Invoke());
        });
    }

    // Since the MidiTriggers don't serialize their Actions
    // and we don't want to alter the actions at runtime anyway,
    // they must be stored and reloaded when we load a preset from a file
    Action[] LoadingActions;
    public override void BeforeLoad()
    {
        LoadingActions = Triggers.Select(trigger => trigger.Action).ToArray();
    }

    public override void AfterLoad()
    {
        for (int i = 0; i < LoadingActions.Length; i++)
        {
            Triggers[i].Action = LoadingActions[i];
        }
        LoadingActions = null;
    }
}

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