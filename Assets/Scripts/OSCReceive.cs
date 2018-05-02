using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityOSC;

public partial class OSCReceive : MonoBehaviour
{

    public static Dictionary<string, ServerLog> Servers = new Dictionary<string, ServerLog>();
    
    void Start()
    {
        OSCHandler.Instance.Init();
    }
    
    void Update()
    {
        for (var i = 0; i < OSCHandler.Instance.packets.Count; i++)
        {
            ProcessOSC(OSCHandler.Instance.packets[i]);
            // Remove them once they have been read.
            OSCHandler.Instance.packets.Remove(OSCHandler.Instance.packets[i]);
            i--;
        }
    }

    private void ProcessOSC(OSCPacket packet)
    {
        if (packet == null) return;

        // Origin
        int serverPort = packet.server.ServerPort;

        // Data at index 0
        if (packet.Data[0] == null) return;

        for (int i = 0; i < packet.Data.Count; i++)
        {
            var address = ((OSCMessage)packet.Data[i]).Address.Split("/".ToCharArray());
            var dataList = ((OSCMessage)packet.Data[i]).Data;
            RouteOSC(address, dataList);
        }

    }

    private void RouteOSC(string[] address, List<object> data)
    {
        Debug.Log(address[2]);

        if (address[1] == "airsticks")
        {
            //Debug.Log("getting message");
            AirSticks.Stick targetStick = AirSticks.Left;
            if (address[2] == "right")
            {
                targetStick = AirSticks.Right;
            }
            if (address[3] == "pos")
            {
                targetStick.Position = new Vector3((float)data[0], (float)data[1], (float)data[2]);
            }
            else if (address[3] == "angles")
            {
                targetStick.EulerAngles.x = (float)data[0];
                targetStick.EulerAngles.y = (float)data[1];
                targetStick.EulerAngles.z = (float)data[2];
            }
            if (address[3] == "note")
            {
                if (address[4] == "on")
                    targetStick.TriggerNoteOn();
                else if (address[4] == "off")
                    targetStick.TriggerNoteOff();
            }
        }
    }
}
