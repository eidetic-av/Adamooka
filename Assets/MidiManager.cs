using Eidetic.Unity.Utility;
using Midi;
using System;
using UnityEngine;

public class MidiManager : MonoBehaviour
{

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

    // Use this for initialization
    void Start()
    {
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
        if (LogNoteOns) Debug.Log(noteOnMessage.Pitch);

        Threading.RunOnMain(() =>
        {
            switch (noteOnMessage.Channel)
            {
                case Channel.Channel1:
                    {
                        if (noteOnMessage.Pitch == Pitch.D2)
                        {
                            OneFiveNineCircleController.Instance.Beep = true;
                        }
                        break;
                    }
                case Channel.Channel2:
                    {
                        if (noteOnMessage.Pitch == Pitch.C2)
                        {
                            OneFiveNineCircleController.Instance.FlashNonagon = true;
                        }
                        break;
                    }
                case Channel.Channel3:
                    {
                        OneFiveNineCircleController instance = OneFiveNineCircleController.Instance;
                        if (noteOnMessage.Pitch == Pitch.C2)
                        {
                            // Kick
                            
                        } else if (noteOnMessage.Pitch == Pitch.D2)
                        {
                            // Snare
                        }
                        break;
                    }
                case Channel.Channel4:
                    {
                        RouteHyphenMidi(noteOnMessage.Pitch);
                        break;
                    }
                case Channel.Channel14:
                    {
                        RouteElectricMidi(noteOnMessage.Pitch);
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
        });
    }

    private void RouteOneFiveNine(Pitch pitch)
    {
            SnakeController.Instance.Advance = true;
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
                if (AbletonState != 5 && AbletonState < 6)
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
            MeshTools.EnableDesmondAirsticksControl = false;
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

        TrackerOutputEffector.CloningActive = true;

        MeshTools.EnableExplode = false;

        AbletonState = 3;
    }

    void EndClones()
    {
        TrackerOutputEffector.HideClones(AirSticks.Hand.Left);
        TrackerOutputEffector.HideClones(AirSticks.Hand.Right);
        TrackerOutputEffector.CloningActive = false;
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
        Debug.Log("StopRain");
        var emissionModule = RainParticles.GetComponentInChildren<ParticleSystem>().emission;
        emissionModule.rateOverTime = 0;
        AbletonState = 5;
    }

    void RemoveRainParticles()
    {
        RainParticles.SetActive(false);
    }

    void ShowWireframe()
    {
        Debug.Log("ShowWireframe");
        UserMeshRenderer.enabled = true;
        UserMeshVisualizer.BlockKinectUpdate = false;
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
        AbletonState = 7;
    }

    void WireframeExplodeDisable()
    {
        Debug.Log("ExplodeDisable");
        UserMeshRenderer.enabled = true;
        MeshTools.EnableExplode = false;
        MeshTools.EnableDesmondAirsticksControl = true;
        AbletonState = 8;
    }

    void WireframeExplodeReEnable()
    {
        Debug.Log("ExplodeRena");
        UserMeshRenderer.enabled = true;
        MeshTools.EnableExplode = true;
        MeshTools.EnableDesmondAirsticksControl = true;
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
        StopRain();
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
