using Eidetic.Unity.Utility;
using Midi;
using System;
using System.Collections.Generic;
using UnityEngine;

public class MidiManager : MonoBehaviour
{

    public static MidiManager Instance;

    public string DeviceName = "rtpMIDI";
    InputDevice InputDevice;

    public bool LogNoteOns = false;

    public int AbletonState = -1;

    public static class OneFiveNine
    {
        public static Action Beep;
        public static Action<Pitch> Bass;
        public static Action<Pitch> Melody;
    }

    public int CloneStartCount = 10;

    public GameObject ParticleScene;
    public GameObject RainParticles;
    public WindZone RainParticlesWind;
    public GameObject UserParticles;
    public GameObject ParticleCamera;
    public ParticleController HitParticlesController;
    public GameObject UserMesh;
    public MeshTools MeshTools;
    public Renderer UserMeshRenderer;
    public UserMeshVisualizer UserMeshVisualizer;
    public GameObject TrackerSceneOutputQuad;
    public Renderer TrackerSceneOutputQuadRenderer;
    public GameObject TrackerSceneFlippedOutputQuad;
    public Renderer TrackerSceneFlippedOutputQuadRenderer;
    public TrackerOutputEffector TrackerOutputEffector;

    public GameObject PatternScene;
    public GameObject Lines;
    public Strobe Strobe;
    public Kino.Mirror Kaleidoscope;

    public GameObject DriftParticles;
    public DriftController DriftController;

    UnityEngine.Material ToonLit;
    UnityEngine.Material BlackOcclusion;
    UnityEngine.Material Wireframe;
    UnityEngine.Material Basic;
    UnityEngine.Material Pink;
    UnityEngine.Material LinePattern;
    UnityEngine.Material GranOutline;

    bool ExitLines = false;
    bool ExitTrackerQuad = false;
    float ExitTime = 0f;

    public Dictionary<String, ParticleSystem> RingKitSystems;

