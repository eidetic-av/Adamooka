using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic;
using Eidetic.Andamooka;
using Eidetic.Unity.Utility;
using Midi;
using Utility;
using static UnityEngine.ParticleSystem;

public class ParticleController : MonoBehaviour
{

    public GameObject LeftParticlesObject;
    public GameObject RightParticlesObject;
    public ParticleSystem LeftParticleSystem;
    public ParticleSystem RightParticleSystem;

    Vector3 LeftPosition, NewLeftPosition, RightPosition, NewRightPosition = Vector3.zero;
    public float PositionDampRate = 3f;

    public MeshFilter BaseMesh;

    public GameObject ParticleOutputQuad;

    public bool SetParticlesPositionsBasedOnMesh;

    public bool TriggerSetsVisibility;

    public bool Visible = false;

    public bool TrackAirsticks = true;

    public Vector3 LeftParticlesPositionOffset = new Vector3(0, 0, 4);
    public Vector3 RightParticlesPositionOffset = new Vector3(0, 0, 4);

    public Vector3 LeftParticlesKinectOffset = new Vector3(0, 0, 4);
    public Vector3 RightParticlesKinectOffset = new Vector3(0, 0, 4);

    public Vector3 LeftMultiplier = new Vector3(0, 0, 0);
    public Vector3 RightMultiplier = new Vector3(0, 0, 0);

    public ParticleSystem HitSystemA;
    public ParticleSystem HitSystemB;
    public ParticleSystem HitSystemC;

    public bool ControlHitParticlesWithKinectHands = true;
    public UserForceController UserForceController;
    public Vector2 HitSystemBTrackXInput;
    public Vector2 HitSystemBTrackXOutput;
    public Vector2 HitSystemBTrackYInput;
    public Vector2 HitSystemBTrackYOutput;
    private Vector3 HitSystemBOrigin;
    private Vector2 HitSystemBXOffset;
    private Vector2 HitSystemBYOffset;
    public float HitSystemBTrackDamping = 3f;

    void Start()
    {
        LeftParticlesObject = GameObject.Find("LeftParticleSystem");
        RightParticlesObject = GameObject.Find("RightParticleSystem");
        LeftParticleSystem = LeftParticlesObject.GetComponent<ParticleSystem>();
        RightParticleSystem = RightParticlesObject.GetComponent<ParticleSystem>();

        BaseMesh = GameObject.Find("UserMesh").GetComponent<MeshFilter>();
        ParticleOutputQuad = GameObject.Find("ParticleOutputQuad");

        HitSystemBOrigin = HitSystemB.transform.position;
    }

    public void MidiHit(Pitch pitch)
    {
        switch (pitch.NoteNumber())
        {
            case 36:
                if (HitSystemA == null) return;
                HitSystemA.Stop();
                HitSystemA.Play();
                break;
            case 38:
                if (HitSystemB == null) return;
                HitSystemB.Stop();
                HitSystemB.Play();
                break;
            case 42:
                if (HitSystemC == null) return;
                HitSystemC.Stop();
                HitSystemC.Play();
                break;
        }
    }

    Vector3 DampPosition(Vector3 value, Vector3 goal, float dampRate)
    {
        if (Mathf.Abs(value.x - goal.x) > 0)
        {
            value.x = value.x + (goal.x - value.x) / dampRate;
        }
        if (Mathf.Abs(value.y - goal.y) > 0)
        {
            value.y = value.y + (goal.y - value.y) / dampRate;
        }
        if (Mathf.Abs(value.z - goal.z) > 0)
        {
            value.z = value.z + (goal.z - value.z) / dampRate;
        }
        return value;
    }

    int CyclePosition = -1;

    void JumpToCyclePosition()
    {
        CyclePosition++;
        if (CyclePosition > 3) CyclePosition = 0;

        int leftJoint = 0;
        int rightJoint = 0;

        switch (CyclePosition)
        {
            case 0:
                leftJoint = rightJoint = (int)KinectInterop.JointType.Head;
                break;
            case 1:
                leftJoint = (int)KinectInterop.JointType.ShoulderLeft;
                rightJoint = (int)KinectInterop.JointType.ShoulderRight;
                break;
            case 2:
                leftJoint = rightJoint = (int)KinectInterop.JointType.SpineMid;
                break;
            case 3:
                leftJoint = (int)KinectInterop.JointType.HipLeft;
                rightJoint = (int)KinectInterop.JointType.HipRight;
                break;

        }

        var leftPos = KinectManager.Instance.GetJointPosition(KinectManager.Instance.GetUserIdByIndex(0), leftJoint);
        NewLeftPosition = leftPos + LeftParticlesKinectOffset;
        var rightPos = KinectManager.Instance.GetJointPosition(KinectManager.Instance.GetUserIdByIndex(0), rightJoint);
        NewRightPosition = rightPos + RightParticlesKinectOffset;
    }

