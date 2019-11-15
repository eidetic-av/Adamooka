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
    // public ParticleController HitParticlesController;
    public GameObject UserMesh;
    public Renderer UserMeshRenderer;
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
                        break;
                    }
                case Channel.Channel3:
                    {
                        break;
                    }
                case Channel.Channel4:
                    {
                        break;
                    }
                case Channel.Channel5:
                    {
                        break;
                    }
                case Channel.Channel6:
                    {
                        break;
                    }
                case Channel.Channel7:
                    {
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

    public GameObject PaintScene;
    public GameObject FaceScene;

    private void UpdateAbletonState(Pitch pitch)
    {
        switch (pitch)
        {
            case Pitch.E2:
                break;
            case Pitch.E1:
                break;
            case Pitch.D2:
                break;
            case Pitch.DSharp2:
                break;
            case Pitch.F1:
                break;
            case Pitch.F2:
                break;
            case Pitch.FSharp2:
                break;
            case Pitch.CSharp3:
                break;
            case Pitch.G2:
                break;
            case Pitch.D3:
                break;
            case Pitch.GSharp2:
                break;
            case Pitch.ASharp2:
                break;
            case Pitch.C3:
                break;
            case Pitch.A2:
                break;
            case Pitch.B2:
                break;
        }
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
