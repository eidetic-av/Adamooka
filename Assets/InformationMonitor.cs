﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityOSC;
using Midi;
using System;
using Eidetic;
using Utility;
using UnityEngine.UI;

public class InformationMonitor : MonoBehaviour
{

    public GameObject MonitorParent;

    public bool Active = true;
    private bool Showing = true;

    Text FrameRateMonitor;

    RawImage OscStatus, OscMonitor;
    bool OscMonitorActive = false;
    float LastOscMonitorTime = 0f;
    RawImage MidiStatus, MidiMonitor;
    bool UpdateMidiStatus = false;
    bool MidiMonitorActive = false;
    float LastMidiMonitorTime = 0f;
    RawImage KinectStatus;

    float Debounce = 0.125f;

    float PollingFrequency = 1f;
    float LastPollingTime = -1f;


    int FrameCounter = 0;
    float TimeCounter = 0.0f;
    float _frameRate;
    float FrameRate
    {
        get
        {
            return _frameRate;
        }
        set
        {
            _frameRate = value;
            if (FrameRateMonitor != null)
                FrameRateMonitor.text = "FPS: " + ((int)value);
        }
    }

    float QuitCount;
    bool QuitTriggered;

    public static InformationMonitor Instance;

    void Start()
    {
        Instance = this;
        FrameRateMonitor = GameObject.Find("FrameRate").GetComponent<Text>();
        OscStatus = GameObject.Find("OscStatus").GetComponent<RawImage>();
        OscMonitor = GameObject.Find("OscMonitor").GetComponent<RawImage>();
        MidiStatus = GameObject.Find("MidiStatus").GetComponent<RawImage>();
        MidiMonitor = GameObject.Find("MidiMonitor").GetComponent<RawImage>();
        KinectStatus = GameObject.Find("KinectStatus").GetComponent<RawImage>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            Active = !Active;
        }
    }

    void LateUpdate()
    {
        if (!Active)
        {
            if (Showing)
            {
                MonitorParent.SetActive(false);
                Showing = false;
            }
            return;
        }
        else
        {
            if (!Showing)
            {
                MonitorParent.SetActive(true);
                Showing = true;
            }
        }

        // Status polling
        if (Time.time > LastPollingTime + PollingFrequency)
        {

            // OSC Status

            if (OSCHandler.Instance.Servers.Count > 0)
            {
                OscStatus.color = Color.green;
            }
            else
            {
                OscStatus.color = Color.black;
            }

            // MIDI Status

            if (MidiManager.Instance != null)
            {
                if (MidiManager.Instance.InputDevice != null)
                {
                    if (MidiManager.Instance.InputDevice.IsOpen)
                    {
                        MidiStatus.color = Color.green;
                    }
                    else
                        MidiStatus.color = Color.black;
                }
                else
                    MidiStatus.color = Color.black;
            }
            else
                MidiStatus.color = Color.black;

            // Kinect status

            if (KinectManager.Instance != null)
            {
                if (KinectManager.Instance.GetUsersCount() > 0)
                {
                    KinectStatus.color = Color.green;
                }
                else
                    KinectStatus.color = Color.black;
            }
            else
                KinectStatus.color = Color.black;

            LastPollingTime = Time.time;
        }

        // Monitor polling

        if (OSCReceive.RecievedMessage)
        {
            OscMonitorActive = true;
            OscMonitor.color = Color.yellow;
            LastOscMonitorTime = Time.time;
        }
        else if (Time.time > LastOscMonitorTime + Debounce)
        {
            OscMonitorActive = false;
            OscMonitor.color = Color.black;
        }

        if (UpdateMidiStatus)
        {
            MidiMonitorActive = true;
            MidiMonitor.color = Color.yellow;
            LastMidiMonitorTime = Time.time;
            UpdateMidiStatus = false;
        }

        if (Time.time > LastMidiMonitorTime + Debounce)
        {
            MidiMonitorActive = false;
            MidiMonitor.color = Color.black;
        }

        if (TimeCounter < PollingFrequency)
        {
            TimeCounter += Time.smoothDeltaTime;
            FrameCounter++;
        }
        else
        {
            FrameRate = (float)FrameCounter / TimeCounter;
            FrameCounter = 0;
            TimeCounter = 0.0f;
        }

        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (!QuitTriggered)
            {
                QuitTriggered = true;
            }
            else
            {
                Debug.Log("QuitApplication");
                Application.Quit();
            }
        }
        if (QuitTriggered)
        {
            QuitCount += Time.deltaTime;
            if (QuitCount >= .5f)
            {
                QuitCount = 0;
                QuitTriggered = false;
            }
        }
    }

    void MidiMessageTrigger()
    {
        UpdateMidiStatus = true;
    }

    public void MidiNoteOn(NoteOnMessage noteOnMessage)
    {
        MidiMessageTrigger();
    }

    public void MidiNoteOff(NoteOffMessage noteOffMessage)
    {
        MidiMessageTrigger();
    }
}
