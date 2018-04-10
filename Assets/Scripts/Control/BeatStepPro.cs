using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Midi;
using Eidetic.Utility;

public class BeatStepPro : MonoBehaviour
{

    public string DeviceName = "Arturia Beatstep Pro";
    InputDevice InputDevice;

    GameObject MAMRibbon;
    MAMRibbonController MAMRibbonController;

    GameObject Spheres;

    GameObject ManualBlackoutQuad;
    Vector2 BlackoutAlpha = Vector2.zero;
    float ManualBlackoutDamping = 4;

    GameObject UserMesh;

    GameObject RaycastSilhouette;

    LineInterrupter.LineInterrupt LineInterrupt;

    GameObject TrackerCamera;

    public bool KickMeshDeform = false;

    public float CircleHue { get; set; } = 0;
    // Use this for initialization
    void Start()
    {
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
        }
        InputDevice.ControlChange += RecieveControlChange;
        InputDevice.NoteOn += RecieveNoteOn;

        MAMRibbon = GameObject.Find("MAMRibbon");
        MAMRibbonController = GameObject.Find("MAMRibbonController").GetComponent<MAMRibbonController>();

        ManualBlackoutQuad = GameObject.Find("ManualBlackoutQuad");

        UserMesh = GameObject.Find("UserMesh");

        Spheres = GameObject.Find("Spheres");

        RaycastSilhouette = GameObject.Find("RaycastSillhouete");

        LineInterrupt = RaycastSilhouette.GetComponent<LineInterrupter.LineInterrupt>();

