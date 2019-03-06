using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Eidetic.Unity.Utility;
using Eidetic.Andamooka;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleDropper : MonoBehaviour
{
    public int ParticleCount;
    public List<ParticleSystem> ParticleSystems;

    public float EmissionInterval = 0.1f;
    float LastEmissionTime = 0f;
    void Update()
    {
        var dropperSystem = GetComponent<ParticleSystem>();
        var dropperMainModule = dropperSystem.main;

        if (Time.time - LastEmissionTime > EmissionInterval)
        {
            if (AirSticks.Left.NoteIsOn)
            {
                ParticleSystems.SelectMany(system =>
                {
                    var theseParticles = new ParticleSystem.Particle[system.particleCount];
                    system.GetParticles(theseParticles);
                    for (int i = 0; i < theseParticles.Length; i++)
                    {
                        theseParticles[i].position = theseParticles[i].position
                            .Multiply(system.transform.localScale)
                            .Add(system.transform.position);
                    }
                    return theseParticles;
                }).ToList()
                    .ForEach(particle =>
                    {
                        particle.startColor = dropperMainModule.startColor.color;
                        particle.startLifetime = dropperMainModule.startLifetime.constant;
                        dropperSystem.Emit(particle);
                    });
            }
        }
    }
}
