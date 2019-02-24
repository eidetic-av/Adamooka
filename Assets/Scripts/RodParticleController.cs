using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic;
using Eidetic.Andamooka;
using Eidetic.Unity.Utility;
using Midi;
using Utility;
using static UnityEngine.ParticleSystem;

public class RodParticleController : MonoBehaviour
{
    public static RodParticleController Instance;

    public bool ConstantEmission = false;
    public bool EmissionEnabled = false;

    public bool TrackAirsticks = true;

    public Vector3 LeftHandOffset = new Vector3(0, 0, 0);
    public Vector3 RightHandOffset = new Vector3(0, 0, 0);

    public Vector3 LeftMultiplier = new Vector3(0, 0, 0);
    public Vector3 RightMultiplier = new Vector3(0, 0, 0);

    public int ParticleEmissionCount = 3;

    public Vector3 ScaleMultiplier = new Vector3(0.1f, 0.1f, 0.1f);
    public Vector2 AirsticksScaleMapX;
    public Vector2 AirsticksScaleMapY;
    public Vector2 AirsticksScaleMapZ;

    public Vector2 AirsticksRotationMinMax = new Vector2(0, 1);

    void Start()
    {
        Instance = this;
        AirSticks.Right.NoteOn += () => EmissionEnabled = true;
        AirSticks.Right.NoteOff += () => EmissionEnabled = false;
    }

    void Update()
    {
        if (TrackAirsticks)
        {
            Vector3 scale = ScaleMultiplier;

            Vector3 position = Vector3.zero;

            if (Time.frameCount % 2 == 0)
            {
                position = transform.position = new Vector3(
                    (-AirSticks.Left.Position.x * LeftMultiplier.x) + LeftHandOffset.x,
                    (AirSticks.Left.Position.y * LeftMultiplier.y) + LeftHandOffset.y,
                    (AirSticks.Left.Position.z * LeftMultiplier.z) + LeftHandOffset.z);
                    
                var mappedX = AirSticks.Left.EulerAngles.z.Map(AirsticksRotationMinMax.x, AirsticksRotationMinMax.y, AirsticksScaleMapX.x, AirsticksScaleMapX.y);
                var mappedY = AirSticks.Left.EulerAngles.z.Map(AirsticksRotationMinMax.x, AirsticksRotationMinMax.y, AirsticksScaleMapY.x, AirsticksScaleMapY.y);
                var mappedZ = AirSticks.Left.EulerAngles.z.Map(AirsticksRotationMinMax.x, AirsticksRotationMinMax.y, AirsticksScaleMapZ.x, AirsticksScaleMapZ.y);
                scale.Scale(new Vector3(mappedX, mappedY, mappedZ));
            }
            else
            {
                position = transform.position = new Vector3(
                    (-AirSticks.Right.Position.x * RightMultiplier.x) + RightHandOffset.x,
                    (AirSticks.Right.Position.y * RightMultiplier.y) + RightHandOffset.y,
                    (AirSticks.Right.Position.z * RightMultiplier.z) + RightHandOffset.z);

                var mappedX = AirSticks.Right.EulerAngles.z.Map(AirsticksRotationMinMax.x, AirsticksRotationMinMax.y, AirsticksScaleMapX.x, AirsticksScaleMapX.y);
                var mappedY = AirSticks.Right.EulerAngles.z.Map(AirsticksRotationMinMax.x, AirsticksRotationMinMax.y, AirsticksScaleMapY.x, AirsticksScaleMapY.y);
                var mappedZ = AirSticks.Right.EulerAngles.z.Map(AirsticksRotationMinMax.x, AirsticksRotationMinMax.y, AirsticksScaleMapZ.x, AirsticksScaleMapZ.y);
                scale.Scale(new Vector3(mappedX, mappedY, mappedZ));
            }

            // var shapeModule = GetComponent<ParticleSystem>().shape;
            // shapeModule.scale = scale;
        }
        else
        {
            var shapeModule = GetComponent<ParticleSystem>().shape;
            shapeModule.scale = ScaleMultiplier;
        }

        // if (EmissionEnabled || ConstantEmission)
        //     GetComponent<ParticleSystem>().Emit(ParticleEmissionCount);
    }
}
