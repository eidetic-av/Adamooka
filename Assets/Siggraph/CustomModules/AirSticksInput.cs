using System.Collections.Generic;
using System.Linq;
using Eidetic.URack.Base;
using Eidetic.URack.Base.UI;
using OscJack;
using UnityEngine;
using Eidetic.Andamooka;

namespace Eidetic.URack.Networking
{
    public class AirSticksInput : Module
    {
        internal static Dictionary<int, OscServer> Servers = new Dictionary<int, OscServer>();
        [SerializeField] public int Port = 12812;

        // [Output, Indicator] public float LeftPositionX => LeftPosition.x;
        // [Output, Indicator] public float LeftPositionY => LeftPosition.y;
        // [Output, Indicator] public float LeftPositionZ => LeftPosition.x;
        // [Output, Indicator] public float LeftAngleX => LeftAngle.x;
        // [Output, Indicator] public float LeftAngleY => LeftAngle.y;
        // [Output, Indicator] public float LeftAngleZ => LeftAngle.z;
        // [Output, Indicator] public float RightPositionX => RightPosition.x;
        // [Output, Indicator] public float RightPositionY => RightPosition.y;
        // [Output, Indicator] public float RightPositionZ => RightPosition.z;
        // [Output, Indicator] public float RightAngleX => RightAngle.x;
        // [Output, Indicator] public float RightAngleY => RightAngle.y;
        // [Output, Indicator] public float RightAngleZ => RightAngle.z;

        [Output, Indicator] public float LeftPositionX => -AirSticks.Left.Position.x;
        [Output, Indicator] public float LeftPositionY => AirSticks.Left.Position.y;
        [Output, Indicator] public float LeftPositionZ => AirSticks.Left.Position.z;
        [Output, Indicator] public float LeftAngleX => AirSticks.Left.EulerAngles.x;
        [Output, Indicator] public float LeftAngleY => AirSticks.Left.EulerAngles.y;
        [Output, Indicator] public float LeftAngleZ => AirSticks.Left.EulerAngles.z;
        [Output, Indicator] public float RightPositionX => AirSticks.Right.Position.x;
        [Output, Indicator] public float RightPositionY => AirSticks.Right.Position.y;
        [Output, Indicator] public float RightPositionZ => AirSticks.Right.Position.z;
        [Output, Indicator] public float RightAngleX => AirSticks.Right.EulerAngles.x;
        [Output, Indicator] public float RightAngleY => AirSticks.Right.EulerAngles.y;
        [Output, Indicator] public float RightAngleZ => AirSticks.Right.EulerAngles.z;

        OscServer Server;

        Vector3 LeftPosition = new Vector3();
        Vector3 RightPosition = new Vector3();
        Vector3 LeftAngle = new Vector3();
        Vector3 RightAngle = new Vector3();

        // new public void OnEnable()
        // {
        //     base.OnEnable();

        //     if (!Servers.ContainsKey(Port))
        //         Servers.Add(Port, Server = new OscServer(Port));
        //     else Server = Servers[Port];

        //     Server.MessageDispatcher.AddRootNodeCallback("airsticks", OnMessageReceived);
        // }

        // new void OnDestroy()
        // {
        //     Servers.Remove(Port);
        //     Server.Dispose();

        //     Server.MessageDispatcher.RemoveRootNodeCallback("airsticks", OnMessageReceived);
        // }

        // void OnMessageReceived(string addressString, OscDataHandle data)
        // {
        //     var address = addressString.TrimStart('/').Split('/').Skip(1).ToArray();

        //     // address.ToList().ForEach(n => Debug.Log(n));

        //     switch (address[1])
        //     {
        //         case "pos":
        //             if (address[0] == "right") RightPosition = ToVector3(data);
        //             else LeftPosition = ToVector3(data);
        //             break;
        //         case "angles":
        //             if (address[0] == "right") RightAngle = ToVector3(data);
        //             else LeftAngle = ToVector3(data);
        //             break;
        //     }
        // }

        Vector3 ToVector3(OscDataHandle data)
        {
            return new Vector3(data.GetElementAsFloat(0), data.GetElementAsFloat(1), data.GetElementAsFloat(2));
        }
    }
}