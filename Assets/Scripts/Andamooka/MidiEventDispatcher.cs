using Eidetic.Unity.Utility;
using Midi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic.Andamooka;

public class MidiEventDispatcher : MonoBehaviour
{
    public static MidiEventDispatcher Instance;

    public string DeviceName = "rtpMIDI";

    public InputDevice InputDevice { get; set; }

    ActivityMonitor ActivityMonitor;

    void Awake()
    {
        Instance = this;
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
            StartCoroutine(UpdateConnectedMonitor());
            MidiEventDispatcher.Instance.InputDevice.NoteOn += (noteOnMessage) =>
                UnityMainThreadDispatcher.Instance().Enqueue(() => StartCoroutine(UpdateReceivedMonitor()));
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

    IEnumerator UpdateConnectedMonitor()
    {
        while (Application.isPlaying)
        {
            if (ActivityMonitor.Instance.gameObject.activeInHierarchy
                && !ActivityMonitor.MidiReceiving)
            {
                ActivityMonitor.MidiConnected = InputDevice != null && InputDevice.IsReceiving;
            }
            yield return new WaitForSeconds(2f);
        }
    }

    IEnumerator UpdateReceivedMonitor()
    {
        if (ActivityMonitor.Instance.gameObject.activeInHierarchy)
        {
            ActivityMonitor.MidiReceiving = true;
            yield return new WaitForSeconds(.125f);
            ActivityMonitor.MidiReceiving = false;
        }
        yield return null;
    }
}