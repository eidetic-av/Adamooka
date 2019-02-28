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
            // check midi channel
            if (!noteOnMessage.Channel.Equals(MidiChannel)) return;

            // get the midi triggers
            var targetTriggers = Triggers.Where(trigger =>
                        trigger.Note.Equals(noteOnMessage.Pitch) || trigger.Note.Equals(Pitch.Any))?.ToList();
            // invoke the standard midi triggers
            targetTriggers.Where(t => t.GetType() == typeof(MidiTrigger))?.ToList()
                .ForEach(t => t.Action.Invoke());
            // invoke the noteOn triggers
            targetTriggers.Where(t => t.GetType() == typeof(NoteOnTrigger))?
                .Cast<NoteOnTrigger>().ToList()
                .ForEach(t =>
                {
                    if (t.NoteOnAction == null) Triggers.Remove(t);
                    else t.NoteOnAction.Invoke(noteOnMessage.Pitch, noteOnMessage.Velocity);
                });
        });
    }

    // Since the MidiTriggers don't serialize their Actions
    // and we don't want to alter the actions at runtime anyway,
    // they must be stored and reloaded when we load a preset from a file
    MidiTrigger[] TriggersBeforeLoad;
    Action[] LoadingTriggerActions;
    Action<Pitch, int>[] NoteOnTriggerActions;
    List<int> NoteOnTriggerIndexes = new List<int>();
    public override void BeforeLoad()
    {
        TriggersBeforeLoad = Triggers.ToArray();
        LoadingTriggerActions = Triggers.Where(t => t.GetType() == typeof(MidiTrigger))
            .Select(trigger => trigger.Action).ToArray();

        NoteOnTriggerActions = new Action<Pitch, int>[Triggers.Count(t => t.GetType() == typeof(NoteOnTrigger))];
        int noteOnIndex = 0;
        for (int i = 0; i < Triggers.Count; i++)
        {
            if (Triggers[i].GetType() == typeof(NoteOnTrigger))
            {
                NoteOnTriggerActions[noteOnIndex] = ((NoteOnTrigger)Triggers[i]).NoteOnAction;
                NoteOnTriggerIndexes.Add(i);
                noteOnIndex++;
            }
        }
    }

    public override void AfterLoad()
    {
        for (int i = 0; i < LoadingTriggerActions.Length; i++)
        {
            if (i < Triggers.Count)
                Triggers[i].Action = LoadingTriggerActions[i];
            else
                Triggers.Add(TriggersBeforeLoad[i]);
        }
        for (int i = 0; i < NoteOnTriggerActions.Length; i++)
        {
            ((NoteOnTrigger)Triggers[NoteOnTriggerIndexes[i]]).NoteOnAction = NoteOnTriggerActions[i];
        }
        TriggersBeforeLoad = null;
        LoadingTriggerActions = null;
        NoteOnTriggerActions = null;
        NoteOnTriggerIndexes.Clear();
    }
}

[System.Serializable]
public class MidiTrigger
{
    public Pitch Note;

    [NonSerialized]
    public Action Action;

    public MidiTrigger(Pitch note)
    {
        Note = note;
        Action = () => { };
    }
    public MidiTrigger(Pitch note, Action action)
    {
        Note = note;
        Action = action;
    }
}
[System.Serializable]
public class NoteOnTrigger : MidiTrigger
{
    [NonSerialized]
    public Action<Pitch, int> NoteOnAction;

    public NoteOnTrigger(Action<Pitch, int> action) : base(Pitch.Any)
    {
        NoteOnAction = action;
    }
}