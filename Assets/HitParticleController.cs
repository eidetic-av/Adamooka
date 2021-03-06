﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitParticleController : MonoBehaviour
{

    public static HitParticleController Instance;
    public ParticleSystem IntroPulse1System;
    public ParticleSystem IntroPulse2System;
    public ParticleSystem IntroPulse3System;
    public ParticleSystem Melody1System;
    public ParticleSystem Melody2System;
    public ParticleSystem Melody3System;
    public ParticleSystem KickSystem;
    public ParticleSystem SnareSystem;
    public ParticleSystem RightHandSystem;
    public ParticleSystem LeftHandSystem;

    public bool TrackKinectHands = true;
    public UserForceController UserForceController;
    public Vector2 LeftHandScaling = Vector2.one;
    public Vector2 LeftHandOffset = Vector2.zero;
    public Vector2 RightHandScaling = Vector2.one;
    public Vector2 RightHandOffset = Vector2.zero;

    Vector3 LeftHandStart;
    Vector3 RightHandStart;
    void Start()
    {
        Instance = this;
        LeftHandStart = LeftHandSystem.gameObject.transform.position;
        RightHandStart = RightHandSystem.gameObject.transform.position;
    }

    void Update() 
    {
        if (TrackKinectHands && KinectManager.Instance != null) {
            var manager = KinectManager.Instance;
            if (manager.GetUsersCount() != 0)
            {
                var userId = KinectManager.Instance.GetUserIdByIndex(0);

                var leftHandPosition = UserForceController.GetJointPosition(manager, userId,
                    (int)KinectInterop.JointType.HandLeft);
                var rightHandPosition = UserForceController.GetJointPosition(manager, userId,
                    (int)KinectInterop.JointType.HandRight);

                var mappedLeftHandX = (leftHandPosition.x * LeftHandScaling.x) + LeftHandOffset.x;
                var mappedLeftHandY = (leftHandPosition.y * LeftHandScaling.y) + LeftHandOffset.y;

                LeftHandSystem.transform.position = 
                    new Vector3(LeftHandStart.x + mappedLeftHandX,
                    LeftHandStart.y + mappedLeftHandY, LeftHandStart.z);
                
                var mappedRightHandX = (rightHandPosition.x * RightHandScaling.x) + RightHandOffset.x;
                var mappedRightHandY = (rightHandPosition.y * RightHandScaling.y) + RightHandOffset.y;

                RightHandSystem.transform.position = 
                    new Vector3(RightHandStart.x + mappedRightHandX,
                    RightHandStart.y + mappedRightHandY, RightHandStart.z);

            }
        }
    }

    public void IntroPulse1()
    {
        if (IntroPulse1System == null) return;
        IntroPulse1System.Clear();
        IntroPulse1System.Play();
    }

    public void IntroPulse2()
    {
        if (IntroPulse2System == null) return;
        IntroPulse2System.Clear();
        IntroPulse2System.Play();
    }

    public void IntroPulse3()
    {
        if (IntroPulse3System == null) return;
        IntroPulse3System.Clear();
        IntroPulse3System.Play();
    }

    public void Melody1()
    {
        if (Melody1System == null) return;
        Melody1System.Clear();
        Melody1System.Play();
    }

    public void Melody2()
    {
        if (Melody2System == null) return;
        Melody2System.Clear();
        Melody2System.Play();
    }

    public void Melody3()
    {
        if (Melody3System == null) return;
        Melody3System.Clear();
        Melody3System.Play();
    }

    public void Snap()
    {

    }

    public void Kick()
    {
        if (KickSystem == null) return;
        KickSystem.Clear();
        KickSystem.Play();
    }

    public void Snare()
    {
        if (SnareSystem == null) return;
        SnareSystem.Clear();
        SnareSystem.Play();
    }

    public void RightHand()
    {
        if (RightHandSystem == null) return;
		RightHandSystem.Clear();
		RightHandSystem.Play();
    }

    public void LeftHand()
    {
        if (LeftHandSystem == null) return;
		LeftHandSystem.Clear();
		LeftHandSystem.Play();
    }
}
