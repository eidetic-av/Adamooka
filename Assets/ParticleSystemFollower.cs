using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(ParticleSystem))]
public class ParticleSystemFollower : MonoBehaviour
{
	public int ParticleCount;
    public List<ParticleSystem> ParticleSystems;

    void Update()
    {
        ParticleCount = ParticleSystems.Sum(p => p.particleCount);
		if (ParticleCount <= 0) return;

		GetComponent<ParticleSystem>().Clear();
        GetComponent<ParticleSystem>().SetParticles(
            ParticleSystems.SelectMany(system =>
                {
                    var theseParticles = new ParticleSystem.Particle[system.particleCount];
                    system.GetParticles(theseParticles);
					for (int i = 0; i < theseParticles.Length; i++)
						theseParticles[i].position += system.transform.position;
                    return theseParticles;
                }).ToArray(),
            ParticleCount
        );
    }
}
