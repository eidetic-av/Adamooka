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
    private Renderer Renderer;

    public bool ConstantEmission = false;
    public bool EmissionEnabled = false;

    public bool TrackAirsticks = true;

    public Vector3 LeftHandOffset = new Vector3(0, 0, 0);
    public Vector3 RightHandOffset = new Vector3(0, 0, 0);

    public Vector3 LeftMultiplier = new Vector3(0, 0, 0);
    public Vector3 RightMultiplier = new Vector3(0, 0, 0);

    public AirSticks.Hand TrackingHand = AirSticks.Hand.Left;
    public int ParticleEmissionCount = 3;

    public Vector3 BaseHandScale = new Vector3(0.1f, 0.1f, 0.1f);

    public bool AirsticksScalingEnabled = true;
    public Vector2 AirsticksScaleMapX;
    public Vector2 AirsticksScaleMapY;
    public Vector2 AirsticksScaleMapZ;

    public Vector2 AirsticksRotationMinMax = new Vector2(0, 1);

    public Color NewParticleColor = Color.white;
    public bool SetColor = false;

    void Start()
    {
        Instance = this;
        AirSticks.Right.NoteOn += () => EnableEmission();
        AirSticks.Right.NoteOff += () => DisableEmission();

        Renderer = ParticleSystem.GetComponent<Renderer>();
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
        if (SetColor)
        {
            Renderer.material.SetColor("_Color", NewParticleColor);
            SetColor = false;
        }

        if (ConstantEmission) EmissionEnabled = true;
        if (TrackAirsticks)
        {
            Vector3 modulatedScale = BaseHandScale;

            switch (TrackingHand)
            {
                case AirSticks.Hand.Left:
                    {
                        // move to hand position
                        ParticlesObject.transform.position = new Vector3(
                            (-AirSticks.Left.Position.x * LeftMultiplier.x) + LeftHandOffset.x, 
                            (AirSticks.Left.Position.y * LeftMultiplier.y) + LeftHandOffset.y,
                            (AirSticks.Left.Position.z * LeftMultiplier.z) + LeftHandOffset.z);
                        // set shape scale
                        var mappedX = AirSticks.Left.EulerAngles.z.Map(AirsticksRotationMinMax.x, AirsticksRotationMinMax.y, AirsticksScaleMapX.x, AirsticksScaleMapX.y);
                        var mappedY = AirSticks.Left.EulerAngles.z.Map(AirsticksRotationMinMax.x, AirsticksRotationMinMax.y, AirsticksScaleMapY.x, AirsticksScaleMapY.y);
                        var mappedZ = AirSticks.Left.EulerAngles.z.Map(AirsticksRotationMinMax.x, AirsticksRotationMinMax.y, AirsticksScaleMapZ.x, AirsticksScaleMapZ.y);
                        modulatedScale.Scale(new Vector3(mappedX, mappedY, mappedZ));
                        // and swap to other hand for next frame
                        TrackingHand = AirSticks.Hand.Right;
                        break;
                    }
                case AirSticks.Hand.Right:
                    {
                        ParticlesObject.transform.position = new Vector3(
                            (-AirSticks.Right.Position.x * RightMultiplier.x) + RightHandOffset.x, 
                            (AirSticks.Right.Position.y * RightMultiplier.y) + RightHandOffset.y,
                            (AirSticks.Right.Position.z * RightMultiplier.z) + RightHandOffset.z);
                        // set shape scale
                        var mappedX = AirSticks.Right.EulerAngles.z.Map(AirsticksRotationMinMax.x, AirsticksRotationMinMax.y, AirsticksScaleMapX.x, AirsticksScaleMapX.y);
                        var mappedY = AirSticks.Right.EulerAngles.z.Map(AirsticksRotationMinMax.x, AirsticksRotationMinMax.y, AirsticksScaleMapY.x, AirsticksScaleMapY.y);
                        var mappedZ = AirSticks.Right.EulerAngles.z.Map(AirsticksRotationMinMax.x, AirsticksRotationMinMax.y, AirsticksScaleMapZ.x, AirsticksScaleMapZ.y);
                        modulatedScale.Scale(new Vector3(mappedX, mappedY, mappedZ));
                        TrackingHand = AirSticks.Hand.Left;
                        break;
                    }
            }

            // Set the scale of the emission shape
            var shapeModule = ParticleSystem.shape;
            if (AirsticksScalingEnabled)
            {
                shapeModule.scale = modulatedScale;
            } else
            {
                shapeModule.scale = BaseHandScale;
            }

            if (EmissionEnabled) ParticleSystem.Emit(ParticleEmissionCount);

        }
    }
}
