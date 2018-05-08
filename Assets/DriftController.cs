using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic.Unity.Utility;

public class DriftController : MonoBehaviour {

    public GameObject UserMesh;
    public MeshFilter UserMeshFilter;

    public WindZone WindZone;

    public ParticleSystem ParticleSystem;

    public Vector3 Scaling = new Vector3(0, 0, 0);
    public Vector3 HonedOffset = new Vector3(0, 0, 0);

    [Range(0, 1)]
    public float Interpolation = 0f;

    ParticleSystem.Particle[] ParticleArray;

    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {
        if (KinectManager.Instance != null && KinectManager.Instance.GetUsersCount() != 0)
        {
            if (ParticleArray == null)
            {
                ParticleArray = new ParticleSystem.Particle[ParticleSystem.main.maxParticles];
            }

            ParticleSystem.GetParticles(ParticleArray);
            var userMeshVertices = UserMeshFilter.mesh.vertices;

            for (int i = 0; i < ParticleSystem.main.maxParticles; i++)
            {
                int vertex = (i * (userMeshVertices.Length / ParticleSystem.main.maxParticles) * 2) % userMeshVertices.Length;
                var vertexPosition = new Vector3(
                    (userMeshVertices[vertex].x * Scaling.x) + HonedOffset.x,
                    (userMeshVertices[vertex].y * Scaling.y) + HonedOffset.y,
                    (userMeshVertices[vertex].z * Scaling.z) + HonedOffset.z
                );

                var xPosition = Mathf.Lerp(ParticleArray[i].position.x, vertexPosition.x, Interpolation);
                var yPosition = Mathf.Lerp(ParticleArray[i].position.y, vertexPosition.x, Interpolation);
                var zPosition = Mathf.Lerp(ParticleArray[i].position.z, vertexPosition.x, Interpolation);

                ParticleArray[i].position = new Vector3(xPosition, yPosition, zPosition);
            }

            ParticleSystem.SetParticles(ParticleArray, ParticleArray.Length);
        }
    }

    public void SelectState(int stateNumber)
    {
        var noiseModule = ParticleSystem.noise;
        switch(stateNumber)
        {
            case 0:
                noiseModule.strength = 0.05f;
                noiseModule.frequency = 3;
                noiseModule.scrollSpeed = 0;
                WindZone.windMain = 0;
                WindZone.windTurbulence = 0;
                WindZone.windPulseMagnitude = 3;
                WindZone.windPulseFrequency = 2;
                break;

            case 1:
                noiseModule.strength = 0.05f;
                noiseModule.frequency = 3;
                noiseModule.scrollSpeed = 0;
                WindZone.windMain = 1;
                WindZone.windTurbulence = 0;
                WindZone.windPulseMagnitude = 3;
                WindZone.windPulseFrequency = 2;
                break;

            case 2:
                noiseModule.strength = 0.5f;
                noiseModule.frequency = 3;
                noiseModule.scrollSpeed = 50;
                WindZone.windMain = 0;
                WindZone.windTurbulence = 0;
                WindZone.windPulseMagnitude = 3;
                WindZone.windPulseFrequency = 2;
                break;
            case 3:
                noiseModule.strength = 0.5f;
                noiseModule.frequency = 1;
                noiseModule.scrollSpeed = 0.05f;
                WindZone.windMain = 3;
                WindZone.windTurbulence = 0;
                WindZone.windPulseMagnitude = 3;
                WindZone.windPulseFrequency = 2;
                break;
            case 4:
                noiseModule.strength = 0.5f;
                noiseModule.frequency = 5;
                noiseModule.scrollSpeed = 50f;
                WindZone.windMain = 2;
                WindZone.windTurbulence = 0.5f;
                WindZone.windPulseMagnitude = 5;
                WindZone.windPulseFrequency = 2;
                break;
            case 5:
                noiseModule.strength = 0.5f;
                noiseModule.frequency = 2;
                noiseModule.scrollSpeed = 0.05f;
                WindZone.windMain = 0;
                WindZone.windTurbulence = 0;
                WindZone.windPulseMagnitude = 3;
                WindZone.windPulseFrequency = 2;
                break;
            case 6:
                noiseModule.strength = 1f;
                noiseModule.frequency = 1;
                noiseModule.scrollSpeed = 20f;
                WindZone.windMain = 0.05f;
                WindZone.windTurbulence = 2;
                WindZone.windPulseMagnitude = 3;
                WindZone.windPulseFrequency = 5;
                break;
            case 7:
                noiseModule.strength = 2f;
                noiseModule.frequency = 15;
                noiseModule.scrollSpeed = 0.05f;
                WindZone.windMain = 0;
                WindZone.windTurbulence = 0;
                WindZone.windPulseMagnitude = 3;
                WindZone.windPulseFrequency = 2;
                break;
            case 8:
                noiseModule.strength = 2f;
                noiseModule.frequency = 15;
                noiseModule.scrollSpeed = 0.05f;
                WindZone.windMain = -2f;
                WindZone.windTurbulence = 0;
                WindZone.windPulseMagnitude = 3;
                WindZone.windPulseFrequency = 2;
                break;
            case 9:
                noiseModule.strength = 2f;
                noiseModule.frequency = 15;
                noiseModule.scrollSpeed = 95f;
                WindZone.windMain = -2f;
                WindZone.windTurbulence = 0;
                WindZone.windPulseMagnitude = 3;
                WindZone.windPulseFrequency = 2;
                break;
        }
    }
}
