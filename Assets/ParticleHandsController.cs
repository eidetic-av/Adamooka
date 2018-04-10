using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParticleHandsController : MonoBehaviour {

    GameObject ParticleHandsSystem;
    KinectManager KinectManager;

    Transform[] HandTransforms = new Transform[2];
	// Use this for initialization
	void Start () {
        ParticleHandsSystem = GameObject.Find("ParticleHandsSystem");
        KinectManager = GameObject.Find("KinectController").GetComponent<KinectManager>();
    }
	
	// Update is called once per frame
	void Update () {
        TrackJoint(0, KinectInterop.JointType.HandRight);
	}

    void TrackJoint(int user, KinectInterop.JointType jointType)
    {
        //long userId = KinectManager.GetUserIdByIndex(user);
        //int jointId = (int)jointType;

        //var transform = ParticleHandsSystem.transform;
        //transform.position = KinectManager.GetJointPosition(userId, jointId);

        //Debug.Log(transform);

        //TrackerCube.transform.position =
        //    KinectManager.GetJointPosColorOverlay(userId, jointId, Camera.main, Camera.main.pixelRect);
    }
}
