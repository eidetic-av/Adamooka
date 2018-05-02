using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic;
using Eidetic.Unity.Utility;
using Midi;

public class ParticleController : MonoBehaviour
{

    public ParticleSystem ParticleSystem;

    public MeshFilter BaseMesh;

    void Start()
    {
        if (ParticleSystem == null)
        {
            ParticleSystem = gameObject.GetComponentInChildren<ParticleSystem>(true);
        }
        BaseMesh = GameObject.Find("UserMesh").GetComponent<MeshFilter>();
    }

    void Update()
    {
        var particleCount = ParticleSystem.particleCount;
        var baseVertexCount = BaseMesh.mesh.vertexCount;
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleCount];
        ParticleSystem.GetParticles(particles);
        for (int i = 0; i < particleCount; i++)
        {
            var particle = particles[i];
            if (i < BaseMesh.mesh.vertexCount)
            {
                particle.position = BaseMesh.mesh.vertices[i];
            }
            else
            {
                particle.position = Vector3.zero;
            }
            particles[i] = particle;
        }
        ParticleSystem.SetParticles(particles, particleCount);
    }
}
