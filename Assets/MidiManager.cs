using Eidetic.Unity.Utility;
using Midi;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;
using UnityEngine.PostProcessing;
using Eidetic.Andamooka;

public class
MidiManager : MonoBehaviour
{

    public static MidiManager Instance;

    public string DeviceName = "rtpMIDI";

    [HideInInspector()]
    public InputDevice InputDevice;

    public bool LogNoteOns = false;

    public bool FilterSingleChannel = false;
    public Channel FilterChannel;

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

    public GameObject PaintOutputQuad;

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

    BlockoutController BlackBlockout;
    BlockoutController WhiteBlockout;

    bool ExitLines = false;
    bool ExitTrackerQuad = false;
    float ExitTime = 0f;

    public Dictionary<String, ParticleSystem> RingKitSystems;

    GameObject ParticleSceneObject, RainParticlesObject;
    RainController RainParticleController;

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

        BlackBlockout = GameObject.Find("BlackBlockout").GetComponent<BlockoutController>();
        WhiteBlockout = GameObject.Find("WhiteBlockout").GetComponent<BlockoutController>();

        TrackerSceneOutputQuadRenderer = TrackerSceneOutputQuad.GetComponent<Renderer>();
        TrackerSceneFlippedOutputQuadRenderer = TrackerSceneFlippedOutputQuad.GetComponent<Renderer>();

        ParticleSceneObject = GameObject.Find("ParticleScene");
        RainParticlesObject = GameObject.Find("RainParticles");
        RainParticleController = GameObject.Find("RainParticleController").GetComponent<RainController>();

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

            InputDevice.NoteOn += InformationMonitor.Instance.MidiNoteOn;
            InputDevice.NoteOff += InformationMonitor.Instance.MidiNoteOff;

            Debug.Log("Opened MIDI Device");
        }
    }

    public void RouteNoteOn(Channel channel, Pitch pitch)
    {
        RouteNoteOn(
            new NoteOnMessage(
                InputDevice,
                channel,
                pitch,
                100,
                Time.time)
            );
    }

    public void RouteNoteOn(int channel, int noteNumber)
    {
        RouteNoteOn((Channel)channel - 1, (Pitch)noteNumber);
    }

    public void RouteNoteOn(int channel, Pitch pitch)
    {
        RouteNoteOn((Channel)channel - 1, pitch);
    }

    public void RouteNoteOn(NoteOnMessage noteOnMessage)
    {
        if (FilterSingleChannel && noteOnMessage.Channel != FilterChannel) return;

        if (LogNoteOns) Debug.Log(noteOnMessage.Channel + "." + noteOnMessage.Pitch);

        Threading.RunOnMain((Action)(() =>
        {
            switch (noteOnMessage.Channel)
            {
                case Channel.Channel1:
                    {
                        RouteCueMidi(noteOnMessage.Pitch);
                        break;
                    }
                case Channel.Channel9:
                    {
                        RouteOneFiveNine(noteOnMessage.Pitch);
                        break;
                    }
                case Channel.Channel14:
                    {
                        // CircleParticleController.Instance.BangRotation = true;
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
                case Channel.Channel4:
                    {
                        RouteHyphenMidi(noteOnMessage.Pitch);
                        break;
                    }
                case Channel.Channel5:
                    {
                        RouteUmbeantsMidi(noteOnMessage.Pitch);
                        break;
                    }
                case Channel.Channel6:
                    {
                        RouteJordanMidi(noteOnMessage.Pitch);
                        break;
                    }
                case Channel.Channel7:
                    {
                        RouteTunnelMidi(noteOnMessage.Pitch);
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

    private bool RodsOn = false;
    private bool RodsTwoHands = false;
    private bool VortexOn = false;

    private void RouteCueMidi(Pitch pitch)
    {
        switch (pitch)
        {
            case Pitch.A1:

                if (!RodsOn)
                {
                    BondiCueController.Instance.EnableRodsAfterSpeech = true;
                }
                else
                {
                    BondiCueController.Instance.DisableRodsForDrums = true;
                }
                RodsOn = !RodsOn;

                break;
            case Pitch.G1:
                if (!RodsTwoHands)
                {
                    BondiCueController.Instance.TwoHandedRodMelody = true;
                }
                RodsTwoHands = !RodsTwoHands;
                break;
            case Pitch.CSharp1:
                BondiCueController.Instance.EnableVortex = true;
                VortexOn = !VortexOn;
                break;
            case Pitch.ASharp1:
                BondiCueController.Instance.EnableRing = true;
                OneFiveNineCircleController.Instance.AirSticksKickSnare = true;
                break;
            case Pitch.DSharp1:
                OneFiveNineCircleController.Instance.AirSticksKickSnare = false;
                break;
        }
    }

    VideoPlaybackController TunnelVideoController;
    MeshRenderer TunnelPlane;
    public bool MeshIsWhite = false;

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
                OneFiveNineCircleController.Instance.DoSubKick();
                break;

            case Pitch.F3:
                // Invasion start
                // CircleParticleController.Instance.StartInvasion = true;
                break;
            case Pitch.FSharp3:
                // Invasion explode / beat in
                // CircleParticleController.Instance.StartInvasionRevert = true;
                OneFiveNineCircleController.Instance.AirSticksKickSnare = true;
                break;

            case Pitch.G3:
                // Go back to Airsticks Note On/Off with the
                OneFiveNineCircleController.Instance.ActivateAirSticksKickSnare = true;

                // Introduce circle hand systems?
                OneFiveNineCircleController.Instance.AirSticksKickSnare = true;
                break;

            case Pitch.A3:
                // Disable rods
                BondiCueController.Instance.DisableRodsAfterMelody = true;
                break;

            case Pitch.GSharp3:
                // Stop vortex
                BondiCueController.Instance.DisableVortex = true;
                break;

            case Pitch.E3:
                // turn rods white
                var rodParticles = GameObject.Find("RodParticleSystem").GetComponent<ParticleSystem>();
                var trailsModule = rodParticles.trails;
                trailsModule.colorOverLifetime = new ParticleSystem.MinMaxGradient(Color.white);
                break;

            case Pitch.DSharp4:
                BondiCueController.Instance.DisableRodsAfterMelody = true;
                break;

            case Pitch.E4:
                // (circles would come in )
                BondiCueController.Instance.EnableRodsAfterSpeech = true;
                break;
        }
    }

    private void RouteComputerRain(Pitch pitch)
    {
        switch (pitch)
        {
            case Pitch.B2:

                // temp transition out from one five nine
                NoiseCircleController.Instance.ExpandToRain = true;

                // start rain
                RainController.Instance.EnableEmission = true;
                if (GameObject.Find("OneFiveNine") != null)
                    GameObject.Find("OneFiveNine").SetActive(false);
                if (GameObject.Find("RodParticles") != null)
                    GameObject.Find("RodParticles").SetActive(false);
                if (TrackerSceneController.Instance != null)
                {
                    TrackerSceneController.Instance.EnableKinectUpdate = true;
                    TrackerSceneController.Instance.DisableUserRender = true;
                }
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
            // clone states
            case Pitch.C1:
                TrackerSceneController.Instance.SetCloneState(0);
                break;
            case Pitch.CSharp1:
                TrackerSceneController.Instance.SetCloneState(1);
                break;
            case Pitch.D1:
                TrackerSceneController.Instance.SetCloneState(2);
                break;
            case Pitch.DSharp1:
                TrackerSceneController.Instance.SetCloneState(3);
                break;
            case Pitch.E1:
                TrackerSceneController.Instance.SetCloneState(4);
                break;
            case Pitch.F1:
                TrackerSceneController.Instance.SetCloneState(5);
                break;
            case Pitch.FSharp1:
                TrackerSceneController.Instance.SetCloneState(6);
                break;
            case Pitch.G1:
                TrackerSceneController.Instance.SetCloneState(7);
                break;
            case Pitch.GSharp1:
                TrackerSceneController.Instance.SetCloneState(8);
                break;
            case Pitch.A1:
                TrackerSceneController.Instance.SetCloneState(9);
                break;
            case Pitch.ASharp1:
                TrackerSceneController.Instance.SetCloneState(10);
                break;
            case Pitch.B1:
                TrackerSceneController.Instance.SetCloneState(11);
                break;
            case Pitch.C2:
                TrackerSceneController.Instance.SetCloneState(12);
                break;
            case Pitch.CSharp2:
                TrackerSceneController.Instance.SetCloneState(13);
                break;
            case Pitch.D2:
                TrackerSceneController.Instance.SetCloneState(13);
                break;
        }
    }

    VideoPlaybackController BassVideo;
    VideoPlaybackController VibesVideo;

    void InitVideos()
    {
        if (BassVideo == null)
        {
            if (GameObject.Find("DesmondPlaybackBass") != null)
                BassVideo = GameObject.Find("DesmondPlaybackBass").GetComponent<VideoPlaybackController>();
        }
        if (VibesVideo == null)
        {
            if (GameObject.Find("DesmondPlaybackVibes") != null)
                VibesVideo = GameObject.Find("DesmondPlaybackVibes").GetComponent<VideoPlaybackController>();
        }
    }

    private void RouteDesmondMidi(Pitch pitch)
    {
        switch (pitch)
        {
            case Pitch.F2:
                if (TrackerSceneController.Instance != null)
                {
                    TrackerSceneController.Instance.EnableKinectUpdate = true;
                    TrackerSceneController.Instance.EnableUserRender = true;
                }
                // start desmond
                if (GameObject.Find("OutputQuad") != null)
                {
                    var renderer = GameObject.Find("OutputQuad").GetComponent<Renderer>();
                    renderer.material.SetColor("_TintColor", new Color(1, 1, 1, 1));
                }
                if (GameObject.Find("UserMesh") != null)
                {
                    var userMesh = GameObject.Find("UserMesh").GetComponent<Renderer>();
                    userMesh.enabled = true;
                    userMesh.material = Resources.Load<UnityEngine.Material>("Rainbow Wireframe");
                }
                MeshTools.Instance.EnableDesmondAirsticksControl = true;
                MeshTools.Instance.DesmondInstensityMinMax = new Vector2(-0.3f, 0.5f);
                MeshTools.Instance.DesmondSmoothingMinMax = new Vector2Int(0, 5);

                var initialPosition = UserMesh.transform.position;
                UserMesh.transform.position = new Vector3(initialPosition.x, -0.72f, initialPosition.z);

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
                if (UserMeshVisualizer.Instance != null)
                    UserMeshVisualizer.Instance.BlockKinectUpdate = true;


                // BassVideo.JumpToCue(13);
                break;
            case Pitch.A3:
                // end breakdown / back to form
                if (UserMeshVisualizer.Instance != null)
                    UserMeshVisualizer.Instance.BlockKinectUpdate = false;
                // BassVideo.JumpToCue(18);
                break;
            case Pitch.ASharp3:
                // drum/guitar outro setting 
                // BassVideo.JumpToCue(22);
                break;
            case Pitch.B3:
                // guitar solo shape
                break;
            case Pitch.C4:
                if (UserMeshVisualizer.Instance != null)
                    UserMeshVisualizer.Instance.BlockKinectUpdate = true;
                // ending transition, five second fade to black
                BlackBlockout.FadeIn(5000);
                // make sure coming scene is active
                PaintScene.SetActive(true);
                FaceScene.SetActive(true);
                break;
            case Pitch.C5:
                // clear blockout
                BlackBlockout.FadeOut(1);
                TrackerSceneController.Instance.DisableUserRender = true;
                // activate hyphen
                GameObject.Find("PaintAndFaceOutputQuad").GetComponent<MeshRenderer>()
                    .enabled = true;
                GameObject.Find("PaintAndSingersOutputQuad").GetComponent<MeshRenderer>()
                    .enabled = true;
                PaintOutputQuad.SetActive(true);
                // remove the bloom main camera
                var pp = GameObject.Find("Main Camera").GetComponent<PostProcessingBehaviour>();
                pp.enabled = false;
                break;
        }
    }

    public GameObject PaintScene;
    public GameObject FaceScene;

    private void RouteHyphenMidi(Pitch pitch)
    {
        switch (pitch)
        {
            case Pitch.C3:
                PaintSceneController.Instance.DrawFullTrails = true;
                break;
            case Pitch.D3:
                PaintSceneController.Instance.SuckTrails = true;
                break;
            case Pitch.CSharp3:
                PaintSceneController.Instance.DisableTrails = true;
                break;
            case Pitch.DSharp3:
                // start beatboxer head
                var headFeedback = GameObject.Find("PaintCamera").GetComponent<Kino.Feedback>();
                headFeedback.color = Color.HSVToRGB(1, 0, 0.98f);
                HeadController.Instance.Transparency = 0f;
                HeadController.Instance.FadeInLength = 7f;
                HeadController.Instance.FadeIn = true;
                break;
            case Pitch.B0:
                // start singers
                SingersController.Instance.Play();
                SingersController.Instance.Transparency = 1f;
                SingersController.Instance.FlyIn = true;
                SingersController.Instance.EnableKickParticles = true;
                break;

            case Pitch.A2:
                HeadController.Instance.Transparency = 1f;
                HeadController.Instance.FadeOutLength = .5f;
                HeadController.Instance.FadeOut = true;
                break;

            case Pitch.E4:
                // full paint on face
                var feedback = GameObject.Find("PaintCamera").GetComponent<Kino.Feedback>();
                feedback.color = Color.HSVToRGB(1, 0, 1);
                break;

            case Pitch.F4:
                // fading paint on face
                var faceFeedback = GameObject.Find("PaintCamera").GetComponent<Kino.Feedback>();
                faceFeedback.color = Color.HSVToRGB(1, 0, 0.98f);
                break;

            case Pitch.B2:
                // outro stutter
                SingersController.Instance.Transparency = 1f;
                SingersController.Instance.FadeOutLength = .25f;
                SingersController.Instance.FadeOut = true;
                break;

            case Pitch.C1:
                // intro 1 pulse, right
                HitParticleController.Instance.IntroPulse1();
                break;
            case Pitch.CSharp1:
                // intro 2 left
                HitParticleController.Instance.IntroPulse2();
                break;
            case Pitch.D1:
                HitParticleController.Instance.IntroPulse3();
                // intro 2 right
                break;
            case Pitch.F1:
                HitParticleController.Instance.Melody1();
                // melody note 1
                break;
            case Pitch.DSharp1:
                HitParticleController.Instance.Melody2();
                break;
            case Pitch.E1:
                HitParticleController.Instance.Melody3();
                break;
            case Pitch.FSharp1:
                // snap strike before beat
                HitParticleController.Instance.Snap();
                break;
            case Pitch.A1:
                HitParticleController.Instance.Kick();
                HeadController.Instance.Kick();
                SingersController.Instance.TriggerKick = true;
                break;
            case Pitch.ASharp1:
                HeadController.Instance.Hats();
                break;
            case Pitch.DSharp2:
                // HitParticleController.Instance.Snare();
                HeadController.Instance.Snare();
                HitParticleController.Instance.LeftHand();
                break;
            case Pitch.D2:
                HitParticleController.Instance.RightHand();
                break;
            case Pitch.F2:
                // HitParticleController.Instance.LeftHand();
                break;
            default:
                // for beatbox animations
                HeadController.Instance.Beatbox(pitch.NoteNumber());
                break;

        }
    }

    private void RouteUmbeantsMidi(Pitch pitch)
    {
        switch (pitch)
        {
            // start umbeants
            case Pitch.C1:
                // turn off rain wind
                GameObject.Find("WindZoneA").SetActive(false);
                // start system emission 
                DriftController.FadeIn = true;
                // remove hyphen layers
                GameObject.Find("FaceScene").SetActive(false);
                GameObject.Find("PaintScene").SetActive(false);
                // add the bloom back to main camera
                var pp = GameObject.Find("Main Camera").GetComponent<PostProcessingBehaviour>();
                pp.enabled = true;
                break;

            // end umbeants
            case Pitch.CSharp1:
                // end system emission
                DriftController.FadeOut = true;
                break;

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
            case Pitch.ASharpNeg1:
                DriftController.SelectState(10);
                break;
            case Pitch.BNeg1:
                DriftController.SelectState(11);
                break;
        }
    }


    private void RouteJordanMidi(Pitch pitch)
    {
        switch (pitch)
        {

            case Pitch.C0:
                // start jordan
                TrackerSceneController.Instance.EnableUserRender = true;
                TrackerSceneController.Instance.EnableKinectUpdate = true;
                UserFreezeFrameController.Instance.HideBaseOuput = true;

                MeshTools.Instance.EnableDesmondAirsticksControl = false;

                MeshTools.Instance.GoToJordanState0 = true;

                MeshTools.Instance.AnimateWireframeAlpha = false;
                UserMeshRenderer.material = Resources.Load("JordanMaterial") as UnityEngine.Material;

                // Move figure (now on drum kit)
                var initialPosition = UserMesh.transform.position;
                UserMesh.transform.position = new Vector3(initialPosition.x, -0.99f, initialPosition.z);

                // Activate Jordan light
                GameObject.Find("JordanLight").GetComponent<Light>().enabled = true;

                break;

            case Pitch.CSharp0:
                UserMeshVisualizer.Instance.DoRotate = true;
                UserMeshVisualizer.Instance.DoSetMedianVertexPosition = true;
                UserMeshVisualizer.Instance.StartRotateAnimation = true;
                JordanRotator.Instance.Enabled = true;
                JordanRotator.Instance.YAmount = 2;
                break;

            case Pitch.D0:
                UserMeshVisualizer.Instance.DoSetMedianVertexPosition = true;
                JordanRotator.Instance.Enabled = true;
                JordanRotator.Instance.YAmount = 4;
                break;

            case Pitch.DSharp0:
                UserMeshVisualizer.Instance.DoSetMedianVertexPosition = true;
                JordanRotator.Instance.Enabled = true;
                JordanRotator.Instance.YAmount = 6;
                break;

            case Pitch.E0:
                UserMeshVisualizer.Instance.DoSetMedianVertexPosition = true;
                JordanRotator.Instance.Enabled = true;
                JordanRotator.Instance.YAmount = 8;
                break;

            case Pitch.F0:
                UserMeshVisualizer.Instance.DoSetMedianVertexPosition = true;
                JordanRotator.Instance.Enabled = true;
                JordanRotator.Instance.YAmount = 10;
                break;

            case Pitch.FSharp0:
                UserMeshVisualizer.Instance.DoSetMedianVertexPosition = true;
                JordanRotator.Instance.Enabled = true;
                JordanRotator.Instance.YAmount = 12;
                break;

            case Pitch.G0:
                UserMeshVisualizer.Instance.DoSetMedianVertexPosition = true;
                JordanRotator.Instance.Enabled = true;
                JordanRotator.Instance.YAmount = 15;
                break;

            case Pitch.GSharp0:
                UserMeshVisualizer.Instance.DoSetMedianVertexPosition = true;
                JordanRotator.Instance.Enabled = true;
                JordanRotator.Instance.YAmount = 18;
                break;

            case Pitch.A0:
                UserMeshVisualizer.Instance.DoSetMedianVertexPosition = true;
                JordanRotator.Instance.Enabled = true;
                JordanRotator.Instance.YAmount = 22;
                break;

            case Pitch.B1:
                MeshTools.Instance.GoToJordanState0 = true;
                break;
            case Pitch.C2:
                MeshTools.Instance.GoToJordanState1 = true;
                break;
            case Pitch.CSharp2:
                MeshTools.Instance.GoToJordanState2 = true;
                break;
            case Pitch.D2:
                MeshTools.Instance.GoToJordanState3 = true;
                break;
            case Pitch.DSharp2:
                MeshTools.Instance.GoToJordanState4 = true;
                break;
            case Pitch.E2:
                MeshTools.Instance.GoToJordanState5 = true;
                break;

            // flashes
            case Pitch.B0:
                UserFreezeFrameController.Instance.FadeLength = 3f;
                UserFreezeFrameController.Instance.Generate = true;
                break;
            case Pitch.C1:
                UserFreezeFrameController.Instance.FadeLength = 1.8f;
                UserFreezeFrameController.Instance.Generate = true;
                break;
            case Pitch.CSharp1:
                UserFreezeFrameController.Instance.FadeLength = 0.8f;
                UserFreezeFrameController.Instance.Generate = true;
                break;
            case Pitch.D1:
                UserFreezeFrameController.Instance.FadeLength = 0.5f;
                UserFreezeFrameController.Instance.Generate = true;
                break;
            case Pitch.DSharp1:
                UserFreezeFrameController.Instance.FadeLength = 0.35f;
                UserFreezeFrameController.Instance.Generate = true;
                break;
            case Pitch.E1:
                UserFreezeFrameController.Instance.FadeLength = 0.15f;
                UserFreezeFrameController.Instance.Generate = true;
                break;
        }
    }

    private void RouteTunnelMidi(Pitch pitch)
    {
        if (TunnelVideoController == null)
        {
            TunnelVideoController = GameObject.Find("TunnelPlayback").GetComponent<VideoPlaybackController>();
            TunnelPlane = GameObject.Find("TunnelPlane").GetComponent<MeshRenderer>();
        }
        switch (pitch)
        {
            case Pitch.C1:
                // remove everything else

                var paintScene = GameObject.Find("PaintScene");
                if (paintScene != null)
                    paintScene.SetActive(false);
                var faceScene = GameObject.Find("FaceScene");
                if (faceScene != null)
                    faceScene.SetActive(false);
                var particleScene = GameObject.Find("ParticleScene");
                if (particleScene != null)
                    particleScene.SetActive(false);

                // start tunnel
                TunnelVideoController.StartPlayback = true;
                var color = TunnelPlane.material.GetColor("_Color");
                TunnelPlane.material.SetColor("_Color", new Color(color.r, color.g, color.b, 1));
                // preset state for Melody Circles
                if (MelodyCirclesController.Instance != null)
                    MelodyCirclesController.Instance.GoToPreset();
                break;
            case Pitch.CSharp1:
                // bring in user on finale outro
                TrackerSceneController.Instance.EnableKinectUpdate = true;
                TrackerSceneController.Instance.EnableUserRender = true;

                // make sure kinect output quad is fully opaque (white)
                var outputQuad = TrackerSceneController.Instance.OutputQuad.GetComponent<MeshRenderer>();
                outputQuad.material.SetColor("_TintColor", Color.white);

                // and no jordan rotation states
                if (JordanRotator.Instance != null)
                {
                    JordanRotator.Instance.YAmount = 0f;
                    JordanRotator.Instance.XAmount = 0f;
                    JordanRotator.Instance.XSpeed = 0f;
                    JordanRotator.Instance.YSpeed = 0f;
                    JordanRotator.Instance.PosAmount = 0f;
                }

                MeshTools.Instance.Noise.NoiseIntensity = 0.005f;
                MeshTools.Instance.Noise.NewNoiseIntensity = 0.005f;
                MeshTools.Instance.Noise.SmoothingTimes = 1;
                MeshTools.EnableDesmondAirsticksControl = false;
                MeshTools.AnimateWireframeAlpha = false;
                
                UserMesh.GetComponent<MeshRenderer>().enabled = true;
                UserMeshRenderer.material = Resources.Load<UnityEngine.Material>("White");
                MeshIsWhite = true;
                break;
            case Pitch.D1:
                // final final cueF
                RainParticles.SetActive(true);
                // fade out tunnel video and make user render white
                VideoLayersController.Instance.FadeOutTunnel = true;
                UserMeshRenderer.material = Resources.Load<UnityEngine.Material>("White");
                // start user render fadeout
                TrackerSceneController.Instance.FadeOutStart = true;
                // Start Particle scene finale
                ParticleSceneController.Instance.FinaleState = true;
                RainController.Instance.ActivateDefaultParticleColour = true;
                // and start fading them out
                ParticleSceneObject.SetActive(true);
                RainParticlesObject.SetActive(true);
                RainParticleController.StopParticleLength = 200f;
                RainParticleController.StopParticleAmounts = new Vector2Int(4000, 4000);
                RainParticleController.SlowlyStopParticles = true;
                break;
            case Pitch.E1:
                RainParticleController.StopParticleLength = 1f;
                break;
            case Pitch.DSharp1:
                // evaporate
                break;
            case Pitch.CSharp6:
                // Crash toggles colour
                if (MeshIsWhite)
                {
                    UserMeshRenderer.material = Resources.Load<UnityEngine.Material>("Black");
                    MeshIsWhite = false;
                }
                else
                {
                    UserMeshRenderer.material = Resources.Load<UnityEngine.Material>("White");
                    MeshIsWhite = true;
                }
                break;
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
        RainParticles.GetComponentInChildren<RainController>().Control = true;
        ParticleCamera.SetActive(true);
        if (UserParticles != null)
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

        // enable the clones
        for (int i = 0; i < 12; i++)
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
        if (UserParticles != null)
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
        if (UserParticles != null)
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
        if (UserParticles != null)
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
        if (UserParticles != null)
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
        RainParticles.GetComponentInChildren<RainController>().Control = false;
        RainParticles.SetActive(false);
        ParticleCamera.SetActive(false);
        if (UserParticles != null)
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

        if (SendTestNoteOn)
        {
            RouteNoteOn(TestChannel, TestPitch);
            SendTestNoteOn = false;
        }

        //DirectKeyCodeScenes();
    }

    [Header("Midi Test")]
    public Channel TestChannel;
    public Pitch TestPitch;
    public bool SendTestNoteOn;
}
