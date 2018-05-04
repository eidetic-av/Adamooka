using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Midi;
using System;
using Eidetic;
using Eidetic.Unity.Utility;

public class MidiManager : MonoBehaviour
{

    public string DeviceName = "rtpMIDI";
    InputDevice InputDevice;

    public static int AbletonState = -1;

    public static class OneFiveNine
    {
        public static Action Beep;
        public static Action<Pitch> Bass;
        public static Action<Pitch> Melody;
    }

    public int CloneStartCount = 10;

    public GameObject ParticleScene;
    public GameObject RainParticles;
    public GameObject UserParticles;
    public GameObject ParticleCamera;
    public GameObject UserMesh;
    public MeshTools MeshTools;
    public Renderer UserMeshRenderer;
    public UserMeshVisualizer UserMeshVisualizer;
    public GameObject TrackerSceneOutputQuad;
    public TrackerOutputEffector TrackerOutputEffector;

    public GameObject PatternScene;
    public Strobe Strobe;

    UnityEngine.Material ToonLit;
    UnityEngine.Material BlackOcclusion;
    UnityEngine.Material Wireframe;

    // Use this for initialization
    void Start()
    {
        ToonLit = Resources.Load<UnityEngine.Material>("ToonLit");
        BlackOcclusion = Resources.Load<UnityEngine.Material>("BlackOcclusion");
        Wireframe = Resources.Load<UnityEngine.Material>("Rainbow Wireframe");
        foreach (InputDevice inputDevice in InputDevice.InstalledDevices)
        {
            if (inputDevice.Name.ToLower().Equals(DeviceName.ToLower()))
            {
                InputDevice = inputDevice;
                break;
            }
        }
        if (InputDevice != null)
        {
            InputDevice.Open();
            InputDevice.StartReceiving(null);
            InputDevice.NoteOn += RouteNoteOn;
        }
    }

    private void RouteNoteOn(NoteOnMessage noteOnMessage)
    {
        switch (noteOnMessage.Channel)
        {
            case Channel.Channel1:
                {
                    OneFiveNine.Beep.Invoke();
                    break;
                }
            case Channel.Channel2:
                {
                    OneFiveNine.Bass.Invoke(noteOnMessage.Pitch);
                    break;
                }
            case Channel.Channel14:
                {
                    RouteElectricMidi(noteOnMessage.Pitch);
                    break;
                }
            case Channel.Channel16:
                {
                    UpdateAbletonState(noteOnMessage.Pitch);
                    break;
                }
        }
    }

    private void RouteElectricMidi(Pitch pitch)
    {
        Debug.Log(pitch);
        switch(pitch)
        {
            case Pitch.CNeg1:
                Strobe.Bang = true;
                break;
        }
    }

    private void UpdateAbletonState(Pitch pitch)
    {
        Debug.Log(pitch);
        Threading.RunOnMain(() =>
        {
            switch (pitch)
            {
                case Pitch.E2:
                    // Rain
                    ActivateRainState();
                    break;
                case Pitch.E1:
                    // SuckRain
                    SuckRain();
                    break;
                case Pitch.D2:
                    // Clone transition
                    ExplodeMeshBeforeClones();
                    break;
                case Pitch.DSharp2:
                    if (AbletonState != 3)
                        // Activate clone scene
                        ShowClones();
                    else
                        // Cycle colours
                        TrackerOutputEffector.CycleCloneColours();
                    break;
                case Pitch.F1:
                    // End clones
                    EndClones();
                    break;
                case Pitch.F2:
                    if (AbletonState != 5)
                        // stop producing rain particles
                        StopRain();
                    else if (AbletonState == 5)
                        // activate wireframe scene
                        ShowWireframe();
                    else if (AbletonState == 6)
                        WireframeExplodeEnable();
                    else if (AbletonState == 7)
                        WireframeExplodeDisable();
                    else if (AbletonState == 8)
                        WireframeExplodeReEnable();
                    else if (AbletonState == 9)
                        DesmondBreak();
                    break;
            }
        });
    }

