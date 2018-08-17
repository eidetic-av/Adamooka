using System.Collections;
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
    void Start()
    {
        Instance = this;
    }

    public void IntroPulse1()
    {
        IntroPulse1System.Stop();
        IntroPulse1System.Play();
    }

    public void IntroPulse2()
    {
        IntroPulse2System.Stop();
        IntroPulse2System.Play();
    }

    public void IntroPulse3()
    {
        IntroPulse3System.Stop();
        IntroPulse3System.Play();
    }

    public void Melody1()
    {
        Melody1System.Stop();
        Melody1System.Play();
    }

    public void Melody2()
    {
        Melody2System.Stop();
        Melody2System.Play();
    }

    public void Melody3()
    {
        Melody3System.Stop();
        Melody3System.Play();
    }

    public void Snap()
    {

    }

    public void Kick()
    {
        KickSystem.Stop();
        KickSystem.Play();
    }

    public void Snare()
    {
        SnareSystem.Stop();
        SnareSystem.Play();
    }

    public void RightHand()
    {
		RightHandSystem.Stop();
		RightHandSystem.Play();
    }

    public void LeftHand()
    {
		LeftHandSystem.Stop();
		LeftHandSystem.Play();
    }
}
