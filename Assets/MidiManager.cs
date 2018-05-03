using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Midi;
using System;

public class MidiManager : MonoBehaviour
{

    public string DeviceName = "rtpMIDI";
    InputDevice InputDevice;

    public static int AbletonState;

    public static class OneFiveNine
    {
        public static Action Beep;
        public static Action<Pitch> Bass;
        public static Action<Pitch> Melody;
    }

    // Use this for initialization
    void Start()
    {
        foreach (InputDevice inputDevice in InputDevice.InstalledDevices)
        {
            if (inputDevice.Name.ToLower().Equals(DeviceName.ToLower()))
            {
                InputDevice = inputDevice;
                break;
            }
        }
        if (InputDevice != null)
        {
            InputDevice.Open();
            InputDevice.StartReceiving(null);
            InputDevice.NoteOn += RouteNoteOn;
        }
    }

    private void RouteNoteOn(NoteOnMessage noteOnMessage)
    {
        switch (noteOnMessage.Channel)
        {
            case Channel.Channel1:
                {
                    OneFiveNine.Beep.Invoke();
                    break;
                }
            case Channel.Channel2:
                {
                    OneFiveNine.Bass.Invoke(noteOnMessage.Pitch);
                    break;
                }
            case Channel.Channel16:
                {
                    UpdateAbletonState(noteOnMessage.Pitch);
                    break;
                }
        }
    }

    private void UpdateAbletonState(Pitch pitch)
    {
        switch (pitch)
        {
            case Pitch.E2:
                break;
        }
    }

    private void OnDestroy()
    {
        if (InputDevice != null)
        {
            InputDevice.StopReceiving();
            InputDevice.Close();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