    void ActivateRainState()
    {
        ClearAllStates();
        RainParticles.SetActive(true);
        RainParticles.GetComponent<RainController>().Control = true;
        ParticleCamera.SetActive(true);
        UserParticles.SetActive(false);

        AbletonState = 0;
    }

    void SuckRain()
    {
        if (AbletonState == 0)
        {
            var rainController = RainParticles.GetComponent<RainController>();
            rainController.Hone = true;
        }

        AbletonState = 1;
    }

    void ExplodeMeshBeforeClones()
    {
        if (AbletonState == 1)
        {
            MeshTools.EnableAirsticksControl = false;
            // enable mesh renderer
            UserMeshRenderer.enabled = true;
            UserMeshRenderer.material = new UnityEngine.Material(ToonLit);
            // set it to red
            TrackerSceneOutputQuad.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1, 0, 0, 1));

            // and explode it
            MeshTools.EnableExplode = true;
            MeshTools.ExplodeA();

            // disable rain
            RainParticles.GetComponent<RainController>().Control = false;
            RainParticles.SetActive(false);
            ParticleCamera.SetActive(false);
        }
        AbletonState = 2;
    }

    void ShowClones()
    {
        // enable mesh renderer
        UserMeshRenderer.enabled = true;
        // and set the base output layer as occlusion with black tint
        TrackerSceneOutputQuad.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(0, 0, 0, 1));

        // add some clones to the scene
        for (int i = 0; i < 3; i++)
        {
            TrackerOutputEffector.InstantiateClone(AirSticks.Hand.Left);
            TrackerOutputEffector.InstantiateClone(AirSticks.Hand.Right);
        }

        MeshTools.EnableExplode = false;

        AbletonState = 3;
    }

    void EndClones()
    {
        TrackerOutputEffector.HideClones(AirSticks.Hand.Left);
        TrackerOutputEffector.HideClones(AirSticks.Hand.Right);
        UserMeshVisualizer.BlockKinectUpdate = true;
        TrackerSceneOutputQuad.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1, 0, 0, 1));

        RainParticles.SetActive(true);
        var RainController = RainParticles.GetComponent<RainController>();
        RainController.Control = true;
        RainController.Revert = true;
        RainController.RevertDamping = 10f;

        ParticleCamera.SetActive(true);

        AbletonState = 4;
    }

    void StopRain()
    {
        var emissionModule = RainParticles.GetComponentInChildren<ParticleSystem>().emission;
        emissionModule.rateOverTime = 0;
        AbletonState = 5;
    }

    void ShowWireframe()
    {
        UserMeshVisualizer.BlockKinectUpdate = false;
        TrackerSceneOutputQuad.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1, 1, 1, 1));
        UserMeshRenderer.material = Wireframe;
        MeshTools.EnableExplode = false;
        MeshTools.EnableAirsticksControl = true;
        AbletonState = 6;
    }

    void WireframeExplodeEnable()
    {
        MeshTools.EnableExplode = true;
        AirSticks.Right.NoteOn += MeshTools.ExplodeA;
        AirSticks.Left.NoteOn += MeshTools.ExplodeB;
        MeshTools.EnableAirsticksControl = false;
        AbletonState = 7;
    }

    void WireframeExplodeDisable()
    {
        MeshTools.EnableExplode = false;
        MeshTools.EnableAirsticksControl = true;
        AbletonState = 8;
    }

    void WireframeExplodeReEnable()
    {
        MeshTools.EnableExplode = true;
        MeshTools.EnableAirsticksControl = false;
        AbletonState = 9;
    }

    void DesmondBreak()
    {
        MeshTools.EnableExplode = false;
        UserMeshVisualizer.BlockKinectUpdate = true;
        AbletonState = 10;
    }

    void ClearAllStates()
    {
        RainParticles.SetActive(false);
        RainParticles.GetComponent<RainController>().Control = false;
        ParticleCamera.SetActive(false);
        UserParticles.SetActive(false);
    }

    private void OnDestroy()
    {
        if (InputDevice != null)
        {
            InputDevice.StopReceiving();
            InputDevice.Close();
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
