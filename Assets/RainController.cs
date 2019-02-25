using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Midi;
using System;
using System.Diagnostics;
using Utility;
using Eidetic.Andamooka;

public class RainController : MonoBehaviour
{
    public static RainController Instance;

    public bool EnableEmission = false;
    public bool DisableEmission = false;
    public int EmissionCount = 2000;

    public Gradient DesmondParticleColor;
    public bool ActivateDesmondParticleColor = false;
    public bool ActivateDefaultParticleColour = false;
    public bool ActivateOutroState = false;

    public bool TransitioningToOutro = false;
    private float OutroTransitionPosition = 0;
    public float OutroTransitionDamping = 5f;
    public bool StartCloneTransition = false;
    public float CloneTransitionLength = 5f;
    public AnimationCurve CloneTransitionCurve = AnimationCurve.EaseInOut(0, 1, 0, 1);
    private bool TransitioningToClone = false;
    private float CloneTransitionStartTime;

    public bool StartCloneDispersion = false;
    public float CloneDispersionLength = 3f;
    public AnimationCurve CloneDispersionCurve = AnimationCurve.EaseInOut(0, 1, 0, 1);
    private bool DispersingToClone = false;
    private float CloneDispersionStartTime;

    public bool SlowlyStopParticles = false;
    public float StopParticleLength = 3f;
    public AnimationCurve StopParticleCurve = AnimationCurve.EaseInOut(0, 1, 0, 1);
    private bool StoppingParticles = false;
    private float StopParticleStartTime;
    public Vector2Int StopParticleAmounts = new Vector2Int(2000, 1000);


    public GameObject Wind;
    public GameObject Particles;

    ParticleSystem ParticleSystem;

    ParticleSystem.Particle[] ParticleArray;
    Vector3[] NewParticlePositions;

    ParticleSystem.MinMaxCurve OriginalParticleEmitRate;
    ParticleSystem.MinMaxCurve OriginalLifetime;
    float OriginalParticleForceMultiplier;
    Vector3[] OriginalParticlePositions;

    bool OriginalNoiseEnabled;

    public bool RadialModifier = false;
    [Range(-2f, 2f)]
    public float RadialModifierIntesity = 0f;

    public KinectInterop.JointType RadialOriginJoint;
    public Vector3 JointScaling;
    public Vector3 JointOffset;

    public Vector3 Scaling = new Vector3(0, 0, 0);
    public Vector3 HonedOffset = new Vector3(0, 0, 0);


    float HoneDamping = 10f;
    public Vector2 HoneDampingSpeed = new Vector2(5f, 1f);
    public float HoneDampingSpeedDamp = 8f;
    public float RevertDamping = 10f;

    public float RevertThreshold = 0.05f;

    public Vector3 RotationMultiply = new Vector3(1f, 1f, 1f);
    public Vector3 RotationOffset = Vector3.zero;
    public Vector4 AirSticksXMap = new Vector4(0, 1, 0, 1);
    public Vector2 AirSticksXClamp = new Vector2(0, 1);

    public bool AirSticksGravityControl = true;    
    public Vector4 AirSticksGravityMap = new Vector4(-1, 1, -0.5f, 0.3f);

    float ShapeSize = 10f;
    float NewShapeSize = 10f;
    float ShapeSizeDamp = 1f;

    public bool Control = true;
    public bool Hone;
    public bool Revert;
    bool DoingHoneOff;

    Stopwatch ReversionTimer = new Stopwatch();
    public int ReversionMilliseconds = 750;

    public Action HoneCallback;

    public bool HitOut = false;
    public bool HitOutSecondStage = false;

    public bool AirStickHoneOnHit = false;
    private bool SetAirsticksHit = false;


    // Use this for initialization
    void Start()
    {
        Instance = this;

        ParticleSystem = Particles.GetComponent<ParticleSystem>();

        // UserMesh = GameObject.Find("UserMesh");
        // UserMeshFilter = UserMesh.GetComponent<MeshFilter>();

        //AirSticks.Right.NoteOn += HoneOn;
        //AirSticks.Right.NoteOff += HoneOff;
        // AirSticks.Right.NoteOn += DoHitOut;
    }

    void DoHitOut()
    {
        if (AirStickHoneOnHit)
        {
            SetAirsticksHit = true;
        }
    }

