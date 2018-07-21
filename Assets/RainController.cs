using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Midi;
using System;
using System.Diagnostics;

public class RainController : MonoBehaviour
{
    GameObject UserMesh;
    MeshFilter UserMeshFilter;

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
        UserMesh = GameObject.Find("UserMesh");
        UserMeshFilter = UserMesh.GetComponent<MeshFilter>();

        //AirSticks.Right.NoteOn += HoneOn;
        //AirSticks.Right.NoteOff += HoneOff;
        AirSticks.Right.NoteOn += DoHitOut;
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
            Wind.transform.localRotation = Quaternion.Euler(
                (AirSticks.Right.EulerAngles.x * 200) * RotationMultiply.x,
                (AirSticks.Right.EulerAngles.y * 200) * RotationMultiply.y,
                (AirSticks.Right.EulerAngles.z * 200) * RotationMultiply.z
            );
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

                ParticleSystem.GetParticles(ParticleArray);
                var userMeshVertices = UserMeshFilter.mesh.vertices;
                // calculate the positions to flock to
                int positionsDamped = 0;
                for (int i = 0; i < ParticleArray.Length; i++)
                {
                    int vertex = (i * (userMeshVertices.Length / ParticleArray.Length) * 2) % userMeshVertices.Length;
                    var vertexPosition = new Vector3(
                        (userMeshVertices[vertex].x * Scaling.x) + HonedOffset.x,
                        (userMeshVertices[vertex].y * Scaling.y) + HonedOffset.y,
                        (userMeshVertices[vertex].z * Scaling.z) + HonedOffset.z
                    );
                    bool damped;
                    ParticleArray[i].position = DampPosition(ParticleArray[i].position, vertexPosition, HoneDamping, 0.01f, out damped);
                    if (damped) positionsDamped++;
                }
                ParticleSystem.SetParticles(ParticleArray, ParticleArray.Length);
                if (Mathf.Abs(HoneDamping - HoneDampingSpeed.y) > 0)
                {
                    HoneDamping = HoneDamping + (HoneDampingSpeed.y - HoneDamping) / HoneDampingSpeedDamp;
                }
                if (positionsDamped == 0)
                {
                    Revert = true;
                    if (HoneCallback != null)
                    {
                        HoneCallback.Invoke();
                        HoneCallback = null;
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
