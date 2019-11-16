using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Eidetic;
using Eidetic.Andamooka;
using Midi;
using UnityEngine;
using UnityOSC;
using Utility;

public partial class OSCReceive : MonoBehaviour
{

    public static Dictionary<string, ServerLog> Servers = new Dictionary<string, ServerLog>();

    public bool LogRecievedAddresses = false;
    public bool LogHandPosition = false;

    public static bool RecievedMessage = false;

    void Start()
    {
        OSCHandler.Instance.Init();
    }

    void Update()
    {
        RecievedMessage = false;
        if (OSCHandler.Instance.packets.Count > 0)
        {
            RecievedMessage = true;
            for (var i = 0; i < OSCHandler.Instance.packets.Count; i++)
            {
                ProcessOSC(OSCHandler.Instance.packets[i]);
                // Remove them once they have been read.
                OSCHandler.Instance.packets.Remove(OSCHandler.Instance.packets[i]);
                i--;
            }
        }
    }

    private void ProcessOSC(OSCPacket packet)
    {
        if (packet == null) return;

        // Data at index 0
        if (packet.Data[0] == null) return;
        Debug.Log(packet.IsBundle());

        for (int i = 0; i < packet.Data.Count; i++)
        {
            try
            {
                // Debug.Log(m);
                // var address = ((OSCMessage)packet.Data[i]).Address.Split("/".ToCharArray());
                // var dataList = ((OSCMessage)packet.Data[i]).Data;
                // RouteOSC(address, dataList);
            }
            catch (Exception e)
            {
                Debug.Log(e.Message + "\n" + e.StackTrace);
            }
        }

    }

    private string DataToString(List<object> data)
    {
        string buffer = "";

        for (int i = 0; i < data.Count; i++)
        {
            buffer += data[i].ToString() + " ";
        }

        buffer += "\n";

        return buffer;
    }

    private void RouteOSC(string[] address, List<object> data)
    {
        if (LogRecievedAddresses)
        {
            var logString = "";
            for (int i = 0; i < address.Length; i++)
            {
                logString += address[i] + "/";
            }
            logString += "\n";
            for (int i = 0; i < data.Count; i++)
            {
                logString += data.ToString() + ", ";
            }
            Debug.Log(logString);
        }
        if (address[1] == "airsticks")
        {
            AirSticks.Stick targetStick = AirSticks.Left;
            if (address[2] == "right")
            {
                targetStick = AirSticks.Right;
            }
            if (address[3] == "pos")
            {
                targetStick.Position = new Vector3((float) data[0], (float) data[1], (float) data[2]);
                if (LogHandPosition)
                    Debug.Log(targetStick.Hand.ToString() + "\n:" + targetStick.Position.x + ", " + targetStick.Position.y + ", " + targetStick.Position.z);
            }
            else if (address[3] == "angles")
            {
                targetStick.EulerAngles.x = (float) data[0];
                targetStick.EulerAngles.y = (float) data[1];
                targetStick.EulerAngles.z = (float) data[2];
            }
            if (address[3] == "note")
            {
                Debug.Log("YUP");
                if (address[4] == "on")
                    targetStick.NoteOn.RunOnMain((int) data[0]);
                if (address[4] == "off")
                    targetStick.NoteOff.RunOnMain();
            }
            if (address[3] == "trigger")
            {
                var value = data[0].ToString();
                if (value == "0")
                {
                    targetStick.Trigger = false;
                }
                else if (value == "1")
                {
                    targetStick.Trigger = true;
                }
            }
            if (address[3] == "joystick")
            {
                var value = Convert.ToSingle(data[1].ToString());
                targetStick.JoystickY = value;
            }
            if (address[3] == "buttons")
            {
                var value = Convert.ToString(data);
                // Debug.Log(value);
            }
        }
    }
}