    void Update()
    {

        if (ControlHitParticlesWithKinectHands)
        {
            var manager = KinectManager.Instance;
            if (manager.GetUsersCount() != 0)
            {
                var userId = KinectManager.Instance.GetUserIdByIndex(0);

                // leftHandPosition uses the Kinect HandRight because we are talking "stage left"
                var leftHandPosition = UserForceController.GetJointPosition(manager, userId,
                    (int)KinectInterop.JointType.HandRight);
                var rightHandPosition = UserForceController.GetJointPosition(manager, userId,
                    (int)KinectInterop.JointType.HandLeft);

                var mappedLeftHandX = leftHandPosition.x.Map(
                    HitSystemBTrackXInput.x, HitSystemBTrackXInput.y,
                    HitSystemBTrackXOutput.x, HitSystemBTrackXOutput.y);

                HitSystemBXOffset.y = mappedLeftHandX;

                var mappedLeftHandY = leftHandPosition.y.Map(
                    HitSystemBTrackYInput.x, HitSystemBTrackYInput.y,
                    HitSystemBTrackYOutput.x, HitSystemBTrackYOutput.y);

                HitSystemBYOffset.y = mappedLeftHandY;
            }
            var updated = false;
            if (Mathf.Abs(HitSystemBXOffset.x - HitSystemBXOffset.y) > 0)
            {
                HitSystemBXOffset.x =
                    HitSystemBXOffset.x + (HitSystemBXOffset.y - HitSystemBXOffset.x) / HitSystemBTrackDamping;
                updated = true;
            }
            if (Mathf.Abs(HitSystemBYOffset.x - HitSystemBYOffset.y) > 0)
            {
                HitSystemBYOffset.x =
                    HitSystemBYOffset.x + (HitSystemBYOffset.y - HitSystemBYOffset.x) / HitSystemBTrackDamping;
                updated = true;
            }
            if (updated)
            {
                HitSystemB.transform.SetPositionAndRotation(new Vector3(
                    HitSystemBOrigin.x + HitSystemBXOffset.x, HitSystemBOrigin.y + HitSystemBYOffset.x, HitSystemBOrigin.z),
                    Quaternion.Euler(Vector3.zero));
            }
        }

        if (TrackAirsticks)
        {
            //Left
            NewLeftPosition = new Vector3(-AirSticks.Left.EulerAngles.y * LeftMultiplier.x, AirSticks.Left.Position.y * LeftMultiplier.y, 0);
            NewLeftPosition = NewLeftPosition + LeftParticlesPositionOffset;
            var particleSystem = LeftParticleSystem;
            //var emitter = particleSystem.emission;
            //emitter.rateOverTime = new MinMaxCurve(AirSticks.Left.JoystickY.Map(-1f, 1f, 2.65f, 10.6f));
            LeftParticleSystem = particleSystem;
            //Right
            NewRightPosition = new Vector3(-AirSticks.Right.EulerAngles.y * RightMultiplier.x, AirSticks.Right.Position.y * RightMultiplier.y, 0);
            NewRightPosition = NewRightPosition + RightParticlesPositionOffset;
            particleSystem = RightParticleSystem;
            //emitter = particleSystem.emission;
            //emitter.rateOverTime = new MinMaxCurve(AirSticks.Left.JoystickY.Map(-1f, 1f, 2.65f, 10.6f));
            RightParticleSystem = particleSystem;

            if (Input.GetKeyDown(KeyCode.Space))
            {
                JumpToCyclePosition();
            }

            LeftParticlesObject.transform.position = DampPosition(LeftPosition, NewLeftPosition, PositionDampRate);
            RightParticlesObject.transform.position = DampPosition(RightPosition, NewRightPosition, PositionDampRate);
        }

        if (TriggerSetsVisibility)
        {
            if (AirSticks.Left.Trigger && !Visible)
            {
                ParticleOutputQuad.GetComponent<MeshRenderer>().enabled = true;
                Visible = true;
            }
            else if (!AirSticks.Left.Trigger && Visible)
            {
                ParticleOutputQuad.GetComponent<MeshRenderer>().enabled = false;
                Visible = false;
            }
        }
        if (SetParticlesPositionsBasedOnMesh)
        {
            var particleCount = LeftParticleSystem.particleCount;
            var baseVertexCount = BaseMesh.mesh.vertexCount;
            ParticleSystem.Particle[] particles = new ParticleSystem.Particle[particleCount];
            LeftParticleSystem.GetParticles(particles);
            for (int i = 0; i < particleCount; i++)
            {
                var particle = particles[i];
                if (i < BaseMesh.mesh.vertexCount)
                {
                    particle.position = BaseMesh.mesh.vertices[i];
                }
                else
                {
                    particle.position = Vector3.zero;
                }
                particles[i] = particle;
            }
            LeftParticleSystem.SetParticles(particles, particleCount);
        }
    }
}
