using System;
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
            MaximumValue = 1f
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
            MaximumValue = 0.05f,
            ClampInputRange = true
        };

    public bool ConstantEmission = false;

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

        AirSticks.Left.NoteOff += ()
            => TurnOff();
        AirSticks.Right.NoteOff += ()
            => TurnOff();
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

            // Airstick to scale mapping
            LeftHand.transform.localScale = Vector3.one *
                HandScale.GetOutput(AirSticks.Hand.Left);
            RightHand.transform.localScale = Vector3.one *
                HandScale.GetOutput(AirSticks.Hand.Right);

            // Velocity to scale mapping
            LeftHand.transform.localScale = LeftHand.transform.localScale
                .Multiply(Vector3.one * AirSticks.Left.Velocity.Map(VelocityToScale));
            RightHand.transform.localScale = RightHand.transform.localScale
                .Multiply(Vector3.one * AirSticks.Right.Velocity.Map(VelocityToScale));

            // Noise mapping
            ApplyNoiseLeft();
            ApplyNoiseRight();

            // Final scale mapping if the system is 'noteoff'
        }
    }

    //
    // Actions
    //

    void Spawn(AirSticks.Hand hand, int velocity)
    {
        GetSystem(hand).SetSimulationSpeed((velocity / 127f).Map(VelocityToSpawnSpeed));
        GetSystem(hand).Restart();
    }

    void TurnOff()
    {
        if (!ConstantEmission)
        {
            if (!AirSticks.Left.NoteIsOn && !AirSticks.Right.NoteIsOn)
            {
                LeftHand.Clear();
                LeftHand.Stop();
                RightHand.Clear();
                RightHand.Stop();
                Ribbon.Clear();
                Ribbon.Stop();
            }
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

    Vector3[] LeftPositionsPreNoise;
    Vector3[] LastLeftRandomVectors;
    Vector3[] RightPositionsPreNoise;
    Vector3[] LastRightRandomVectors;

    void ApplyNoiseLeft()
    {
        var multiplier = HandNoise.GetOutput(AirSticks.Hand.Left);
        var system = GetSystem(AirSticks.Hand.Left);
        var particles = new ParticleSystem.Particle[system.particleCount];
        system.GetParticles(particles);

        // Remove the noise of the previous frame
        // Otherwise the particles lose their shape
        LeftPositionsPreNoise = particles.Select((particle, i) =>
        {
            if (LastLeftRandomVectors != null && i < LastLeftRandomVectors.Length)
                particle.position -= LastLeftRandomVectors[i];
            return particle.position;
        }).ToArray();

        var randomVectors = new Vector3[system.particleCount];

        for (int i = 0; i < particles.Length; i++)
        {
            randomVectors[i] = GenerateRandomVector3(multiplier) * HandScale.GetOutput(AirSticks.Hand.Left);
            particles[i].position = LeftPositionsPreNoise[i] + randomVectors[i];
        }

        LastLeftRandomVectors = randomVectors;

        system.SetParticles(particles, particles.Length);
    }
    void ApplyNoiseRight()
    {
        var multiplier = HandNoise.GetOutput(AirSticks.Hand.Right);
        var system = GetSystem(AirSticks.Hand.Right);
        var particles = new ParticleSystem.Particle[system.particleCount];
        system.GetParticles(particles);

        // Remove the noise of the previous frame
        // Otherwise the particles lose their shape
        RightPositionsPreNoise = particles.Select((particle, i) =>
        {
            if (LastRightRandomVectors != null && i < LastRightRandomVectors.Length)
                particle.position -= LastRightRandomVectors[i];
            return particle.position;
        }).ToArray();

        var randomVectors = new Vector3[system.particleCount];

        for (int i = 0; i < particles.Length; i++)
        {
            randomVectors[i] = GenerateRandomVector3(multiplier) * HandScale.GetOutput(AirSticks.Hand.Right);
            particles[i].position = RightPositionsPreNoise[i] + randomVectors[i];
        }

        LastRightRandomVectors = randomVectors;

        system.SetParticles(particles, particles.Length);
    }

    Vector3 GenerateRandomVector3(float multiplier = 1f)
    {
        return new Vector3(
            UnityEngine.Random.Range(-1f, 1f) * multiplier,
            UnityEngine.Random.Range(-1f, 1f) * multiplier,
            UnityEngine.Random.Range(-1f, 1f) * multiplier
        );
    }
}
