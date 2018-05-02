using UnityEngine;
using System.Collections.Generic;
using System;
using Eidetic.Unity.Utility;

public static class AirSticks
{
    public static Stick Left = new Stick() { Hand = Hand.Left };
    public static Stick Right = new Stick() { Hand = Hand.Right };

    public enum Hand
    {
        Left, Right
    }

    public class Stick
    {
        public Hand Hand;

        public Vector3 EulerAngles = Vector3.zero;

        public bool FlipX = false;
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

        public Action NoteOn { get; set; }
        public Action NoteOff { get; set; }

        public void TriggerNoteOn()
        {
            if (NoteOn == null) return;
            Threading.RunOnMain(NoteOn);
        }
        public void TriggerNoteOff()
        {
            if (NoteOff == null) return;
            Threading.RunOnMain(NoteOff);
        }
    }
}
