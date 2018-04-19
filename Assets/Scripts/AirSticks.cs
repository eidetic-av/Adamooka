using UnityEngine;

public static class AirSticks
{
    public static Stick Left = new Stick();
    public static Stick Right = new Stick();

    public class Stick
    {
        public bool FlipX = true;
        public bool FlipY = false;
        public bool FlipZ = true;

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

        public Vector3 EulerAngles = Vector3.zero;
    }
}