    // Use this for initialization
    void Start()
    {
        Instance = this;

        ToonLit = Resources.Load<UnityEngine.Material>("ToonLit");
        BlackOcclusion = Resources.Load<UnityEngine.Material>("BlackOcclusion");
        Wireframe = Resources.Load<UnityEngine.Material>("Rainbow Wireframe");
        Basic = Resources.Load<UnityEngine.Material>("Basic");
        Pink = Resources.Load<UnityEngine.Material>("Pink");
        LinePattern = Resources.Load<UnityEngine.Material>("LinePattern");
        GranOutline = Resources.Load<UnityEngine.Material>("GranOutline");


        TrackerSceneOutputQuadRenderer = TrackerSceneOutputQuad.GetComponent<Renderer>();
        TrackerSceneFlippedOutputQuadRenderer = TrackerSceneFlippedOutputQuad.GetComponent<Renderer>();

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
            Debug.Log("Opened MIDI Device");
        }
    }

    private void RouteNoteOn(NoteOnMessage noteOnMessage)
    {
        if (LogNoteOns) Debug.Log(noteOnMessage.Channel + "." + noteOnMessage.Pitch);

        Threading.RunOnMain((Action)(() =>
        {
            switch (noteOnMessage.Channel)
            {
                case Channel.Channel1:
                    {
                        RouteOneFiveNine(noteOnMessage.Pitch);
                        break;
                    }
                case Channel.Channel2:
                    {
                        RouteComputerRain(noteOnMessage.Pitch);
                        break;
                    }
                case Channel.Channel3:
                    {
                        RouteDesmondMidi(noteOnMessage.Pitch);
                        break;
                    }
                case Channel.Channel14:
                    {
                        if (noteOnMessage.Pitch == Pitch.C0)
                        {
                            CircleParticleController.Instance.BangRotation = true;
                        }
                        break;
                    }
                case Channel.Channel4:
                    {
                        RouteHyphenMidi(noteOnMessage.Pitch);
                        break;
                    }
                case Channel.Channel15:
                    {
                        RouteDriftMidi(noteOnMessage.Pitch);
                        break;
                    }
                case Channel.Channel16:
                    {
                        UpdateAbletonState(noteOnMessage.Pitch);
                        break;
                    }
            }
        }));
    }
    private void RouteOneFiveNine(Pitch pitch)
    {
        var noise = NoiseCircleController.Instance;

        switch (pitch)
        {
            case Pitch.CSharp2:
                // Beep Rec
                MembraneController.Instance.FlashColor = true;
                break;
            case Pitch.C2:
                MembraneController.Instance.FlashColor = true;
                // Beep Loop
                break;

            case Pitch.DSharp2:
                // Growl Rec
                noise.Triggers[0] = true;
                break;
            case Pitch.D2:
                // Growl Loop
                noise.Triggers[0] = true;
                break;
            case Pitch.F2:
                // Kick Rec
                noise.Triggers[1] = true;
                break;
            case Pitch.E2:
                noise.Triggers[1] = true;
                // Kick Loop
                break;
            case Pitch.G2:
                noise.Triggers[2] = true;
                // Snare1 Rec
                break;
            case Pitch.FSharp2:
                noise.Triggers[2] = true;
                // Snare1 Loop
                break;
            case Pitch.A2:
                // Open HH Rec
                noise.Triggers[3] = true;
                break;
            case Pitch.GSharp2:
                // Open HH Loop
                noise.Triggers[3] = true;
                break;
            case Pitch.ASharp2:
                // Kick2 Rec
                noise.Triggers[4] = true;
                break;
            case Pitch.B2:
                // Kick2 Loop
                noise.Triggers[4] = true;
                break;
            case Pitch.CSharp3:
                noise.Triggers[5] = true;
                // Snare2 Rec
                break;
            case Pitch.C3:
                noise.Triggers[5] = true;
                // Snare2 Loop
                break;
            case Pitch.DSharp3:
                // Snare3 Rec
                noise.Triggers[6] = true;
                break;
            case Pitch.D3:
                // Snare3 Loop
                noise.Triggers[6] = true;
                break;
            case Pitch.B1:
                // Sub kick
                break;

            case Pitch.F3:
                // Invasion start
                CircleParticleController.Instance.StartInvasion = true;
                break;
            case Pitch.FSharp3:
                // Invasion explode / beat in
                CircleParticleController.Instance.StartInvasionRevert = true;
                // Noisier high hat? Different texture, colour?
                break;

            case Pitch.G3:
                // Go back to Airsticks Note On/Off with the
                OneFiveNineCircleController.Instance.ActivateAirSticksKickSnare = true;
                break;
        }
    }

    private void RouteComputerRain(Pitch pitch) {
        switch(pitch) {
            case Pitch.B2:
                // start rain
                RainController.Instance.EnableEmission = true;
                GameObject.Find("OneFiveNine").SetActive(false);
                TrackerSceneController.Instance.EnableKinectUpdate = true;
                break;
            case Pitch.C3:
                // Suck in particles
                RainController.Instance.StartCloneTransition = true;
                break;
            case Pitch.CSharp3:
                // Disperse particles
                RainController.Instance.StartCloneDispersion = true;
                RainController.Instance.StopParticleLength = 3;
                RainController.Instance.SlowlyStopParticles = true;
                // Switch on white clones
                TrackerSceneController.Instance.ActivateClones = true;
                TrackerSceneController.Instance.ActivateCloneIntroState = true;
                break;
            case Pitch.ASharp2:
                // Activate airsticks clone colour cycle
                TrackerSceneController.Instance.AirSticksNoteOnCyclesColours = true;
                break;
            case Pitch.D3:
                // Go to desmond colours
                TrackerSceneController.Instance.ActivateRainOutroState = true;
                break;
            case Pitch.DSharp3:
                // TrackerSceneController.Instance.SillhouetteOff = true;
                break;
            case Pitch.E3:
                // Step two, introduce particles with desmond colours
                // Explode particles, remove clones
                RainController.Instance.ActivateOutroState = true;
                break;
            case Pitch.F3:
                RainController.Instance.TransitioningToOutro = true;
                TrackerSceneController.Instance.RemoveAllClones = true;
                break;
            case Pitch.FSharp3:
                RainController.Instance.StopParticleLength = 10;
                RainController.Instance.SlowlyStopParticles = true;
                break;
        }
    }


    private void RouteDesmondMidi(Pitch pitch)
    {
        switch (pitch)
        {
            // Desmond control
            case Pitch.F2:
                // start desmond
                var renderer = GameObject.Find("OutputQuad").GetComponent<Renderer>();
                renderer.material.SetColor("_TintColor", new Color(1, 1, 1, 1));
                var userMesh = GameObject.Find("UserMesh").GetComponent<Renderer>();
                userMesh.enabled = true;
                userMesh.material = Resources.Load<UnityEngine.Material>("Rainbow Wireframe");
                MeshTools.Instance.EnableDesmondAirsticksControl = true;
                MeshTools.Instance.DesmondInstensityMinMax = new Vector2(-0.3f, 0.5f);
                MeshTools.Instance.DesmondSmoothingMinMax = new Vector2Int(0, 5);
                break;
            case Pitch.E2:
                // minimal shape
                MeshTools.Instance.DesmondInstensityMinMax = new Vector2(0f, 0.25f);
                MeshTools.Instance.DesmondSmoothingMinMax = new Vector2Int(1, 5);
                break;
            case Pitch.FSharp2:
                // intense desmond shape
                MeshTools.Instance.DesmondInstensityMinMax = new Vector2(-0.3f, 0.5f);
                MeshTools.Instance.DesmondSmoothingMinMax = new Vector2Int(0, 5);
                break;
            case Pitch.GSharp3:
                // desmond lose shape
                UserMeshVisualizer.Instance.BlockKinectUpdate = true;
                break;
            case Pitch.A3:
                // end breakdown / back to form
                UserMeshVisualizer.Instance.BlockKinectUpdate = false;
                break;
            case Pitch.ASharp3:
                // drum/guitar outro setting
                break;
            case Pitch.B3:
                // guitar solo shape
                break;
            case Pitch.C4:
                UserMeshVisualizer.Instance.BlockKinectUpdate = true;
                // ending transition
                break;

            // Track control
            case Pitch.D2:
                // pause track
                break;
            case Pitch.DSharp2:
                // unpause track
                break;
            // case Pitch.E2:
                // glitch out
                // break;
            
            // Paint control
            case Pitch.G2:
                // piano 1 on
                // only left hand
                break;
            case Pitch.GSharp2:
                // piano 1 off
                break;
            case Pitch.A2:
                // bass on
                break;
            case Pitch.ASharp2:
                // bass off
                break;
            case Pitch.B2:
                // vibes on
                // only right hand
                // 1 Minute long trail
                break;
            case Pitch.C3:
                // vibes off
                break;
            case Pitch.CSharp3:
                // drums on
                break;
            case Pitch.D3:
                // drums off
                break;
            case Pitch.DSharp3:
                // vibes off quickly
            case Pitch.E3:
                // piano 2 on
                // 37 second long trail
                break;
            case Pitch.F3:
                // piano 2 off
                break;
            case Pitch.FSharp3:
                // drums 2 on
                // 1:15 trail length
                break;
            case Pitch.G3:
                // drums 2 off
                break;
        }
    }
    private void RouteDriftMidi(Pitch pitch)
    {
        Debug.Log(pitch.ToString());
        switch (pitch)
        {
            case Pitch.CNeg1:
                DriftController.SelectState(0);
                break;
            case Pitch.CSharpNeg1:
                DriftController.SelectState(1);
                break;
            case Pitch.DNeg1:
                DriftController.SelectState(2);
                break;
            case Pitch.DSharpNeg1:
                DriftController.SelectState(3);
                break;
            case Pitch.ENeg1:
                DriftController.SelectState(4);
                break;
            case Pitch.FNeg1:
                DriftController.SelectState(5);
                break;
            case Pitch.FSharpNeg1:
                DriftController.SelectState(6);
                break;
            case Pitch.GNeg1:
                DriftController.SelectState(7);
                break;
            case Pitch.GSharpNeg1:
                DriftController.SelectState(8);
                break;
            case Pitch.ANeg1:
                DriftController.SelectState(9);
                break;
        }
    }

    private void RouteElectricMidi(Pitch pitch)
    {
        switch (pitch)
        {
            case Pitch.CNeg1:
                Strobe.Flash();
                break;
            case Pitch.GNeg1:
                RandomiseKaleidoscope();
                UserMeshVisualizer.EndAnimateUserRotation();
                break;
            case Pitch.GSharpNeg1:
                IncrementalKaleidoscopeA();
                UserMeshVisualizer.StartAnimateUserRotation();
                break;
            case Pitch.ANeg1:
                IncrementalKaleidoscopeB();
                break;
            case Pitch.ASharpNeg1:
                IncrementalKaleidoscopeC();
                break;
            case Pitch.BNeg1:
                IncrementalKaleidoscopeD();
                break;
            case Pitch.C0:
                HighKaleidoscopeSqueal();
                break;
            case Pitch.CSharp0:
                LowKaleidoscopeSqueal();
                break;
            case Pitch.C1:
                LinesOut();
                break;
        }
    }

    private void RouteHyphenMidi(Pitch pitch)
    {
        //Debug.Log(pitch.NoteNumber());
        HitParticlesController.MidiHit(pitch);
    }

    private void RandomiseKaleidoscope()
    {
        Kaleidoscope.enabled = true;
        int lastRepeat = Kaleidoscope.Repeat;
        int repeat = 1;
        while ((repeat % 2 != 0) || (repeat == lastRepeat))
        {
            repeat = Mathf.FloorToInt(UnityEngine.Random.Range(2, 7));
        }
        Kaleidoscope.Repeat = repeat;
        float roll = UnityEngine.Random.Range(0, 360);
        Kaleidoscope.Roll = repeat;
        Kaleidoscope.Offset = 0;
        Kaleidoscope.AnimateRoll = true;
        Kaleidoscope.AnimateRollRate = 40;
        Kaleidoscope.Symmetry = true;
    }

    private void IncrementalKaleidoscopeA()
    {
        Kaleidoscope.Repeat = 5;
        Kaleidoscope.Offset = 0;
        Kaleidoscope.AnimateRoll = true;
        Kaleidoscope.AnimateRollRate = 80;
        Kaleidoscope.Symmetry = false;
    }

    private void IncrementalKaleidoscopeB()
    {
        Kaleidoscope.Repeat = 3;
        Kaleidoscope.Offset = 0;
        Kaleidoscope.AnimateRoll = true;
        Kaleidoscope.AnimateRollRate = -20;
        Kaleidoscope.Symmetry = false;
    }

    private void IncrementalKaleidoscopeC()
    {
        Kaleidoscope.Repeat = 6;
        Kaleidoscope.Offset = 0;
        Kaleidoscope.AnimateRoll = true;
        Kaleidoscope.AnimateRollRate = 150;
        Kaleidoscope.Symmetry = false;
    }

    private void IncrementalKaleidoscopeD()
    {
        Kaleidoscope.Repeat = 9;
        Kaleidoscope.Offset = 0;
        Kaleidoscope.AnimateRoll = true;
        Kaleidoscope.AnimateRollRate = 270;
        Kaleidoscope.Symmetry = false;
    }

    private void HighKaleidoscopeSqueal()
    {
        Kaleidoscope.enabled = true;
        Kaleidoscope.AnimateRoll = false;
        Kaleidoscope.Symmetry = false;
        Kaleidoscope.Repeat = 2;
        Kaleidoscope.Roll = -45f;
    }

    private void LowKaleidoscopeSqueal()
    {
        Kaleidoscope.enabled = true;
        Kaleidoscope.AnimateRoll = false;
        Kaleidoscope.Symmetry = false;
        Kaleidoscope.Repeat = 2;
        Kaleidoscope.Roll = 45f;
    }

    private void LinesOut()
    {
        ExitLines = true;
        ExitTrackerQuad = true;
        ExitTime = Time.time;
        UserParticles.SetActive(false);
    }
    private void DirectKeyCodeScenes()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
            UpdateAbletonState(Pitch.E2);
        if (Input.GetKeyDown(KeyCode.Alpha2))
            UpdateAbletonState(Pitch.E1);
        if (Input.GetKeyDown(KeyCode.Alpha3))
            UpdateAbletonState(Pitch.D2);
        if (Input.GetKeyDown(KeyCode.Alpha4))
            UpdateAbletonState(Pitch.DSharp2);
        if (Input.GetKeyDown(KeyCode.Alpha5))
            UpdateAbletonState(Pitch.F1);
        if (Input.GetKeyDown(KeyCode.Alpha6))
            UpdateAbletonState(Pitch.F2);
        if (Input.GetKeyDown(KeyCode.Alpha7))
            UpdateAbletonState(Pitch.FSharp2);
        if (Input.GetKeyDown(KeyCode.Alpha8))
            UpdateAbletonState(Pitch.G2);
        if (Input.GetKeyDown(KeyCode.Alpha9))
            UpdateAbletonState(Pitch.GSharp2);
        if (Input.GetKeyDown(KeyCode.Alpha0))
            UpdateAbletonState(Pitch.A2);
        if (Input.GetKeyDown(KeyCode.Minus))
            UpdateAbletonState(Pitch.B2);

        if (Input.GetKeyDown(KeyCode.H))
        {
            UpdateAbletonState(Pitch.ASharp2);
        }

        if (Input.GetKeyDown(KeyCode.U))
        {
            GameObject.Find("ParticleScene").SetActive(false);
            GameObject.Find("TrackerScene").SetActive(false);
        }
    }

    private void UpdateAbletonState(Pitch pitch)
    {
        switch (pitch)
        {
            case Pitch.E2:
                // Rain
                ActivateRainState();
                ParticleSceneController.Instance.OneFiveNineOut = true;
                break;
            case Pitch.E1:
                // SuckRain
                SuckRain();
                break;
            case Pitch.D2:
                // Clone transition
                PreClones();
                break;
            case Pitch.DSharp2:
                if (AbletonState != 3)
                    // Activate clone scene
                    TrackerSceneController.Instance.ActivateClones = true;
                else
                    // Cycle colours
                    TrackerOutputEffector.CycleCloneColours();
                break;
            case Pitch.F1:
                // End clones
                EndClones();
                break;
            case Pitch.F2:
                if (AbletonState != 5 && AbletonState < 6)
                    PreDesmond();
                else if (AbletonState == 5)
                    // activate wireframe scene
                    ShowDesmond();
                else if (AbletonState == 6)
                    WireframeExplodeEnable();
                else if (AbletonState == 7)
                    WireframeExplodeDisable();
                else if (AbletonState == 8)
                    WireframeExplodeReEnable();
                else if (AbletonState == 9)
                    DesmondBreak();
                break;
            case Pitch.FSharp2:
                DesmondOut();
                break;
            case Pitch.CSharp3:
                ActivateHeifen();
                break;
            case Pitch.G2:
                TransitionOutOfHeifen();
                UmbeAnts();
                break;
            case Pitch.D3:
                RemoveRainParticles();
                break;
            case Pitch.GSharp2:
                EndUmbeAnts();
                break;
            case Pitch.ASharp2:
                ActivateGranRelatedBody();
                break;
            case Pitch.C3:
                ActivateGranRelatedBody();
                ActivateGranRelatedHands();
                break;
            case Pitch.A2:
                ElectricPartOne();
                break;
            case Pitch.B2:
                ElectricPartTwo();
                break;
        }
    }

    void UmbeAnts()
    {
        RainParticlesWind.windMain = 0;
        DriftParticles.SetActive(true);
    }

    void EndUmbeAnts()
    {
        DriftParticles.SetActive(false);
    }

    public void ActivateRainState()
    {
        ClearAllStates();
        RainParticles.SetActive(true);
        RainParticles.GetComponent<RainController>().Control = true;
        ParticleCamera.SetActive(true);
        UserParticles.SetActive(false);

        AbletonState = 0;
    }

    public void SuckRain()
    {
        if (AbletonState == 0)
        {
            var rainController = RainParticles.GetComponent<RainController>();
            rainController.HoneDampingSpeed = new Vector2(5, 5);
            rainController.HoneDampingSpeedDamp = 5f;
            rainController.Hone = true;
        }

        AbletonState = 1;
    }

    public void PreClones()
    {
        if (AbletonState == 1)
        {
            MeshTools.EnableDesmondAirsticksControl = false;
            // enable mesh renderer
            UserMeshRenderer.enabled = true;
            //UserMeshRenderer.material = new UnityEngine.Material(ToonLit);
            //// set it to red
            //TrackerSceneOutputQuad.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1, 0, 0, 1));

            //// and explode it
            //MeshTools.EnableExplode = true;
            //MeshTools.ExplodeA();

            // disable rain
            RainParticles.GetComponent<RainController>().Control = false;
            RainParticles.SetActive(false);
            ParticleCamera.SetActive(false);
        }
        AbletonState = 2;
    }

    public void ShowClones()
    {
        // enable mesh renderer
        UserMeshRenderer.enabled = true;
        TrackerOutputEffector.CloningActive = false;

        // Set material
        UserMeshVisualizer.Instance.gameObject.GetComponent<Renderer>().material = Resources.Load<UnityEngine.Material>("CloneMaterial");

        //MeshTools.EnableExplode = false;

        // add some clones to the scene
        for (int i = 0; i < 4; i++)
        {
            TrackerOutputEffector.InstantiateClone(AirSticks.Hand.Left);
            TrackerOutputEffector.InstantiateClone(AirSticks.Hand.Right);
        }

        // and set the base output layer as occlusion by tinting it black
        TrackerSceneOutputQuad.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(0, 0, 0, 1));
        TrackerSceneController.Instance.ActivateClones = true;

        // set the mesh deformation levels
        var meshTools = GameObject.Find("UserMesh").GetComponent<MeshTools>();
        meshTools.Noise.NewNoiseIntensity = 0;
        meshTools.Noise.NoiseIntensity = 0;
        meshTools.Noise.SmoothingTimes = 1;

        // activate the UserLight
        GameObject.Find("UserLight").GetComponent<Light>().enabled = true;

        AbletonState = 3;
    }

    void EndClones()
    {
        TrackerOutputEffector.HideClones(AirSticks.Hand.Left);
        TrackerOutputEffector.HideClones(AirSticks.Hand.Right);
        TrackerOutputEffector.CloningActive = false;
        //UserMeshVisualizer.Instance.BlockKinectUpdate = true;

        TrackerSceneOutputQuad.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(0, 0, 0, 1));

        //RainParticles.SetActive(true);
        //var RainController = RainParticles.GetComponent<RainController>();
        //RainController.Control = true;
        //RainController.Revert = true;
        //RainController.RevertDamping = 10f;

        ParticleCamera.SetActive(true);

        AbletonState = 4;
    }

    void PreDesmond()
    {
        Debug.Log("PreDesmond");
        //var emissionModule = RainParticles.GetComponentInChildren<ParticleSystem>().emission;
        //emissionModule.rateOverTime = 0;
        AbletonState = 5;
    }

    void RemoveRainParticles()
    {
        RainParticles.SetActive(false);
    }

    void ShowDesmond()
    {
        Debug.Log("ShowDesmond");
        UserMeshRenderer.enabled = true;
        UserMeshVisualizer.Instance.BlockKinectUpdate = false;
        TrackerSceneOutputQuad.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1, 1, 1, 1));
        UserMeshRenderer.material = Wireframe;
        MeshTools.EnableExplode = false;
        MeshTools.EnableDesmondAirsticksControl = true;
        AbletonState = 6;
    }

    void WireframeExplodeEnable()
    {
        Debug.Log("ExplodeEnable");
        UserMeshRenderer.enabled = true;
        MeshTools.EnableExplode = true;
        MeshTools.EnableDesmondAirsticksControl = true;
        UserMeshVisualizer.Instance.BlockKinectUpdate = true;
        AbletonState = 7;
    }

    void WireframeExplodeDisable()
    {
        Debug.Log("ExplodeDisable");
        UserMeshRenderer.enabled = true;
        MeshTools.EnableExplode = false;
        MeshTools.EnableDesmondAirsticksControl = true;
        UserMeshVisualizer.Instance.BlockKinectUpdate = false;
        AbletonState = 8;
    }

    void WireframeExplodeReEnable()
    {
        Debug.Log("ExplodeRena");
        UserMeshRenderer.enabled = true;
        MeshTools.EnableExplode = true;
        MeshTools.EnableDesmondAirsticksControl = true;
        UserMeshVisualizer.Instance.BlockKinectUpdate = true;
        AbletonState = 9;
    }

    void DesmondBreak()
    {
        Debug.Log("DesmondBreak");
        MeshTools.EnableExplode = false;
        MeshTools.EnableDesmondAirsticksControl = true;
        UserMeshVisualizer.BlockKinectUpdate = true;
        AbletonState = 10;
    }

    void DesmondOut()
    {
        ExitTrackerQuad = true;
        ExitTime = Time.time;
    }

    void ActivateGranRelatedBody()
    {
        UserParticles.SetActive(false);
        RainParticles.SetActive(false);
        DriftParticles.SetActive(false);

        UserMeshRenderer.enabled = true;
        UserMeshRenderer.material = new UnityEngine.Material(Pink);
        MeshTools.AnimateWireframeAlpha = false;
        MeshTools.Noise.SmoothingTimes = 2;
        MeshTools.Noise.NoiseIntensity = 0.05f;
        MeshTools.Noise.NewNoiseIntensity = 0.05f;

        TrackerSceneOutputQuad.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1, 1, 1, 1));
        TrackerSceneFlippedOutputQuad.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1, 1, 1, 1));

        AbletonState = 88;
    }

    void ActivateGranRelatedHands()
    {
        UserParticles.SetActive(true);
    }

    void ActivateHeifen()
    {
        RainParticles.SetActive(true);
        RainParticles.GetComponent<RainController>().Hone = true;
        RainParticlesWind.windMain = 0;
        var emissionModule = RainParticles.GetComponentInChildren<ParticleSystem>().emission;
        emissionModule.rateOverTime = 1000;
        emissionModule.enabled = true;
        UserMeshRenderer.enabled = false;
    }

    void TransitionOutOfHeifen()
    {
        RainParticles.GetComponent<RainController>().Revert = true;
        PreDesmond();
    }


    void ElectricPartOne()
    {
        UserParticles.SetActive(false);
        RainParticles.SetActive(false);
        DriftParticles.SetActive(false);
        // enable mesh renderer
        UserMeshRenderer.enabled = true;
        UserMeshRenderer.material = new UnityEngine.Material(LinePattern);

        TrackerSceneOutputQuad.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1, 1, 1, 1));
        TrackerSceneFlippedOutputQuad.GetComponent<Renderer>().material.SetColor("_TintColor", new Color(1, 1, 1, 1));

        MeshTools.EnableExplode = true;
        MeshTools.EnableDesmondAirsticksControl = false;
        UserMeshVisualizer.BlockKinectUpdate = false;


    }

    void ElectricPartTwo()
    {
        UserParticles.SetActive(false);
        RainParticles.SetActive(false);
        DriftParticles.SetActive(false);
        UserMeshRenderer.enabled = true;
        UserMeshRenderer.material = new UnityEngine.Material(Basic);
        UserMeshRenderer.material.SetColor("_EmissionColor", Color.yellow);

        MeshTools.EnableDesmondAirsticksControl = false;
        MeshTools.EnableExplode = false;
        MeshTools.Noise.NoiseIntensity = 0.06f;
        MeshTools.Noise.NewNoiseIntensity = 0.06f;
        MeshTools.Noise.SmoothingTimes = 0;

        UserMeshVisualizer.DisableMeshUpdate = false;
        UserMeshVisualizer.BlockKinectUpdate = false;

        TrackerSceneFlippedOutputQuad.SetActive(true);

        Lines.SetActive(true);
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
        if (ExitLines)
        {
            Lines.transform.Translate(0, Time.deltaTime * 8, 0);
        }
        if (ExitTrackerQuad)
        {

            var alpha = ((Time.time - ExitTime) / 10f);
            float h, s, v;
            Color.RGBToHSV(TrackerSceneOutputQuadRenderer.material.GetColor("_TintColor"), out h, out s, out v);
            TrackerSceneOutputQuadRenderer.material.SetColor("_TintColor", Color.HSVToRGB(h, alpha, alpha));
            TrackerSceneFlippedOutputQuadRenderer.material.SetColor("_TintColor", Color.HSVToRGB(h, alpha, alpha));
            if (alpha < 0.01f)
            {
                ExitTrackerQuad = false;
            }
        }

        DirectKeyCodeScenes();
    }
}
