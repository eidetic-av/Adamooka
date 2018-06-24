using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic;
using Eidetic.Unity.Utility;
using Midi;
using Utility;
using static UnityEngine.ParticleSystem;

public class CircleParticleController : MonoBehaviour
{

    public GameObject ParticlesObject;
    public ParticleSystem ParticleSystem;

    public bool EmitConstantly = true;

    public bool TrackAirsticks = false;

    public Vector3 LeftMultiplier = new Vector3(0, 0, 0);
    public Vector3 RightMultiplier = new Vector3(0, 0, 0);

    public AirSticks.Hand TrackingHand = AirSticks.Hand.Left;
    public int ParticleEmissionCount = 500;

    public bool ControlSphereSize = false;
    public Vector2 SphereSize = new Vector2(0.01f, 1f);
    public Vector3 SphereSizeMultiplier = new Vector3(1, 1, 1);

    public Vector3 SpherePositionOffset = Vector3.zero;
    public Vector3 SpherePositionMultiplier = Vector3.zero;

    private bool EmissionEnabled = false;

void Start()
    {
        AirSticks.Right.NoteOn += () => EnableEmission();
        AirSticks.Right.NoteOff += () => DisableEmission();

    }

    void EnableEmission()
    {
        EmissionEnabled = true;
        Debug.Log(EmissionEnabled);
    }

    void DisableEmission()
    {
        EmissionEnabled = false;
        Debug.Log(EmissionEnabled);
    }

    void Update()
    {
        if (TrackAirsticks)
        {
            var leftPosition = new Vector3(
                (-AirSticks.Left.Position.x * LeftMultiplier.x),
                (AirSticks.Left.Position.y * LeftMultiplier.y),
                (AirSticks.Left.Position.z * LeftMultiplier.z));

            var rightPosition = new Vector3(
                (-AirSticks.Right.Position.x * RightMultiplier.x),
                (AirSticks.Right.Position.y * RightMultiplier.y),
                (AirSticks.Right.Position.z * RightMultiplier.z));

            var averageX = (leftPosition.x + rightPosition.x) / 2;
            var averageY = (leftPosition.x + rightPosition.x) / 2;
            var averageZ = (leftPosition.x + rightPosition.x) / 2;
            
            ParticleSystem.transform.position = new Vector3(
                    (averageX * SpherePositionMultiplier.x) + SpherePositionOffset.x,
                    (averageY * SpherePositionMultiplier.y) + SpherePositionOffset.y,
                    (averageZ * SpherePositionMultiplier.z) + SpherePositionOffset.z);

            if (ControlSphereSize)
            {
                Vector3 outputSize = Vector3.zero;

                outputSize.x = Mathf.Abs(rightPosition.x - leftPosition.x);
                outputSize.y = Mathf.Abs(rightPosition.y - leftPosition.y);
                outputSize.z = Mathf.Abs(rightPosition.z - leftPosition.z);

                var shapeModule = ParticleSystem.shape;
                shapeModule.scale = new Vector3(outputSize.x * SphereSizeMultiplier.x, outputSize.y * SphereSizeMultiplier.y, outputSize.z * SphereSizeMultiplier.z);
            }

            if (EmissionEnabled || EmitConstantly) ParticleSystem.Emit(ParticleEmissionCount);
        }
    }
}
