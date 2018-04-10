using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TrackerTest : MonoBehaviour
{
    public float TrailMovementDamp = 4;

    GameObject TrackerCube;
    GameObject ParticleSystemLeft, ParticleSystemRight;
    KinectManager KinectManager;

    Vector3 NewLeftPosition = Vector3.zero;
    // Use this for initialization
    void Start()
    {
        TrackerCube = GameObject.Find("TrackerCube");
        KinectManager = GameObject.Find("KinectController").GetComponent<KinectManager>();
        ParticleSystemLeft = GameObject.Find("ParticleHandsSystemLeft");
        ParticleSystemRight = GameObject.Find("ParticleHandsSystemRight");
    }

    // Update is called once per frame
    void Update()
    {
        long userId = KinectManager.GetUserIdByIndex(0);
        int jointType = (int)KinectInterop.JointType.HandLeft;
        Vector3 jointPosition = KinectManager.GetJointPosition(userId, jointType);
        Vector3 jointOverlayPosition = KinectManager.GetJointPosColorOverlay(userId, jointType, Camera.main, Camera.main.pixelRect);

        if (Mathf.Abs(NewLeftPosition.x - jointPosition.x) > 0)
        {
            jointPosition.x = jointPosition.x + (NewLeftPosition.x - jointPosition.x) / TrailMovementDamp;
            jointOverlayPosition.x = jointOverlayPosition.x + (NewLeftPosition.x - jointOverlayPosition.x) / TrailMovementDamp;
        }
        if (Mathf.Abs(NewLeftPosition.y - jointPosition.y) > 0)
        {
            jointPosition.y = jointPosition.y + (NewLeftPosition.y - jointPosition.y) / TrailMovementDamp;
            jointOverlayPosition.y = jointOverlayPosition.y + (NewLeftPosition.y - jointOverlayPosition.y) / TrailMovementDamp;
        }
        if (Mathf.Abs(NewLeftPosition.z - jointPosition.z) > 0)
        {
            jointPosition.z = jointPosition.z + (NewLeftPosition.z - jointPosition.z) / TrailMovementDamp;
            jointOverlayPosition.z = jointOverlayPosition.z + (NewLeftPosition.z - jointOverlayPosition.z) / TrailMovementDamp;
        }

        ParticleSystemLeft.transform.position = jointPosition;

        ParticleSystemLeft.transform.position = jointOverlayPosition;

        jointType = (int)KinectInterop.JointType.HandRight;

        ParticleSystemRight.transform.position =
            KinectManager.GetJointPosition(userId, jointType);

        ParticleSystemRight.transform.position =
            KinectManager.GetJointPosColorOverlay(userId, jointType, Camera.main, Camera.main.pixelRect);

    }
}
