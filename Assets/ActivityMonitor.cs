using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using RuntimeInspectorNamespace;

public class ActivityMonitor : MonoBehaviour
{

    public static Color ConnectedColor { get; private set; } = Color.green;
    public static Color DisconnectedColor { get; private set; } = Color.black;
    public static Color StatusIndicationColor { get; private set; } = Color.yellow;

    public static ActivityMonitor Instance { get; private set; }

    Image OscStatusImage;
    Image MidiStatusImage;

    void Awake()
    {
        Instance = this;
        OscStatusImage = GameObject.Find("OscStatus").GetComponent<Image>();
        MidiStatusImage = GameObject.Find("MidiStatus").GetComponent<Image>();
    }

    static bool oscConnected;
    public static bool OscConnected
    {
        get
        {
            return oscConnected;
        }
        set
        {
            if (value)
                Instance.OscStatusImage.color = ConnectedColor;
            else Instance.OscStatusImage.color = DisconnectedColor;
            oscConnected = value;
        }
    }
    static bool oscReceiving;
    public static bool OscReceiving
    {
        get
        {
            return oscReceiving;
        }
        set
        {
            if (value)
                Instance.OscStatusImage.color = StatusIndicationColor;
            else Instance.OscStatusImage.color = ConnectedColor;
            oscReceiving = value;
        }
    }

    static bool midiConnected;
    public static bool MidiConnected
    {
        get
        {
            return midiConnected;
        }
        set
        {
            if (value)
                Instance.MidiStatusImage.color = ConnectedColor;
            else Instance.MidiStatusImage.color = DisconnectedColor;
            midiConnected = value;
        }
    }
    static bool midiReceiving;
    public static bool MidiReceiving
    {
        get
        {
            return midiReceiving;
        }
        set
        {
            if (value)
                Instance.MidiStatusImage.color = StatusIndicationColor;
            else Instance.MidiStatusImage.color = ConnectedColor;
            midiReceiving = value;
        }
    }
}
