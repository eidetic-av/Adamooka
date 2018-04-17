using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic.Devices;
using Midi;

public class MAMRibbonController : MonoBehaviour
{

    ParticleSystem MAMRibbon;

    BeatStepPro BeatStepPro;

    Minitaur Minitaur = new Minitaur();
    Tanzmaus Tanzmaus;

    public int EmissionCount = 1;

    public float SphereDampRate = 3;

    int SphereFlashPosition = 0;

    GameObject[] Spheres = new GameObject[16];
    Vector2[] SphereAlpha = new Vector2[16];

    GameObject BlockoutQuad;
    public float BlockoutDampRate = 20;
    Vector2 BlockoutAlpha = Vector2.zero;

    public float DebounceTime = 0.05f;
    float DebouncePosition = 0;

    float RowDebounceTime;
    float RowDebouncePosition;

    GameObject UserMesh;
    Vector2 MeshDeform = Vector2.zero;
    public float MinMeshDeform = 0;
    public float MaxMeshDeform = 2;
    public float MeshDeformDampRate = 25;

    // Use this for initialization
    void Start()
    {
        MAMRibbon = GameObject.Find("MAMRibbon").GetComponent<ParticleSystem>();
        Minitaur.AttachInputDevice("MIDIIN6 (mio4)");

        BeatStepPro = GameObject.Find("UpdateController").GetComponent<BeatStepPro>();

        BlockoutQuad = GameObject.Find("BlockoutQuad");

        Minitaur.AddNoteOnAction(DoNoteOn);
        Minitaur.AddNoteOffAction(DoNoteOff);
        Tanzmaus = new Tanzmaus();
        Tanzmaus.AddNoteOnAction(DoTanzmausNoteOn);

        UserMesh = GameObject.Find("UserMesh");

        for (int i = 0; i < 16; i++)
        {
            Spheres[i] = GameObject.Find("Sphere (" + (i + 1) + ")");
            SphereAlpha[i] = new Vector2(0, 0);
        }
        // Minitaur.AddControlChangeAction(ControlChange);
    }

    // Update is called once per frame
    void Update()
    {
        for (int i = 0; i < 16; i++)
        {
            if (Mathf.Abs(SphereAlpha[i].x - SphereAlpha[i].y) > 0)
            {
                SphereAlpha[i].x = SphereAlpha[i].x + (SphereAlpha[i].y - SphereAlpha[i].x) / SphereDampRate;
                var sphereColor = Spheres[i].GetComponent<MeshRenderer>().material.color;
                sphereColor.a = SphereAlpha[i].x;
                Spheres[i].GetComponent<MeshRenderer>().material.color = sphereColor;
            }
        }

        if (Mathf.Abs(BlockoutAlpha.x - BlockoutAlpha.y) > 0)
        {
            BlockoutAlpha.x = BlockoutAlpha.x + (BlockoutAlpha.y - BlockoutAlpha.x) / BlockoutDampRate;
            var material = BlockoutQuad.GetComponent<MeshRenderer>().material;
            material.color = new Color(1, 1, 1, BlockoutAlpha.x);
            BlockoutQuad.GetComponent<MeshRenderer>().material = material;
        }

        if (Mathf.Abs(MeshDeform.x - MeshDeform.y) > 0)
        {
            MeshDeform.x = MeshDeform.x + (MeshDeform.y - MeshDeform.x) / MeshDeformDampRate;
            var noise = UserMesh.GetComponent<MeshTools>().Noise;
            noise.NoiseIntensity = MeshDeform.x;
        }

        DebouncePosition += Time.deltaTime;
        RowDebouncePosition += Time.deltaTime;

        RowDebounceTime = DebounceTime;
    }

    void OnDestroy()
    {
        foreach (InputDevice d in InputDevice.InstalledDevices)
        {
            if (d.IsReceiving)
            {
                d.StopReceiving();
            }
            if (d.IsOpen)
            {
                d.Close();
            }
        }
    }