        TrackerCamera = GameObject.Find("Tracker Camera");
    }
    

    void RecieveControlChange(ControlChangeMessage controlChangeMessage)
    {
        int channel = controlChangeMessage.Channel.Number();
        int ccNumber = controlChangeMessage.Control.Number();
        int value = controlChangeMessage.Value;

        UnityMainThreadDispatcher.Instance().Enqueue(ControlChange(ccNumber, value));
    }

    void RecieveNoteOn(NoteOnMessage noteOnMessage)
    {
        int channel = noteOnMessage.Channel.Number();
        int noteNumber = noteOnMessage.Pitch.NoteNumber();
        int velocity = noteOnMessage.Velocity;

        UnityMainThreadDispatcher.Instance().Enqueue(NoteOn(noteNumber, velocity));
    }

    IEnumerator NoteOn(int noteNumber, int velocity)
    {

        if (noteNumber == 1)
        {
            LineInterrupt.VisualiseLeftRaycasts = !LineInterrupt.VisualiseLeftRaycasts;
        }
        else if (noteNumber == 2)
        {
            LineInterrupt.VisualiseRightRaycasts = !LineInterrupt.VisualiseRightRaycasts;
        }
        else if (noteNumber == 3)
        {
            LineInterrupt.VisualiseLeftConnectors = !LineInterrupt.VisualiseLeftConnectors;
        }
        else if (noteNumber == 4)
        {
            LineInterrupt.VisualiseRightConnectors = !LineInterrupt.VisualiseRightConnectors;
        }
        else if (noteNumber == 5)
        {
            switch (Mathf.CeilToInt(Random.Range(0, 4)))
            {
                case 1:
                    LineInterrupt.VisualiseLeftRaycasts = !LineInterrupt.VisualiseLeftRaycasts;
                    break;
                case 2:
                    LineInterrupt.VisualiseRightRaycasts = !LineInterrupt.VisualiseRightRaycasts;
                    break;
                case 3:
                    LineInterrupt.VisualiseLeftConnectors = !LineInterrupt.VisualiseLeftConnectors;
                    break;
                case 4:
                    LineInterrupt.VisualiseRightConnectors = !LineInterrupt.VisualiseRightConnectors;
                    break;
            }
        }
        else if (noteNumber == 5)
        {
            GameObject.Find("Main Camera").GetComponent<PostProfileEditor>().RandomiseHue();
        }
        yield return null;
    }

    IEnumerator ControlChange(int ccNumber, int value)
    {
        switch (ccNumber)
        {
            case 1:
                UpdateRibbonColor(value);
                break;
            case 2:
                UpdateRibbonEmissionCount(value);
                break;
            case 3:
                UpdateRibbonNoiseFrequency(value);
                break;
            case 4:
                UpdateRibbonWidth(value);
                break;
            case 5:
                UpdateRotationRateY(value);
                break;
            case 6:
                UpdateInterruptMoveSpeed(value);
                break;

            case 9:
                UpdateMeshNoise(value);
                break;
            case 10:
                MAMRibbonController.MeshDeformDampRate = ((float)value).Map(0f, 127f, 3, 40);
                break;
            case 11:
                MAMRibbonController.MaxMeshDeform = ((float)value).Map(0f, 127f, .05f, 10);
                break;

            case 16:
                UpdateManualBlackoutQuad(value);
                break;
            case 20:
                EnableMAMRibbon(value);
                break;
            case 21:
                EnableSpheres(value);
                break;
            case 24:
                EnableRaycastSilhouette(value);
                break;
            case 28:
                EnableUserMesh(value);
                break;
            case 29:
                EnableKickMeshDeform(value);
                break;
        }

        yield return null;
    }

    void EnableKickMeshDeform(int value)
    {
        KickMeshDeform = (value >= 64);
    }

    void UpdateMeshNoise(int midiValue)
    {
        var SmoothScript = UserMesh.GetComponent<SmoothMyMesh>();
        SmoothScript.NoiseIntensity = ((float)midiValue).Map(0, 127, 0, 3);
    }

    void UpdateRotationRateY(int midiValue)
    {
        LineInterrupt.RotationRate.x = ((float)midiValue).Map(0f, 127f, 0f, 3f);
    }

    void UpdateInterruptMoveSpeed(int midiValue)
    {
        RaycastSilhouette.GetComponent<EndPointAnimator>().Rate = ((float)midiValue).Map(0f, 127f, 0f, 50f);
    }

    void EnableMAMRibbon(int midiValue)
    {
        MAMRibbon.SetActive(midiValue >= 64);
    }

    void EnableSpheres(int midiValue)
    {
        Spheres.SetActive(midiValue >= 64);
    }

    void EnableRaycastSilhouette(int midiValue)
    {
        bool enabled = midiValue >= 64;
        RaycastSilhouette.SetActive(enabled);
        var lineInterrupt = RaycastSilhouette.GetComponent<LineInterrupter.LineInterrupt>();
        foreach (GameObject gameObject in lineInterrupt.LeftInterruptLineObjects)
        {
            gameObject.SetActive(enabled);
        }
        foreach (GameObject gameObject in lineInterrupt.RightRaycastLineObjects)
        {
            gameObject.SetActive(enabled);
        }
        foreach (GameObject gameObject in lineInterrupt.LeftConnectingQuadObjects)
        {
            gameObject.SetActive(enabled);
        }
        foreach (GameObject gameObject in lineInterrupt.RightConnectingQuadObjects)
        {
            gameObject.SetActive(enabled);
        }
    }

    void EnableUserMesh(int midiValue)
    {
        if (midiValue >= 64)
        {
            var renderer = UserMesh.GetComponent<MeshRenderer>();
            renderer.enabled = true;
        }
        else
        {
            var renderer = UserMesh.GetComponent<MeshRenderer>();
            renderer.enabled = false;
        }
    }

    void UpdateRibbonColor(int midiValue)
    {
        float newHue = ((float)midiValue).Map(0, 127, 0, 1);
        CircleHue = ((float)midiValue).Map(127, 0, 0, 1);

        var renderer = MAMRibbon.GetComponent<ParticleSystem>().GetComponent<ParticleSystemRenderer>();
        var trailMaterialColor = renderer.trailMaterial.color;

        float h = 0f, s = 0f, v = 0f;
        Color.RGBToHSV(trailMaterialColor, out h, out s, out v);

        trailMaterialColor = Color.HSVToRGB(newHue, s, v);

        renderer.trailMaterial.color = trailMaterialColor;
    }

    void UpdateRibbonEmissionCount(int midiValue)
    {
        int emissionCount = Mathf.CeilToInt(((float)midiValue).Map(0, 127, 1, 35));
        MAMRibbonController.EmissionCount = emissionCount;
    }

    void UpdateRibbonNoiseFrequency(int midiValue)
    {
        ParticleSystem particleSystem = MAMRibbon.GetComponent<ParticleSystem>();
        var noiseModule = particleSystem.noise;
        noiseModule.frequency = ((float)midiValue).Map(0, 127, 0.05f, 1f);
    }

    void UpdateRibbonWidth(int midiValue)
    {
        ParticleSystem particleSystem = MAMRibbon.GetComponent<ParticleSystem>();
        var trailsModule = particleSystem.trails;
        var curve = trailsModule.widthOverTrail.curve;
        var key = curve.keys[1];
        key.value = ((float)midiValue).Map(0, 127, 0.5f, 5f);
        curve.MoveKey(1, key);
        var widthOverTrail = trailsModule.widthOverTrail;
        widthOverTrail.curve = curve;
        trailsModule.widthOverTrail = widthOverTrail;

    }

    void UpdateManualBlackoutQuad(int midiValue)
    {
        BlackoutAlpha.y = ((float)midiValue).Map(0, 127, 0f, 1f);
    }

    // Update is called once per frame
    void Update()
    {
        if (Mathf.Abs(BlackoutAlpha.x - BlackoutAlpha.y) > 0)
        {
            BlackoutAlpha.x = BlackoutAlpha.x + (BlackoutAlpha.y - BlackoutAlpha.x) / ManualBlackoutDamping;
            var material = ManualBlackoutQuad.GetComponent<MeshRenderer>().material;
            material.color = new Color(1, 1, 1, BlackoutAlpha.x);
            ManualBlackoutQuad.GetComponent<MeshRenderer>().material = material;
        }

    }
}