    void Update()
    {
        if (SetAirsticksHit)
        {
            HitOut = true;
            SetAirsticksHit = false;
        }

        if (TransitioningToOutro)
        {
            OutroTransitionPosition = OutroTransitionPosition + (1 - OutroTransitionPosition) / OutroTransitionDamping;
            if (OutroTransitionPosition >= 0.99)
            {
                ProceduralMeshController.Instance.Interpolation = 1;
                ParticleSystemRenderer renderer = ParticleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.maxParticleSize = 0.003f;
                ProceduralMeshController.Instance.ControlInterpolationWithAirSticks = true;
                var externalForces = ParticleSystem.externalForces;
                externalForces.multiplier = 5f;
                TransitioningToOutro = false;
                OutroTransitionPosition = 0;
            }
            else
            {
                ProceduralMeshController.Instance.Interpolation = OutroTransitionPosition;
                ParticleSystemRenderer renderer = ParticleSystem.GetComponent<ParticleSystemRenderer>();
                renderer.maxParticleSize = OutroTransitionPosition.Map(0f, 1f, 0.01f, 0.003f);
            }
        }

        if (EnableEmission)
        {
            var particleSystem = Particles.GetComponent<ParticleSystem>();
            var emissionModule = particleSystem.emission;
            emissionModule.rateOverTime = EmissionCount;
            // particleSystem.Emit(EmissionCount);
            EnableEmission = false;
        }
        if (DisableEmission)
        {
            var emissionModule = Particles.GetComponent<ParticleSystem>().emission;
            emissionModule.rateOverTime = 0;
            DisableEmission = false;
        }
        if (ActivateDesmondParticleColor)
        {
            var particleSystem = Particles.GetComponent<ParticleSystem>();
            var mainModule = particleSystem.main;
            var desmondGradient = new ParticleSystem.MinMaxGradient(DesmondParticleColor);
            desmondGradient.mode = ParticleSystemGradientMode.RandomColor;
            mainModule.startColor = desmondGradient;
            mainModule.gravityModifierMultiplier = 0.3f;
            ActivateDesmondParticleColor = false;
        }
        if (ActivateDefaultParticleColour)
        {
            var particleSystem = Particles.GetComponent<ParticleSystem>();
            var mainModule = particleSystem.main;
            var desmondGradient = new ParticleSystem.MinMaxGradient(Color.white);
            desmondGradient.mode = ParticleSystemGradientMode.Color;
            mainModule.startColor = Color.white;
            ActivateDefaultParticleColour = false;
        }

        if (ActivateOutroState)
        {
            EnableEmission = true;
            
            EmissionCount = 2000;
            var emissionModule = ParticleSystem.emission;
            emissionModule.rateOverTime = EmissionCount;
            var mainModule = ParticleSystem.main;
            mainModule.maxParticles = 1000;

            ActivateDesmondParticleColor = true;
            ProceduralMeshController.Instance.ControlInterpolationWithAirSticks = false;
            ProceduralMeshController.Instance.Interpolation = 0;
            ParticleSystemRenderer renderer = ParticleSystem.GetComponent<ParticleSystemRenderer>();
            renderer.maxParticleSize = 0.003f;
            ActivateOutroState = false;
        }

        if (StartCloneTransition) {
            CloneTransitionStartTime = Time.time;
            ProceduralMeshController.Instance.ControlInterpolationWithAirSticks = false;
            TransitioningToClone = true;
            StartCloneTransition = false;
        }

        if (TransitioningToClone) {
            var position = (Time.time - CloneTransitionStartTime) / CloneTransitionLength;
            var curveValue = CloneTransitionCurve.Evaluate(position);
            if (curveValue >= 1) {
                curveValue = 1;
                TransitioningToClone = false;
            }
            var mainModule = ParticleSystem.main;
            mainModule.gravityModifierMultiplier = curveValue.Map(0f, 1f, 0.3f, 0f);
            var externalForces = ParticleSystem.externalForces;
            externalForces.multiplier = curveValue.Map(0f, 1f, 5f, 0f);
            ProceduralMeshController.Instance.Interpolation = curveValue.Map(0f, 1f, 1f, 0f);
        }

        if (StartCloneDispersion) {
            CloneDispersionStartTime = Time.time;
            ProceduralMeshController.Instance.ControlInterpolationWithAirSticks = false;
            DispersingToClone = true;
            StartCloneDispersion = false;
        }

        if (DispersingToClone) {
            var position = (Time.time - CloneDispersionStartTime) / CloneDispersionLength;
            var curveValue = CloneDispersionCurve.Evaluate(position);
            if (curveValue >= 1) {
                curveValue = 1;
                DispersingToClone = false;
            }
            var mainModule = ParticleSystem.main;
            mainModule.gravityModifierMultiplier = curveValue.Map(0f, 1f, 0, 0.3f);
            ProceduralMeshController.Instance.Interpolation = curveValue.Map(0f, 1f, 0f, 1f);
        }

        if (SlowlyStopParticles) {
            StopParticleStartTime = Time.time;
            ProceduralMeshController.Instance.ControlInterpolationWithAirSticks = false;
            StoppingParticles = true;
            SlowlyStopParticles = false;
        }

        if (StoppingParticles) {
            var position = (Time.time - StopParticleStartTime) / StopParticleLength;
            var curveValue = StopParticleCurve.Evaluate(position);
            if (curveValue >= 1) {
                curveValue = 1;
                StoppingParticles = false;
            }
            EmissionCount = Mathf.RoundToInt(curveValue.Map(0f, 1f, (float)StopParticleAmounts.x, 0f));
            var emissionModule = ParticleSystem.emission;
            emissionModule.rateOverTime = EmissionCount;
            var mainModule = ParticleSystem.main;
            mainModule.maxParticles = Mathf.RoundToInt(curveValue.Map(0f, 1f, (float)StopParticleAmounts.y, 0f));
        }

    }