    void FlashSphere(int velocity)
    {
        if (DebouncePosition > DebounceTime)
        {
            SphereAlpha[SphereFlashPosition].x = 1f;
            SphereAlpha[SphereFlashPosition].y = 0f;
            SphereFlashPosition = (SphereFlashPosition + 1) % 16;
            DebouncePosition = 0;
            if (velocity >= 120)
            {
                var material = Spheres[SphereFlashPosition].GetComponent<MeshRenderer>().material;
                material.color = Random.ColorHSV();
                Spheres[SphereFlashPosition].GetComponent<MeshRenderer>().material = material;
            }
            else
            {
                var material = Spheres[SphereFlashPosition].GetComponent<MeshRenderer>().material;
                material.color = Color.white;
                Spheres[SphereFlashPosition].GetComponent<MeshRenderer>().material = material;
            }
        }
    }
    
    void FlashRow()
    {
        if (RowDebouncePosition >= RowDebounceTime)
        {
            int a = Mathf.FloorToInt(Random.Range(1, 13));
            int b = Mathf.FloorToInt(Random.Range(1, 13));
            int c = Mathf.FloorToInt(Random.Range(1, 13));
            Flash(a);
            Flash(b);
            Flash(c);
            RowDebouncePosition = 0;
        }
    }

    void Flash(int sphereIndex)
    {
        SphereAlpha[sphereIndex].x = 1f;
        SphereAlpha[sphereIndex].y = 0f;
    }

    void FlashBlockoutQuad()
    {
        BlockoutAlpha.x = 1f;
        BlockoutAlpha.y = 0f;
    }

    void DoNoteOn(int noteNumber, int velocity)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(NoteOn(noteNumber, velocity));
    }
    void DoNoteOff(int noteNumber)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(NoteOff(noteNumber));
    }
    IEnumerator NoteOn(int noteNumber, int velocity)
    {
        var mainModule = MAMRibbon.main;
        mainModule.maxParticles = EmissionCount;
        MAMRibbon.Emit(EmissionCount);
        yield return null;
    }
    IEnumerator NoteOff(int noteNumber)
    {
        MAMRibbon.SetParticles(new ParticleSystem.Particle[0], 0);
        yield return null;
    }
    void ControlChange(int ccNumber, int value)
    {
    }

    void DoTanzmausNoteOn(int noteNumber, int velocity)
    {
        Debug.Log("TanzmausOn: " + noteNumber);
        UnityMainThreadDispatcher.Instance().Enqueue(TanzmausNoteOn(noteNumber, velocity));
    }
    IEnumerator TanzmausNoteOn(int noteNumber, int velocity)
    {
        switch (noteNumber)
        {
            case 60:
                // Kick
                if (BeatStepPro.KickMeshDeform)
                {
                    MeshDeform.x = MaxMeshDeform;
                    MeshDeform.y = MinMeshDeform;
                }
                break;
            case 63:
                // Clap
                FlashBlockoutQuad();
                break;
            case 66:
                // SP1
                FlashRow();
                break;
            case 68:
                // SP2
                FlashSphere(velocity);
                break;
        }
        yield return null;
    }


}

namespace Eidetic.Devices
{

    class Tanzmaus : DrumMachine
    {

        public Tanzmaus() : base()
        {

        }

        public override string MidiDeviceName { get; protected set; }
            = "MIDIIN4 (mio4)";

        override public void Setup()
        {
            NoteOnActionPointers = new List<System.Action<int, int>>();
        }
        override protected void ControlChange(ControlChangeMessage ccMessage)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(DoControlChange(ccMessage.Control.Number(), ccMessage.Value));
        }

        IEnumerator DoControlChange(int ccNumber, int value)
        {
            // ControlChangeActionPointers
            yield return null;
        }
        IEnumerator DoNoteOn(int noteNumber, int velocity)
        {
            NoteOnActionPointers.ForEach(a => a.Invoke(noteNumber, velocity));
            yield return null;
        }
        override protected void NoteOn(NoteOnMessage noteOnMessage)
        {
            if (noteOnMessage.Channel.Number() == 10)
            {
                UnityMainThreadDispatcher.Instance().Enqueue(DoNoteOn(noteOnMessage.Pitch.NoteNumber(), noteOnMessage.Velocity));
            }
        }

        override protected void NoteOff(NoteOffMessage noteOffMessage)
        {
        }
        void OnDestroy()
        {
            foreach (InputDevice d in InputDevice.InstalledDevices)
            {
                if (d.IsReceiving)
                {
                    d.StopReceiving();
                }
                if (d.IsOpen)
                {
                    d.Close();
                }
            }
        }
    }
}