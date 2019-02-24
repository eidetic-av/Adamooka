using System;

namespace Eidetic.Andamooka
{
    public static partial class AirSticks
    {
        public enum Hand
        {
            Left, Right
        }

        public static class ControlType
        {
            public enum Motion
            {
                PositionX, PositionY, PositionZ,
                RotationX, RotationY, RotationZ
            }
            public enum Joystick
            {
                X, Y
            }
            public enum Button
            {
                Button1, Button2, Button3, Button4, Bumper
            }

            public enum Velocity { Velocity }
        }

        public abstract class AirSticksMapping<T> : Mapping<T> where T : struct, IConvertible
        {
            public Hand Hand;
            public override float GetInputValue()
            {
                return GetStick(Hand).GetControlValue<T>(Input);
            }
        }

        [System.Serializable]
        public class MotionMapping : AirSticksMapping<ControlType.Motion> { }

        [System.Serializable]
        public class JoystickMapping : AirSticksMapping<ControlType.Joystick> { }

        [System.Serializable]
        public class VelocityMapping : AirSticksMapping<ControlType.Velocity> { }

    }
}