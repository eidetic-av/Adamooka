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
        = new AirSticks.VelocityMapping(AirSticks.Hand.Both);
    public AirSticks.MotionMapping MotionToHandScale { get; set; }
        = new AirSticks.MotionMapping(AirSticks.Hand.Both);

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

        // AirSticks.Left.NoteOff += StopLeft;
        // AirSticks.Right.NoteOff += StopRight;
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

            LeftHand.transform.localScale = Vector3.one *
                MotionToHandScale.GetOutput(AirSticks.Hand.Left);
            RightHand.transform.localScale = Vector3.one *
                MotionToHandScale.GetOutput(AirSticks.Hand.Right);

            // This works but need to use a different noise
            // so the particles don't drift away from each other

            // var leftNoise = LeftHand.noise;
            // leftNoise.strength = NoiseMappingLeft.Output;
            // var rightNoise = RightHand.noise;
            // rightNoise.strength = NoiseMappingRight.Output;
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

    void StopLeft() => StartCoroutine(RemoveParticlesRoutine(LeftHand));
    void StopRight() => StartCoroutine(RemoveParticlesRoutine(RightHand));

    IEnumerator RemoveParticlesRoutine(ParticleSystem system, float interval = 0.01f, int count = 5)
    {
        while (system.particleCount > 0)
        {
            var particles = new ParticleSystem.Particle[system.particleCount];
            particles.ToList().RemoveRange(0, count);
            system.SetParticles(particles.ToArray(), system.particleCount - count);
            yield return new WaitForSeconds(interval);
        }
        system.Stop();
    }
}
