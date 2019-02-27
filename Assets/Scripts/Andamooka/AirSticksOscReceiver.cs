using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using OscJack;

namespace Eidetic.Andamooka
{
    public class AirSticksOscReceiver : MonoBehaviour
    {
        public int Port = 12812;
        public bool LogIncomingMessages = false;
        public string LogFilterAddress = "";

        public OscServer Server { get; private set; }

        void Start()
        {
            Server = new OscServer(Port);
			
            Server.MessageDispatcher.AddCallback(String.Empty, (address, data) =>
            {
                if (address.StartsWith("/airsticks"))
                    RouteAirSticksOsc(address, data);
            });
        }

        void OnDestroy()
        {
            Server.Dispose();
        }

        // Routing
        void RouteAirSticksOsc(string addressString, OscDataHandle data)
        {
            var address = addressString.TrimStart('/').Split('/').Skip(1).ToArray();

			// address.ToList().ForEach(n => Debug.Log(n));

            // select the target airstick
            AirSticks.Stick targetStick;
            if (address[0] == "right")
                targetStick = AirSticks.Right;
            else if (address[0] == "left")
                targetStick = AirSticks.Left;
            else return;

            switch (address[1])
            {
                case "pos":
                    targetStick.Position = data.ToVector3();
                    break;
                case "angles":
                    targetStick.EulerAngles = data.ToVector3();
                    break;
                case "note":
                    if (address[2] == "on")
                        targetStick.NoteOn.RunOnMain(data.GetElementAsInt(0));
                    else targetStick.NoteOff.RunOnMain();
                    break;
            }
        }
    }
}