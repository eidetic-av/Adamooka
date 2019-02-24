using UnityEngine;
using System.Collections.Generic;
using System;
using Eidetic.Andamooka;
using Eidetic.Unity.Utility;

namespace Eidetic.Andamooka
{
    public static partial class AirSticks
    {
        public static Stick Left = new Stick() { Hand = Hand.Left };
        public static Stick Right = new Stick() { Hand = Hand.Right };

        public class Stick
        {
            public Hand Hand;

            public Vector3 EulerAngles = Vector3.zero;

            public bool FlipX = false;
            public bool FlipY = false;
            public bool FlipZ = true;

            public float JoystickY;

            public bool NoteIsOn { get; private set; } = false;

            public bool Button4 { get; private set; } = false;

            private Vector3 position;
            public Vector3 Position
            {
                get
                {
                    return position;
                }
                set
                {
                    var newPosition = new Vector3();

                    if (FlipX)
                        newPosition.x = -value.x;
                    else newPosition.x = value.x;
                    if (FlipY)
                        newPosition.y = -value.y;
                    else newPosition.y = value.y;
                    if (FlipZ)
                        newPosition.z = -value.z;
                    else newPosition.z = value.z;

                    position = newPosition;
                }
            }

            public bool Trigger = false;

            public Action NoteOn { get; set; }
            public Action NoteOff { get; set; }

            public void TriggerNoteOn()
            {
                if (NoteOn == null) return;
                Threading.RunOnMain(NoteOn);
                NoteIsOn = true;
            }
            public void TriggerNoteOff()
            {
                if (NoteOff == null) return;
                Threading.RunOnMain(NoteOff);
                NoteIsOn = false;
            }

            public void Button4On()
            {
                Button4 = true;
            }

            public void Button4Off()
            {
                Button4 = false;
            }
        }
    }
}