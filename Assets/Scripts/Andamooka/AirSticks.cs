using UnityEngine;
using System.Collections.Generic;
using System;
using System.Linq;
using Eidetic.Andamooka;
using Eidetic.Unity.Utility;

namespace Eidetic.Andamooka
{
    public static partial class AirSticks
    {
        public static Stick Left = new Stick() { Hand = Hand.Left };
        public static Stick Right = new Stick() { Hand = Hand.Right };
        public static Stick GetStick(Hand hand)
        {
            if (hand == Hand.Left)
                return Left;
            else return Right;
        }

        public class Stick
        {
            public Hand Hand;
            public float GetControlValue<T>(T controlType) where T : struct, IConvertible
            {
                Enum controlTypeEnum = controlType as Enum; 
                if (controlTypeEnum == null) return 0;

                var controlTypeName = controlTypeEnum.ToString();
                
                switch (controlTypeName)
                {
                    case ("PositionX"): return Position.x;
                    case ("PositionY"): return Position.y;
                    case ("PositionZ"): return Position.z;
                    case ("RotationX"): return EulerAngles.x;
                    case ("RotationY"): return EulerAngles.y;
                    case ("RotationZ"): return EulerAngles.z;
                    default: return 0;
                }
            }
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

            // Normalised velocity (0-1)
            public float Velocity { get; set; }

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