    // Update is called once per frame
    void LateUpdate()
    {

        if (RadialModifier)
        {
            var manager = KinectManager.Instance;
            var origin = manager.GetJointPosition(manager.GetUserIdByIndex(0), (int)RadialOriginJoint);
            var velocityModule = ParticleSystem.velocityOverLifetime;

            velocityModule.orbitalOffsetX = (origin.x * JointScaling.x) + JointOffset.x;
            velocityModule.orbitalOffsetY = (origin.y * JointScaling.y) + JointOffset.y;
            velocityModule.orbitalOffsetZ = (origin.z * JointScaling.z) + JointOffset.z;

            velocityModule.radialMultiplier = RadialModifierIntesity;

        }
        if (HitOut)
        {
            Hone = true;
            HitOutSecondStage = true;
            HitOut = false;
        }
        else if (HitOutSecondStage)
        {
            Revert = true;
            HitOutSecondStage = false;
        }

        if (Input.GetKeyDown(KeyCode.N))
        {
            HoneOn();
        }
        else if (Input.GetKeyDown(KeyCode.M))
        {
            HoneOff();
        }
        if (Revert)
        {
            HoneOff();
            Revert = false;
        }

        if (Control)
        {
            // wind control
            var mapPosition = Mathf.Clamp(AirSticks.Left.Position.x.Map(AirSticksXMap.x, AirSticksXMap.y, AirSticksXMap.z, AirSticksXMap.w), AirSticksXClamp.x, AirSticksXClamp.y);
            var yRotation = (mapPosition * RotationMultiply.x) + RotationOffset.x;
            Wind.transform.localRotation = Quaternion.Euler(
                0f,
                (AirSticks.Left.Position.x) * RotationMultiply.x + (RotationOffset.x),
                0.0f
            );
            // gravity control
            if (AirSticksGravityControl) {
                var gravity = AirSticks.Left.EulerAngles.z.Map(AirSticksGravityMap.x, AirSticksGravityMap.y, AirSticksGravityMap.z, AirSticksGravityMap.w);
                var mainModule = ParticleSystem.main;
                mainModule.gravityModifierMultiplier = gravity;
            }
        }
        if (Hone)
        {
            if (KinectManager.Instance.GetUsersCount() != 0)
            {
                if (Wind != null)
                    Wind.SetActive(false);

                if (ParticleArray == null)
                {
                    ParticleArray = new ParticleSystem.Particle[ParticleSystem.main.maxParticles];

                    // and stop emission and forces
                    var emissionModule = ParticleSystem.emission;
                    OriginalParticleEmitRate = emissionModule.rateOverTime;
                    emissionModule.rateOverTime = 0;
                    var forcesModule = ParticleSystem.externalForces;
                    OriginalParticleForceMultiplier = forcesModule.multiplier;
                    forcesModule.multiplier = 0;
                    var noiseModule = ParticleSystem.noise;
                    OriginalNoiseEnabled = noiseModule.enabled;
                    noiseModule.enabled = false;

                    // increase the lifetime of particles and emit one burst
                    var mainModule = ParticleSystem.main;
                    OriginalLifetime = mainModule.startLifetime;
                    mainModule.startLifetime = new ParticleSystem.MinMaxCurve(1000f, 1000f);
                    ParticleSystem.Clear();
                    ParticleSystem.Emit(ParticleSystem.main.maxParticles);

                    // initialise the particle position arrays
                    var particleCount = ParticleSystem.GetParticles(ParticleArray);
                    OriginalParticlePositions = new Vector3[particleCount];
                    NewParticlePositions = new Vector3[particleCount];

                    for (int i = 0; i < particleCount; i++)
                    {
                        OriginalParticlePositions[i] = ParticleArray[i].position;
                        NewParticlePositions[i] = ParticleArray[i].position;
                    }
                }
            }
        }
        else if (DoingHoneOff)
        {
            ParticleSystem.GetParticles(ParticleArray);
            // go back to original position before hone action
            // track how many are back in original positions
            int positionsDamped = 0;
            for (int i = 0; i < ParticleArray.Length; i++)
            {
                bool damped;
                ParticleArray[i].position = DampPosition(ParticleArray[i].position, OriginalParticlePositions[i], RevertDamping, RevertThreshold, out damped);
                if (damped) positionsDamped++;
            }
            ParticleSystem.SetParticles(ParticleArray, ParticleArray.Length);
            // if we are not damping any more particles, revert all the default settings
            //if (positionsDamped == 0)
            //{
            //    RevertToDefault(ParticleArray);
            //}
            // Timer based instead of position based
            if (ReversionTimer.ElapsedMilliseconds >= ReversionMilliseconds)
            {
                RevertToDefault(ParticleArray);
                ReversionTimer.Stop();
            }
        }
    }

