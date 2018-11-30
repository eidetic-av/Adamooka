using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BondiCueController : MonoBehaviour
{
    public static BondiCueController Instance;

    public bool EnableRodsAfterSpeech = false;
    public bool TwoHandedRodMelody = false;
    public bool EnableVortex = false;
    public bool DisableRodsForDrums = false;
    public bool EnableRing = false;
    public bool DisableRingWithLowVoice = false;
    public bool EnableRodsInSilence = false;
    public bool DisableRodsWithGrowl = false;
    public bool EnableRodAndRingNoteOns = false;
    public bool DisableRodsAfterMelody = false;
    public bool DisableVortex = false;
    public bool FadeOutNoiseCircle = false;
    void Start()
    {
        Instance = this;
    }

    void Update()
    {
        if (EnableRodsAfterSpeech)
        {
            RodParticleController.Instance.TrackAirsticks = true;
            RodParticleController.Instance.ConstantEmission = false;
            EnableRodsAfterSpeech = false;
        }
        if (TwoHandedRodMelody) {
            RodParticleController.Instance.TrackAirsticks = true;
            RodParticleController.Instance.ConstantEmission = true;
            TwoHandedRodMelody = false;
        }
        if (EnableVortex) {
            RodParticleController.Instance.TrackAirsticks = true;
            RodParticleController.Instance.ConstantEmission = false;
            CircleParticleController.Instance.StartSystem = true;
            EnableVortex = false;
        }
        if (DisableRodsForDrums) {
            RodParticleController.Instance.TrackAirsticks = false;
            RodParticleController.Instance.ConstantEmission = false;
            DisableRodsForDrums = false;
        }
        if (EnableRing) {
            NoiseCircleController.Instance.StartSystem = true;
            OneFiveNineCircleController.Instance.ActivateAirSticksKickSnare = true;
            EnableRing = false;
        }
        if (DisableRingWithLowVoice) {
            OneFiveNineCircleController.Instance.DeactivateAirSticksKickSnare = true;
            DisableRingWithLowVoice = false;
        }
        if (EnableRodsInSilence) {
            RodParticleController.Instance.TrackAirsticks = true;
            RodParticleController.Instance.ConstantEmission = false;
            EnableRodsInSilence = false;
        }
        if (DisableRodsWithGrowl) {
            RodParticleController.Instance.TrackAirsticks = false;
            RodParticleController.Instance.ConstantEmission = false;
            DisableRodsWithGrowl = false;
        }
        if (EnableRodAndRingNoteOns) {
            RodParticleController.Instance.TrackAirsticks = true;
            OneFiveNineCircleController.Instance.ActivateAirSticksKickSnare = true;
            EnableRodAndRingNoteOns = false;
        }
        if (DisableRodsAfterMelody) {
            RodParticleController.Instance.TrackAirsticks = false;
            RodParticleController.Instance.TrackAirsticks = false;
            DisableRodsAfterMelody = false;
        }
        if (DisableVortex) {
            CircleParticleController.Instance.StopSystem = true;
            DisableVortex = false;
        }
        if (FadeOutNoiseCircle) {
            NoiseCircleController.Instance.StopSystem = true;
            FadeOutNoiseCircle = false;
        }
    }
}
