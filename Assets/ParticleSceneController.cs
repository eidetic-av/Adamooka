using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleSceneController : MonoBehaviour
{

    public static ParticleSceneController Instance;

    public bool IntroVisuals = false;
    public bool IncreaseExpandCircle = false;
    public bool EnableRods = false;
    public bool DisableRods = false;
    public bool KickAndSnareWithRing = false;
    public bool Breakdown = false;
    public bool OneFiveNineFull = false;
    public bool StartOneFiveNineOut = false;
    public bool OneFiveNineOut = false;
    public bool ActivateRain = false;
    public bool FinaleState = false;

    private void Start()
    {
        Instance = this;
    }

    void Update()
    {
        if (IntroVisuals)
        {
            RodParticleController.Instance.ConstantEmission = false;
            RodParticleController.Instance.TrackAirsticks = false;
            // CircleParticleController.Instance.Visible = true;
            // CircleParticleController.Instance.ControlSphereSize = true;
            // CircleParticleController.Instance.Expanded = false;
            // CircleParticleController.Instance.Increasing = false;
            // CircleParticleController.Instance.EmitConstantly = false;
            // CircleParticleController.Instance.TrackAirsticks = true;
            OneFiveNineCircleController.Instance.ResetSceneLight = true;
            OneFiveNineCircleController.Instance.HideRings = true;
            IntroVisuals = false;
        }
        else if (IncreaseExpandCircle)
        {
            // CircleParticleController.Instance.Expanded = true;
            //CircleParticleController.Instance.Increasing = true;
            IncreaseExpandCircle = false;
        }
        else if (EnableRods)
        {
            RodParticleController.Instance.TrackAirsticks = true;
            RodParticleController.Instance.ConstantEmission = false;
            EnableRods = false;
        } else if (DisableRods)
        {
            RodParticleController.Instance.TrackAirsticks = false;
            RodParticleController.Instance.ConstantEmission = false;
            DisableRods = false;
        }
        else if (KickAndSnareWithRing)
        {
            OneFiveNineCircleController.Instance.ActivateScene = true;
            // CircleParticleController.Instance.Visible = true;
            RodParticleController.Instance.TrackAirsticks = false;
            KickAndSnareWithRing = false;
        }
        else if (Breakdown)
        {
            RodParticleController.Instance.TrackAirsticks = true;
            RodParticleController.Instance.ConstantEmission = true;
            OneFiveNineCircleController.Instance.ResetSceneLight = true;
            OneFiveNineCircleController.Instance.SendTrackingToSceneLight = false;
            OneFiveNineCircleController.Instance.SendSnareToParticles = false;
            // CircleParticleController.Instance.Visible = true;
            // CircleParticleController.Instance.Increasing = false;
            // CircleParticleController.Instance.Expanded = true;
            // CircleParticleController.Instance.EmitConstantly = true;
            Breakdown = false;
        }
        else if (OneFiveNineFull)
        {
            OneFiveNineCircleController.Instance.ResetSceneLight = false;
            OneFiveNineCircleController.Instance.SendTrackingToSceneLight = true;
            OneFiveNineCircleController.Instance.SendSnareToParticles = true;
            OneFiveNineFull = false;
        }
        else if (StartOneFiveNineOut)
        {
            // CircleParticleController.Instance.Increasing = true;
            RodParticleController.Instance.ConstantEmission = false;
            StartOneFiveNineOut = false;
        }
        else if (OneFiveNineOut)
        {
            if (OneFiveNineCircleController.Instance != null) {
                OneFiveNineCircleController.Instance.HideRings = true;
                OneFiveNineCircleController.Instance.ResetSceneLight = true;
                OneFiveNineCircleController.Instance.SendTrackingToSceneLight = false;
                OneFiveNineCircleController.Instance.SendSnareToParticles = false;
            }
            RodParticleController.Instance.TrackAirsticks = false;
            // CircleParticleController.Instance.Visible = false;
            if (GameObject.Find("MembraneCircle") != null)
                GameObject.Find("MembraneCircle").SetActive(false);
            if (GameObject.Find("LightPlane") != null)
                GameObject.Find("LightPlane").SetActive(false);
            OneFiveNineOut = false;
        } else if (ActivateRain)
        {
            MidiManager.Instance.ActivateRainState();
            ActivateRain = false;
        }

        if (FinaleState) {
            GameObject.Find("Particle Camera").GetComponent<Camera>().fieldOfView = 30.6f;
            var rainController = GameObject.Find("RainParticleController").GetComponent<RainController>();
            rainController.EmissionCount = 4000;
            rainController.EnableEmission = true;
            rainController.AirSticksGravityControl = false;
            var proceduralController = GameObject.Find("ProceduralMesh").GetComponent<ProceduralMeshController>();
            proceduralController.ControlInterpolationWithAirSticks = false;
            proceduralController.Interpolation = 0;
            var windzone = GameObject.Find("WindZoneA");
            if (windzone != null) windzone.SetActive(false);
            var particles = GameObject.Find("RainParticleSystem").GetComponent<ParticleSystem>();
            var mainModule = particles.main;
            mainModule.gravityModifier = -0.03f;
            mainModule.startSpeed = new ParticleSystem.MinMaxCurve(2f);
            mainModule.maxParticles = 4000;
            mainModule.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 2.5f);
            var emitterModule = particles.emission;
            emitterModule.rateOverTime = new ParticleSystem.MinMaxCurve(4000);
            FinaleState = false;
        }
    }
}
