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
    public float RotationDamping = 5f;
    private Vector2 Rotation = Vector2.zero;

    public bool ControlTrailThickness = false;
    public Vector2 ThicknessMinMax = new Vector2(0.3f, 2);
    public bool ControlScaleFromMembrane = false;
    public float ScaleMultiplier = 1f;

    public bool StartSystem = false;

    public bool StartInvasion = false;
    public float InvasionLength = 75;
    public float InvasionMaxParticles = 500;
    private bool Invading = false;
    private float InvasionStartTime;
    private int InvasionStartMaxParticles;
    private float InvasionStartRadiusThickness;
    public bool StartInvasionRevert = false;
    public float InvasionRevertDamp = 5f;
    public float InvasionSimmerLength = 18;
    private bool InvasionReverting = false;
    private float InvasionRevertStartTime;
    public Vector2 ScaleBangOnInvasionRevert = new Vector2(0.25f, 0.128f);

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

        if (StartSystem) {
            var mainModule = ParticleSystem.main;
            mainModule.maxParticles = 150;
            StartSystem = false;
        }

        if (StartInvasion) {
            Invading = true;
            InvasionStartTime = Time.time;
            InvasionStartMaxParticles = ParticleSystem.main.maxParticles;
            var shapeModule = ParticleSystem.shape;
            InvasionStartRadiusThickness = shapeModule.radiusThickness;
            StartInvasion = false;
        }
        if (Invading) {
            var invadedAmount = (Time.time - InvasionStartTime) / InvasionLength;
            if (invadedAmount >= 1) {
                invadedAmount = 1;
                Invading = false;
            }
            var shapeModule = ParticleSystem.shape;
            // fill radius thickness to 1 over specified time
            shapeModule.radiusThickness = InvasionStartRadiusThickness + ((1 - InvasionStartRadiusThickness) * invadedAmount);
            // and increase to the desired max particles
            var main = ParticleSystem.main;
            main.maxParticles = Mathf.RoundToInt(InvasionStartMaxParticles + ((InvasionMaxParticles - InvasionStartMaxParticles) * invadedAmount));
        }
        if (StartInvasionRevert) {
            Invading = false;
            InvasionRevertStartTime = Time.time;
            var scale = ParticleSystem.transform.localScale;
            scale.x = ScaleBangOnInvasionRevert.x;
            scale.y = ScaleBangOnInvasionRevert.x;
            ParticleSystem.transform.localScale = scale;
            InvasionReverting = true;
            StartInvasionRevert = false;
        }
        if (InvasionReverting) {
            var shapeModule = ParticleSystem.shape;
            shapeModule.radiusThickness = shapeModule.radiusThickness + (InvasionStartRadiusThickness - shapeModule.radiusThickness) / InvasionRevertDamp;
            var scaleValue = ParticleSystem.transform.localScale.x;
            scaleValue = scaleValue + (ScaleBangOnInvasionRevert.y - scaleValue) / InvasionRevertDamp;
            var scale = ParticleSystem.transform.localScale;
            scale.x = scale.y = scaleValue;
            ParticleSystem.transform.localScale = scale;
            var simmerAmount = (Time.time - InvasionRevertStartTime) / InvasionSimmerLength;
            if (simmerAmount >= 1) {
                simmerAmount = 1;
                InvasionReverting = false;
            }
            var main = ParticleSystem.main;
            main.maxParticles = Mathf.RoundToInt(InvasionMaxParticles - ((InvasionMaxParticles - InvasionStartMaxParticles) * simmerAmount));
        }

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
            Rotation.y = Rotation.y + RotationIncrement;
            BangRotation = false;
        }

        Rotation.x = Rotation.x + (Rotation.y - Rotation.x) / RotationDamping;

        var euler = transform.rotation.eulerAngles;
        euler.z = Rotation.x;
        var quaternion = Quaternion.Euler(euler);
        transform.SetPositionAndRotation(Vector3.zero, quaternion);

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

            if (ControlScaleFromMembrane) {
                var scale = ParticleSystem.transform.localScale;
                scale.x = MembraneController.Instance.Radius.x * ScaleMultiplier;
                scale.y = MembraneController.Instance.Radius.x * ScaleMultiplier;
                ParticleSystem.transform.localScale = scale;
            }

            if (EmissionEnabled || EmitConstantly) ParticleSystem.Emit(ParticleEmissionCount);
        }
    }
}
