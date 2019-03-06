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
            public Hand Hand { get; set; }
            public bool FlipAxis { get; set; }
            SerializableVector2 inputRange = new SerializableVector2(0f, 1f);
            public SerializableVector2 InputRange
            {
                get
                {
                    return inputRange;
                }
                set
                {
                    if (value.x == value.y)
                    {
                        value.x += 0.01f;
                    }
                    inputRange = value;
                }
            }
            public bool ClampInputRange { get; set; } = false;
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
            public float GetInputValue(Hand hand)
            {
                var value = GetStick(hand).GetControlValue<T>(Input);
                if (ClampInputRange)
                    value = Clamp(value, InputRange);
                if (FlipAxis) value *= -1;
                return value;
            }
            public float GetOutput(Hand hand)
            {
                var value = GetInputValue(hand);
                value = Map(value, InputRange, MinimumValue, MaximumValue);
                return value;
            }

            public static float Map(float input, SerializableVector2 inputRange, float minimumOutput, float maximumOutput)
            {
                return ((input - inputRange.x) / (inputRange.y - inputRange.x)) * (maximumOutput - minimumOutput) + minimumOutput;
            }

            public static float Clamp(float input, SerializableVector2 range)
            {
                if (input.CompareTo(range.x) < 0) return range.x;
                else if (input.CompareTo(range.y) > 0) return range.y;
                else return input;
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