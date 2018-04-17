using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityOSC;

public partial class OSCReceive : MonoBehaviour
{

    public static Dictionary<string, ServerLog> Servers = new Dictionary<string, ServerLog>();

    // Use this for initialization
    void Start()
    {
        OSCHandler.Instance.Init();
    }

    // Update is called once per frame
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
        var address = ((OSCMessage)packet.Data[0]).Address.Split("/".ToCharArray());

        // Data at index 0
        if (packet.Data[0] == null) return;
        var dataList = ((OSCMessage)packet.Data[0]).Data;

        RouteOSC(address, dataList);
    }

    private void RouteOSC(string[] address, List<object> data)
    {
        if (address[1] == "airsticks")
        {
            AirSticks.Stick targetStick = AirSticks.Left;
            if (address[2] == "right")
            {
                targetStick = AirSticks.Right;
            }
            if (address[3] == "pos")
            {
                targetStick.Position.x = (float)data[0];
                targetStick.Position.y = (float)data[1];
                targetStick.Position.z = (float)data[2];
            } else if (address[3] == "angles")
            {
                targetStick.EulerAngles.x = (float)data[0];
                targetStick.EulerAngles.y = (float)data[1];
                targetStick.EulerAngles.z = (float)data[2];
            }
        }
    }
}
