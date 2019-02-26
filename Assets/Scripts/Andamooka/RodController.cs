using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RuntimeInspectorNamespace;
using Eidetic.Andamooka;
using Eidetic.Utility;
using Eidetic.Unity.Runtime;
using Eidetic.Unity.Utility;

[System.Serializable]
public class RodController : RuntimeController
{
    //
    // Runtime control properties
    //
    public bool ControlWithAirSticks { get; set; } = true;
    public SerializableVector3 PositionScale { get; set; }
        = new SerializableVector3(1, 1, 1);
    public SerializableVector3 LeftHandOffset { get; set; }
        = new SerializableVector3(0, 0, 0);
    public SerializableVector3 RightHandOffset { get; set; }
        = new SerializableVector3(0, 0, 0);

    public AirSticks.VelocityMapping VelocityToSpawnSpeed { get; set; }
        = new AirSticks.VelocityMapping(AirSticks.Hand.Both)
        {
            MinimumValue = 1f,
            MaximumValue = 1f
        };
    public AirSticks.MotionMapping HandScale { get; set; }
        = new AirSticks.MotionMapping(AirSticks.Hand.Both)
        {
            Input = AirSticks.ControlType.Motion.RotationX,
            MinimumValue = 0.2f,
            MaximumValue = 1.5f
        };
    public AirSticks.VelocityMapping VelocityToScale { get; set; }
        = new AirSticks.VelocityMapping(AirSticks.Hand.Both)
        {
            MinimumValue = 0.5f,
            MaximumValue = 1f
        };
    public AirSticks.MotionMapping HandNoise { get; set; }
        = new AirSticks.MotionMapping(AirSticks.Hand.Both)
        {
            Input = AirSticks.ControlType.Motion.PositionY,
            MinimumValue = 0f,
            MaximumValue = 1f,
            FlipAxis = true
        };

    //
    // Initialisation stuff
    //
    ParticleSystem LeftHand;
    ParticleSystem RightHand;
    ParticleSystem Ribbon;

    void Start()
    {
        LeftHand = GameObject.Find("LeftHand").GetComponent<ParticleSystem>();
        RightHand = GameObject.Find("RightHand").GetComponent<ParticleSystem>();
        Ribbon = GameObject.Find("Ribbon").GetComponent<ParticleSystem>();

        AirSticks.Left.NoteOn += (velocity)
            => Spawn(AirSticks.Hand.Left, velocity);
        AirSticks.Right.NoteOn += (velocity)
            => Spawn(AirSticks.Hand.Right, velocity);

        // AirSticks.Left.NoteOff += ()
        //     => Shrink(AirSticks.Hand.Left, NoteOffShrinkRate, HandScale.MinimumValue);
        // AirSticks.Right.NoteOff += ()
        //     => Shrink(AirSticks.Hand.Right, NoteOffShrinkRate, HandScale.MinimumValue);
    }

    //
    // Runtime loop
    //
    void Update()
    {
        if (ControlWithAirSticks)
        {
            LeftHand.transform.position =
                AirSticks.Left.Position * PositionScale + LeftHandOffset;
            RightHand.transform.position =
                AirSticks.Right.Position * PositionScale + RightHandOffset;

            // if (AirSticks.Left.NoteIsOn || AirSticks.Right.NoteIsOn)
            // {
            LeftHand.transform.localScale = Vector3.one *
                HandScale.GetOutput(AirSticks.Hand.Left);
            RightHand.transform.localScale = Vector3.one *
                HandScale.GetOutput(AirSticks.Hand.Right);
            // }

            // Velocity to scale mapping
            LeftHand.transform.localScale = LeftHand.transform.localScale
                .Multiply(Vector3.one * AirSticks.Left.Velocity.Map(VelocityToScale));
                
            RightHand.transform.localScale = RightHand.transform.localScale
                .Multiply(Vector3.one * AirSticks.Right.Velocity.Map(VelocityToScale));

            // Noise mapping
        }
    }

    //
    // Inner methods
    //
    ParticleSystem GetSystem(AirSticks.Hand hand)
    {
        if (hand == AirSticks.Hand.Left)
            return LeftHand;
        else if (hand == AirSticks.Hand.Right)
            return RightHand;
        else throw new System.ArgumentException();
    }
    ParticleSystem GetOtherSystem(AirSticks.Hand hand)
    {
        if (hand == AirSticks.Hand.Left)
            return RightHand;
        else if (hand == AirSticks.Hand.Right)
            return LeftHand;
        else throw new System.ArgumentException();
    }
    void Spawn(AirSticks.Hand hand, int velocity)
    {
        GetSystem(hand).SetSimulationSpeed((velocity / 127f).Map(VelocityToSpawnSpeed));
        GetSystem(hand).Restart();
    }

    // public float NoteOffShrinkRate { get; set; } = 20f;

    // public bool ConstantEmission { get; set; } = false;

    // void Shrink(AirSticks.Hand hand, float shrinkRate, float minimumScale = 0f) =>
    //     StartCoroutine(ShrinkRoutine(hand, shrinkRate, minimumScale));

    // IEnumerator ShrinkRoutine(AirSticks.Hand hand, float shrinkRate, float minimumScale = 0f)
    // {
    //     var system = GetSystem(hand);
    //     while (system.transform.localScale.x > (minimumScale + .005f))
    //     {
    //         var scale = system.transform.localScale;
    //         scale -= (Vector3.one * shrinkRate * Time.deltaTime);
    //         system.transform.localScale = scale;
    //         yield return null;
    //     }
    //     system.transform.localScale = Vector3.one * minimumScale;

    //     if (!ConstantEmission)
    //     {
    //         var otherSystem = GetSystem(hand.GetOtherHand());
    //         if (otherSystem.transform.localScale == Vector3.one * minimumScale)
    //         {
    //             // if both systems are shrunk, stop the simulations
    //             system.Stop();
    //             system.Clear();
    //             otherSystem.Stop();
    //             otherSystem.Clear();
    //             Ribbon.Clear();
    //         }
    //     }
    // }
}
