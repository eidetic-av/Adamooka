using UnityEngine;

public static class AirSticks
{
    public class Stick
    {
        public Vector3 Position = Vector3.zero;
        public Vector3 EulerAngles = Vector3.zero;
    }

    public static Stick Left = new Stick();
    public static Stick Right = new Stick();
}
