using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic.Unity.Utility;
using Utility;

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

    public bool FadeIn = false;
    bool FadingIn = false;
    public float FadeInLength = 5f;
    public int MaxParticles = 600;
    float FadeInStartTime;

    public bool FadeOut = false;
    bool FadingOut = false;
    public float FadeOutLength = 25f;
    float FadeOutStartTime;
    

    // Use this for initialization
    void Start () {
		
	}

    // Update is called once per frame
    void Update()
    {
        if (FadeIn) {
            FadeInStartTime = Time.time;
            FadingIn = true;
        }
        if (FadingIn) {
            var position = (Time.time - FadeInStartTime) / FadeInLength;
            if (position >= 1) {
                position = 1;
                FadingIn = false;
            }
            var particleCount = Mathf.RoundToInt(position.Map(0, 1, 0, 600));
            var driftMain = ParticleSystem.main;
            driftMain.maxParticles = particleCount;
        }
        if (FadeOut) {
            FadeOutStartTime = Time.time;
            FadingOut = true;
        }
        if (FadingOut) {
            var position = (Time.time - FadeOutStartTime) / FadeOutLength;
            if (position >= 1) {
                position = 1;
                FadingOut = false;
            }
            var particleCount = Mathf.RoundToInt(position.Map(0, 1, 600, 0));
            var driftMain = ParticleSystem.main;
            driftMain.maxParticles = particleCount;
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
            case 10:
                WindZone.windMain = -12f;
                WindZone.windTurbulence = 0;
                WindZone.windPulseMagnitude = 0;
                WindZone.windPulseFrequency = 0;
            break;
            case 11:
                WindZone.windMain = 0;
                WindZone.windTurbulence = 0;
                WindZone.windPulseMagnitude = 0;
                WindZone.windPulseFrequency = 0;
            break;
        }
    }
}
