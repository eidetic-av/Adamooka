using System;
using System.Collections;
using System.Collections.Generic;
using Eidetic.URack.Base;
using Eidetic.URack.Base.UI;
using NAudio.Midi;
using UnityEngine;
using UnityEngine.UIElements;
using Midi;

namespace Eidetic.URack.Midi
{
    public class AirSticksCC : Module
    {
        [Output] public float A;
        [Output] public float B;
        [Output] public float C;
        [Output] public float D;
        [Output] public float E;
        [Output] public float F;

        public void Start()
        {
            MidiEventDispatcher.Instance.InputDevice.ControlChange += InputMessage;
            Debug.Log("Attached event");
        }

        IntSelector Channel;
        public void ElementAttach()
        {
            if (!Element.Container.Contains(Channel))
            {
                Channel = new IntSelector(this, "Channel", 1, 16);
                Element.Container.Add(Channel);
            }
        }

        public void InputMessage(ControlChangeMessage controlChangeMessage)
        {
            if (controlChangeMessage.Channel.Number() == int.Parse(Values["Channel"]))
            {
                var number = controlChangeMessage.Control.Number();
                var value = ((float) controlChangeMessage.Value).Map(0, 127, -1, 1);
                if (number == 1) A = value;
                else if (number == 2) B = value;
                else if (number == 3) C = value;
                else if (number == 4) D = value;
                else if (number == 5) E = value;
                else if (number == 6) F = value;
            }
        }
    }
}