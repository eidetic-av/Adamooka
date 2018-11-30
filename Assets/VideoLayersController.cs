using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Deform.Deformers;
using Utility;

public class VideoLayersController : MonoBehaviour
{

    public static VideoLayersController Instance;

    public float MaxDeformationMagnitude = 3.5f;
    public float TransitionPower = 4f;
    public float TransitionLength = 1f;
    public NoiseDeformer VibesDeformer;
    public Material VibesMaterial;
    public bool StartVibesTransition = false;
    public bool StartVibesOutro = false;
    bool TransitioningVibes = false;
    bool VibesOutro = false;
    float VibesTransitionStartTime;
    float VibesTransition = 0f;

    public NoiseDeformer BassDeformer;
    public Material BassMaterial;
    public bool StartBassTransition = false;
    public bool StartBassOutro = false;
    bool TransitioningBass = false;
    bool BassOutro = false;
    float BassTransitionStartTime;
    float BassTransition = 0f;

    public bool FadeOutTunnel = false;
    bool FadingTunnel = false;
    public float TunnelFadeLength = 5f;
    float TunnelFadeOutStartTime;
    MeshRenderer TunnelPlane;

    void Start()
    {
        Instance = this;
        TunnelPlane = GameObject.Find("TunnelPlane").GetComponent<MeshRenderer>();
    }


    void Update()
    {

        if (StartVibesTransition)
        {
            VibesTransitionStartTime = Time.time;
            TransitioningVibes = true;
            StartVibesTransition = false;
        }
        if (TransitioningVibes)
        {
            VibesTransition = (Time.time - VibesTransitionStartTime) / TransitionLength;
            if (VibesTransition >= 1)
            {
                VibesTransition = 1;
                TransitioningVibes = false;
            }
        }

        if (StartVibesOutro)
        {
            VibesTransitionStartTime = Time.time;
            VibesOutro = true;
            StartVibesOutro = false;
        }
        if (VibesOutro)
        {
            VibesTransition = 1 - ((Time.time - VibesTransitionStartTime) / TransitionLength);
            if (VibesTransition <= 0)
            {
                VibesTransition = 0;
                VibesOutro = false;
            }
        }

        VibesDeformer.globalMagnitude = Mathf.Pow(VibesTransition, TransitionPower).Map(0, 1f, MaxDeformationMagnitude, 0f);
        VibesMaterial.color = new Color(1, 1, 1, Mathf.Pow(VibesTransition, TransitionPower));

        if (StartBassTransition)
        {
            BassTransitionStartTime = Time.time;
            TransitioningBass = true;
            StartBassTransition = false;
        }
        if (TransitioningBass)
        {
            BassTransition = (Time.time - BassTransitionStartTime) / TransitionLength;
            if (BassTransition >= 1)
            {
                BassTransition = 1;
                TransitioningBass = false;
            }
        }

        if (StartBassOutro)
        {
            BassTransitionStartTime = Time.time;
            BassOutro = true;
            StartBassOutro = false;
        }
        if (BassOutro)
        {
            BassTransition = 1 - ((Time.time - BassTransitionStartTime) / TransitionLength);
            if (BassTransition <= 0)
            {
                BassTransition = 0;
                BassOutro = false;
            }
        }

        BassDeformer.globalMagnitude = Mathf.Pow(BassTransition, TransitionPower).Map(0, 1f, MaxDeformationMagnitude, 0f);
        BassMaterial.color = new Color(1, 1, 1, Mathf.Pow(BassTransition, TransitionPower));

        if (FadeOutTunnel)
        {
            FadingTunnel = true;
            TunnelFadeOutStartTime = Time.time;
            FadeOutTunnel = false;
        }
        if (FadingTunnel)
        {
            var alpha = 1 - ((Time.time - TunnelFadeOutStartTime) / TunnelFadeLength);
            if (alpha <= 0)
            {
                alpha = 0;
                FadingTunnel = false;
            }
            TunnelPlane.material.SetColor("_Color", new Color(1, 1, 1, alpha));
        }
    }
}
