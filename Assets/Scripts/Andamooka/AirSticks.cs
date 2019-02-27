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
        public static Stick Left = new Stick(Hand.Left);
        public static Stick Right = new Stick(Hand.Right);

        public static Stick GetStick(Hand hand)
        {
            if (hand == Hand.Left)
                return Left;
            else return Right;
        }

        public class Stick
        {
            public Hand Hand {get; private set;}

            public Action<int> NoteOn { get; set; }
            public Action NoteOff { get; set; }
            public float Velocity = 0; // Normalised
            public bool NoteIsOn = false;

            public Stick(Hand hand)
            {
                Hand = hand;
                NoteOn += (velocity) => { NoteIsOn = true; Velocity = (velocity / 127f); };
                NoteOff += () => { NoteIsOn = false; };
            }

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

            public void Button4On()
            {
                Button4 = true;
            }

            public void Button4Off()
            {
                Button4 = false;
            }
        }
        public static Hand GetOtherHand(this Hand hand){
            if (hand.Equals(Hand.Left))
                return Hand.Right;
            else
                return Hand.Left;
        }
    }
}