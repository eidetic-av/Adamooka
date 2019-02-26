using System;
using Midi;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Eidetic.Unity.Runtime;

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
            Triggers.SingleOrDefault(trigger =>
                    trigger.Note.Equals(noteOnMessage.Pitch))?.Action.Invoke();
        });
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