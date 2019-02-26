using System;
using Eidetic.Utility;

namespace Eidetic.Andamooka
{
    public static partial class AirSticks
    {
        public enum Hand
        {
            Left, Right, Both
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

        [System.Serializable]
        public abstract class AirSticksMapping<T> : Mapping<T> where T : struct, IConvertible
        {
            public Hand Hand {get; set;}
            public bool FlipAxis {get; set;}
            public AirSticksMapping(Hand hand)
            {
                Hand = hand;
            }
            public AirSticksMapping(Hand hand, float minimum, float maximum)
            {
                Hand = hand;
                MinimumValue = minimum;
                MaximumValue = maximum;
            }
            // all this below is too convoluted... needs to be refactored
            public override float Output => GetOutput(Hand);
            public override float GetInputValue() => GetInputValue(Hand);
            public float GetInputValue(Hand hand) =>
                GetStick(hand).GetControlValue<T>(Input);
            public float GetOutput(Hand hand) { 
                var value = GetInputValue(hand).Map(MinimumValue, MaximumValue);
                if (FlipAxis) value *= -1;
                return value;
            }
        }

        [System.Serializable]
        public class MotionMapping : AirSticksMapping<ControlType.Motion>
        {
            public MotionMapping(Hand hand) : base(hand) { }
            public MotionMapping(Hand hand, float minimum, float maximum) : base(hand, minimum, maximum) { }
            
        }

        [System.Serializable]
        public class JoystickMapping : AirSticksMapping<ControlType.Joystick>
        {
            public JoystickMapping(Hand hand) : base(hand) { }
            public JoystickMapping(Hand hand, float minimum, float maximum) : base(hand, minimum, maximum) { }
        }

        [System.Serializable]
        public class VelocityMapping : AirSticksMapping<ControlType.Velocity>
        {
            public VelocityMapping(Hand hand) : base(hand) { }
            public VelocityMapping(Hand hand, float minimum, float maximum) : base(hand, minimum, maximum) { }
        }

    }
}