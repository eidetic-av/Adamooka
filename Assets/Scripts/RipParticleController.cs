using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic;
using Eidetic.Unity.Utility;
using Midi;
using Utility;
using static UnityEngine.ParticleSystem;

public class RipParticleController : MonoBehaviour
{

    public static RipParticleController Instance;

    public GameObject ParticlesObject;
    public ParticleSystem ParticleSystem;

    public bool ConstantEmission = false;
    public bool EmissionEnabled = false;

    public bool TrackAirsticks = true;

    public Vector3 LeftHandOffset = new Vector3(0, 0, 0);
    public Vector3 RightHandOffset = new Vector3(0, 0, 0);

    public Vector3 LeftMultiplier = new Vector3(0, 0, 0);
    public Vector3 RightMultiplier = new Vector3(0, 0, 0);

    public AirSticks.Hand TrackingHand = AirSticks.Hand.Left;
    public int ParticleEmissionCount = 3;

    void Start()
    {
        Instance = this;
        AirSticks.Right.NoteOn += () => EnableEmission();
        AirSticks.Right.NoteOff += () => DisableEmission();

    }

    void EnableEmission()
    {
        EmissionEnabled = true;
    }

    void DisableEmission()
    {
        EmissionEnabled = false;
    }

    void Update()
    {
        if (ConstantEmission) EmissionEnabled = true;
        if (TrackAirsticks)
        {
            switch (TrackingHand)
            {
                case AirSticks.Hand.Left:
                    {
                        ParticlesObject.transform.position = new Vector3(
                            (-AirSticks.Left.Position.x * LeftMultiplier.x) + LeftHandOffset.x, 
                            (AirSticks.Left.Position.y * LeftMultiplier.y) + LeftHandOffset.y,
                            (AirSticks.Left.Position.z * LeftMultiplier.z) + LeftHandOffset.z);

                        if (EmissionEnabled) ParticleSystem.Emit(ParticleEmissionCount);
                        TrackingHand = AirSticks.Hand.Right;
                        break;
                    }
                case AirSticks.Hand.Right:
                    {
                        ParticlesObject.transform.position = new Vector3(
                            (-AirSticks.Right.Position.x * RightMultiplier.x) + RightHandOffset.x, 
                            (AirSticks.Right.Position.y * RightMultiplier.y) + RightHandOffset.y,
                            (AirSticks.Right.Position.z * RightMultiplier.z) + RightHandOffset.z);

                        if (EmissionEnabled) ParticleSystem.Emit(ParticleEmissionCount);
                        TrackingHand = AirSticks.Hand.Left;
                        break;
                    }
            }

        }
    }
}
