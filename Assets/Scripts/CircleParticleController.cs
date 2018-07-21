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
    public static CircleParticleController Instance;

    public GameObject ParticlesObject;
    public ParticleSystem ParticleSystem;

    public bool Visible = false;
    public bool Expanded = false;
    public bool Increasing = false;
    public float IncreaseRate = 0.01f;
    private Vector2 ExpandedScale = new Vector2(1f, 1f);
    public float ExpandedScaleDamping = 20f;

    public bool EmitConstantly = true;
    public bool CentrePosition = false;

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

    public bool BangRotation = false;
    public float RotationIncrement = 8f;

    public bool ControlTrailThickness = false;
    public Vector2 ThicknessMinMax = new Vector2(0.3f, 2);

void Start()
    {
        Instance = this;
        AirSticks.Right.NoteOn += () => EnableEmission();
        AirSticks.Right.NoteOff += () => DisableEmission();
        ParticleSystem.gameObject.SetActive(false);
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
        ParticleSystem.gameObject.SetActive(Visible);

        if (Expanded)
        {
            if (!Increasing)
            {
                ExpandedScale.y = 1f;
            } else
            {
                ExpandedScale.y += IncreaseRate;
            }
            CentrePosition = true;
            EmitConstantly = true;

            var mainModule = ParticleSystem.main;
            mainModule.simulationSpeed = 0.5f;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.Local;
            mainModule.startLifetime = new MinMaxCurve(0.2f, 1f);

        } else
        {
            ExpandedScale.y = 0.01f;

            var mainModule = ParticleSystem.main;
            mainModule.simulationSpeed = 1f;
            mainModule.simulationSpace = ParticleSystemSimulationSpace.World;
            mainModule.startLifetime = new MinMaxCurve(0.2f, 0.3f);
        }

        if (BangRotation)
        {
            var euler = transform.rotation.eulerAngles;
            euler.z = euler.z + RotationIncrement;
            var rotation = Quaternion.Euler(euler);
            transform.SetPositionAndRotation(Vector3.zero, rotation);
            BangRotation = false;
        }

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

            if (!CentrePosition)
            {
                ParticleSystem.transform.position = new Vector3(
                        (averageX * SpherePositionMultiplier.x) + SpherePositionOffset.x,
                        (averageY * SpherePositionMultiplier.y) + SpherePositionOffset.y,
                        (averageZ * SpherePositionMultiplier.z) + SpherePositionOffset.z);
            } else
            {
                ParticleSystem.transform.position = new Vector3(
                        SpherePositionOffset.x,
                        SpherePositionOffset.y,
                        SpherePositionOffset.z);
            }

            if (ControlSphereSize)
            {
                Vector3 outputSize = Vector3.zero;

                outputSize.x = Mathf.Abs(rightPosition.x - leftPosition.x);
                outputSize.y = Mathf.Abs(rightPosition.y - leftPosition.y);
                outputSize.z = Mathf.Abs(rightPosition.z - leftPosition.z);

                var shapeModule = ParticleSystem.shape;
                if (!Expanded)
                    shapeModule.scale = new Vector3(outputSize.x * SphereSizeMultiplier.x, outputSize.y * SphereSizeMultiplier.y, outputSize.z * SphereSizeMultiplier.z);
                else
                {
                    if (ExpandedScale.x != ExpandedScale.y)
                    {
                        ExpandedScale.x = ExpandedScale.x + (ExpandedScale.y - ExpandedScale.x) / ExpandedScaleDamping;
                    }
                    shapeModule.scale = new Vector3(1 * ExpandedScale.x, 1 * ExpandedScale.x, 2 * ExpandedScale.x);
                }
            }

            if (ControlTrailThickness)
            {
                var trailModule = ParticleSystem.trails;
                var shiftedRotation = (AirSticks.Left.EulerAngles.x + 1) / 2;
                var mappedThickness = shiftedRotation.Map(0, 1, ThicknessMinMax.x, ThicknessMinMax.y);
                trailModule.widthOverTrail = new MinMaxCurve(mappedThickness, 0.1f);
            }

            if (EmissionEnabled || EmitConstantly) ParticleSystem.Emit(ParticleEmissionCount);
        }
    }
}
