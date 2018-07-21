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

    private void Start()
    {
        Instance = this;
    }

    void Update()
    {
        if (IntroVisuals)
        {
            RipParticleController.Instance.ConstantEmission = false;
            RipParticleController.Instance.TrackAirsticks = false;
            CircleParticleController.Instance.Visible = true;
            CircleParticleController.Instance.ControlSphereSize = true;
            CircleParticleController.Instance.Expanded = false;
            CircleParticleController.Instance.Increasing = false;
            CircleParticleController.Instance.EmitConstantly = false;
            CircleParticleController.Instance.TrackAirsticks = true;
            OneFiveNineCircleController.Instance.ResetSceneLight = true;
            OneFiveNineCircleController.Instance.HideRings = true;
            IntroVisuals = false;
        }
        else if (IncreaseExpandCircle)
        {
            CircleParticleController.Instance.Expanded = true;
            CircleParticleController.Instance.Increasing = true;
            IncreaseExpandCircle = false;
        }
        else if (EnableRods)
        {
            RipParticleController.Instance.TrackAirsticks = true;
            RipParticleController.Instance.ConstantEmission = false;
            EnableRods = false;
        } else if (DisableRods)
        {
            RipParticleController.Instance.TrackAirsticks = false;
            RipParticleController.Instance.ConstantEmission = false;
            DisableRods = false;
        }
        else if (KickAndSnareWithRing)
        {
            OneFiveNineCircleController.Instance.ActivateScene = true;
            CircleParticleController.Instance.Visible = true;
            RipParticleController.Instance.TrackAirsticks = false;
            KickAndSnareWithRing = false;
        }
        else if (Breakdown)
        {
            RipParticleController.Instance.TrackAirsticks = true;
            RipParticleController.Instance.ConstantEmission = true;
            OneFiveNineCircleController.Instance.ResetSceneLight = true;
            OneFiveNineCircleController.Instance.SendTrackingToSceneLight = false;
            OneFiveNineCircleController.Instance.SendSnareToParticles = false;
            CircleParticleController.Instance.Visible = true;
            CircleParticleController.Instance.Increasing = false;
            CircleParticleController.Instance.Expanded = true;
            CircleParticleController.Instance.EmitConstantly = true;
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
            CircleParticleController.Instance.Increasing = true;
            RipParticleController.Instance.ConstantEmission = false;
            StartOneFiveNineOut = false;
        }
        else if (OneFiveNineOut)
        {
            OneFiveNineCircleController.Instance.HideRings = true;
            OneFiveNineCircleController.Instance.ResetSceneLight = true;
            OneFiveNineCircleController.Instance.SendTrackingToSceneLight = false;
            OneFiveNineCircleController.Instance.SendSnareToParticles = false;
            RipParticleController.Instance.TrackAirsticks = false;
            CircleParticleController.Instance.Visible = false;
            if (GameObject.Find("Nonagon") != null)
                GameObject.Find("Nonagon").SetActive(false);
            if (GameObject.Find("LightPlane") != null)
                GameObject.Find("LightPlane").SetActive(false);
            OneFiveNineOut = false;
        } else if (ActivateRain)
        {
            MidiManager.Instance.ActivateRainState();
            ActivateRain = false;
        }
    }
}
