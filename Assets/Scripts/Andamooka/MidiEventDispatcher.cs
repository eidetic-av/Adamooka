using Eidetic.Unity.Utility;
using Midi;
using System;
using System.Collections.Generic;
using UnityEngine;
using Eidetic.Andamooka;

public class MidiEventDispatcher : MonoBehaviour
{
    static MidiEventDispatcher instance;
    public static MidiEventDispatcher Instance
    {
        get
        {
            if (instance == null)
                instance = new GameObject("MidiEventDispatcher").AddComponent<MidiEventDispatcher>();
            return instance;
        }
    }
    // public string DeviceName = "rtpMIDI";
    public string DeviceName = "loopMIDI Port";

    public InputDevice InputDevice { get; set; }

    void Awake()
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
            Debug.Log("Opened MIDI Device: " + InputDevice.Name);
        }
    }

    void OnDestroy()
    {
        if (InputDevice != null)
        {
            InputDevice.StopReceiving();
            InputDevice.Close();
            Debug.Log("Closed MIDI Device: " + InputDevice.Name);
        }
    }
}