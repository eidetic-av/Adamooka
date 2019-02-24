
namespace Eidetic.Andamooka
{
    public static partial class AirSticks
    {
        public enum Hand
        {
            Left, Right
        }

        public static class ControlTypes
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
        }
        
        [System.Serializable]
        public struct MotionMapping
        {
            public ControlTypes.Motion Input;
            public float MinimumValue;
            public float MaximumValue;
        }
        
        [System.Serializable]
        public struct VelocityMapping
        {
            public float MinimumValue;
            public float MaximumValue;
        }

    }
}