    void HoneOn()
    {
        Hone = true;
        DoingHoneOff = false;
    }
    void HoneOff()
    {
        Hone = false;
        DoingHoneOff = true;
        // set back to originals
        var emissionModule = ParticleSystem.emission;
        emissionModule.rateOverTime = OriginalParticleEmitRate;
        var forcesModule = ParticleSystem.externalForces;
        forcesModule.multiplier = OriginalParticleForceMultiplier;
        var mainModule = ParticleSystem.main;
        mainModule.startLifetime = OriginalLifetime;

        ReversionTimer.Restart();
    }

    void RevertToDefault(ParticleSystem.Particle[] currentParticles)
    {
        DoingHoneOff = false;
        // set wind to active
        if (Wind != null)
            Wind.SetActive(true);
        var noiseModule = ParticleSystem.noise;
        noiseModule.enabled = OriginalNoiseEnabled;
        // replace particles with new ones with original lifetime
        var particleCount = ParticleSystem.particleCount;
        ParticleSystem.Clear();
        ParticleSystem.Emit(particleCount);
        var newParticles = new ParticleSystem.Particle[particleCount];
        ParticleSystem.GetParticles(newParticles);
        // getting the positions from where they were when we stopped damping
        for (int i = 0; i < particleCount; i++)
        {
            newParticles[i].position = currentParticles[i].position;
        }
        ParticleSystem.SetParticles(newParticles, particleCount);
        // null all the arrays
        OriginalParticlePositions = null;
        NewParticlePositions = null;
        ParticleArray = null;

        HoneDamping = HoneDampingSpeed.x;
    }

    Vector3 DampPosition(Vector3 value, Vector3 goal, float dampRate, float dampThreshold, out bool damped)
    {
        damped = false;
        if (Mathf.Abs(value.x - goal.x) > dampThreshold)
        {
            value.x = value.x + (goal.x - value.x) / dampRate;
            damped = true;
        }
        if (Mathf.Abs(value.y - goal.y) > dampThreshold)
        {
            value.y = value.y + (goal.y - value.y) / dampRate;
            damped = true;
        }
        if (Mathf.Abs(value.z - goal.z) > dampThreshold)
        {
            value.z = value.z + (goal.z - value.z) / dampRate;
            damped = true;
        }
        return value;
    }
}
