using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RuntimeInspectorNamespace;
using Eidetic.Unity.Runtime;
using Eidetic.Unity.Utility;

[System.Serializable]
public class RodController : RuntimeController
{
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

        AirSticks.Left.NoteOn += SpawnLeft;
        AirSticks.Right.NoteOn += SpawnRight;
        AirSticks.Left.NoteOff += StopLeft;
        AirSticks.Right.NoteOff += StopRight;
    }

    //
    // Runtime control properties and methods
    //

    // Spawn speed maps these values to the NoteOn velocity 
    SerializableVector2 _spawnSpeedMinMax;
    public SerializableVector2 SpawnSpeedMinMax
    {
        get
        {
            return _spawnSpeedMinMax;
        }
        set
        {
            _spawnSpeedMinMax = value;
        }
    }

    //
    // Inner methods
    //
    void SpawnLeft() { LeftHand.Restart(); }
    void SpawnRight() { RightHand.Restart(); }
    void StopLeft()
    {
        StartCoroutine(RemoveParticlesRoutine(LeftHand));
    }
    void StopRight()
    {
        StartCoroutine(RemoveParticlesRoutine(RightHand));
    }

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
