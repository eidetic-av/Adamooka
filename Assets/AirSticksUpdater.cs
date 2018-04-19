using UnityEngine;
using System.Collections.Generic;
using Midi;
using System.Linq;

public class AirSticksUpdater : MonoBehaviour
{

    public GameObject Left;
    public GameObject Right;
    public GameObject User;
    private SirenDisc LeftDiscs;
    private SirenDisc RightDiscs;
    private SirenDisc UserDiscs;

    public Vector3 UserStartPosition = Vector3.zero;

    public bool DampTransforms = true;
    [Range(1.1f, 30f)]
    public float DampingRate = 5f;
    private Vector3 DampedLeftPosition = Vector3.zero;
    private Vector3 DampedLeftRotation = Vector3.zero;
    private Vector3 DampedRightPosition = Vector3.zero;
    private Vector3 DampedRightRotation = Vector3.zero;
    private Vector3 DampedUserPosition = Vector3.zero;

    [Range(0, 300)]
    public int DiscCount = 15;
    private int LastDiscCount = 0;
    [Range(2, 150)]
    public int ColorSpread = 15;
    private int LastColorSpread = 0;

    [Range(0.3f, 15f)]
    public float BaseSize = 2f;
    private float LastBaseSize = 0;
    [Range(0.3f, 15f)]
    public float Spacing = 2f;
    private float LastSpacing = 0;

    public Material Material;
    public Material BaseMaterial;

    public enum PatternOrigin
    {
        LeftHand, RightHand, User
    }

    public PatternOrigin Origin = PatternOrigin.LeftHand;

    public Material Blockout;
    public List<Color> Colors;

    private InputDevice LoopMidi;

    // Use this for initialization
    void Start()
    {
        foreach (var device in InputDevice.InstalledDevices)
        {
            if (device.Name.Equals("LoopBe Internal MIDI"))
            {
                LoopMidi = device;
                LoopMidi.Open();
                LoopMidi.StartReceiving(null);
                LoopMidi.NoteOn += RouteNoteOns;
                Debug.Log("Connected to LoopBe Device");
                break;
            }
        }

        SetPatternOrigin(Origin);
        if (Left != null)
        {
            LeftDiscs = Left.AddComponent<SirenDisc>().WithMaterial(Material);
            LeftDiscs.Blockout = Blockout;
        }
        if (Right != null)
        {
            RightDiscs = Right.AddComponent<SirenDisc>().WithMaterial(Material);
            RightDiscs.Blockout = Blockout;
        }
        if (User != null)
        {
            UserDiscs = User.AddComponent<SirenDisc>().WithMaterial(Material);
            UserDiscs.Blockout = Blockout;
        }
    }

    void OnDestroy()
    {
        if (LoopMidi != null)
        {
            LoopMidi.StopReceiving();
            LoopMidi.Close();
        }
    }

    void RouteNoteOns(NoteOnMessage noteOnMessage)
    {
        if (noteOnMessage.Channel.Number() == 1)
        {
            switch (noteOnMessage.Pitch.NoteNumber())
            {
                case 42:
                    UnityMainThreadDispatcher.Instance().Enqueue(ReverseColors);
                    break;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        CheckKeyPresses();
        SetPatternOrigin(Origin);
        if (DampTransforms)
        {
            if (Left != null)
            {
                if (Mathf.Abs(DampedLeftPosition.x - AirSticks.Left.Position.x) > 0)
                {
                    DampedLeftPosition.x = DampedLeftPosition.x + (AirSticks.Left.Position.x - DampedLeftPosition.x) / DampingRate;
                }
                if (Mathf.Abs(DampedLeftPosition.y - AirSticks.Left.Position.y) > 0)
                {
                    DampedLeftPosition.y = DampedLeftPosition.y + (AirSticks.Left.Position.y - DampedLeftPosition.y) / DampingRate;
                }
                if (Mathf.Abs(DampedLeftPosition.z - AirSticks.Left.Position.z) > 0)
                {
                    DampedLeftPosition.z = DampedLeftPosition.z + (AirSticks.Left.Position.z - DampedLeftPosition.z) / DampingRate;
                }
                Left.transform.localPosition = DampedLeftPosition;

                if (Mathf.Abs(DampedLeftRotation.x - AirSticks.Left.EulerAngles.x) > 0)
                {
                    DampedLeftRotation.x = DampedLeftRotation.x + (AirSticks.Left.EulerAngles.x - DampedLeftRotation.x) / DampingRate;
                }
                if (Mathf.Abs(DampedLeftRotation.y - AirSticks.Left.Position.y) > 0)
                {
                    DampedLeftRotation.y = DampedLeftRotation.y + (AirSticks.Left.EulerAngles.y - DampedLeftRotation.y) / DampingRate;
                }
                if (Mathf.Abs(DampedLeftRotation.z - AirSticks.Left.Position.z) > 0)
                {
                    DampedLeftRotation.z = DampedLeftRotation.z + (AirSticks.Left.EulerAngles.z - DampedLeftRotation.z) / DampingRate;
                }
                Left.transform.localRotation = Quaternion.Euler(DampedLeftRotation * 90);
            }

            if (Right != null)
            {
                if (Mathf.Abs(DampedRightPosition.x - AirSticks.Right.Position.x) > 0)
                {
                    DampedRightPosition.x = DampedRightPosition.x + (AirSticks.Right.Position.x - DampedRightPosition.x) / DampingRate;
                }
                if (Mathf.Abs(DampedRightPosition.y - AirSticks.Right.Position.y) > 0)
                {
                    DampedRightPosition.y = DampedRightPosition.y + (AirSticks.Right.Position.y - DampedRightPosition.y) / DampingRate;
                }
                if (Mathf.Abs(DampedRightPosition.z - AirSticks.Right.Position.z) > 0)
                {
                    DampedRightPosition.z = DampedRightPosition.z + (AirSticks.Right.Position.z - DampedRightPosition.z) / DampingRate;
                }
                Right.transform.localPosition = DampedRightPosition;

                if (Mathf.Abs(DampedRightRotation.x - AirSticks.Left.EulerAngles.x) > 0)
                {
                    DampedRightRotation.x = DampedRightRotation.x + (AirSticks.Left.EulerAngles.x - DampedRightRotation.x) / DampingRate;
                }
                if (Mathf.Abs(DampedRightRotation.y - AirSticks.Left.Position.y) > 0)
                {
                    DampedRightRotation.y = DampedRightRotation.y + (AirSticks.Left.EulerAngles.y - DampedRightRotation.y) / DampingRate;
                }
                if (Mathf.Abs(DampedRightRotation.z - AirSticks.Left.Position.z) > 0)
                {
                    DampedRightRotation.z = DampedRightRotation.z + (AirSticks.Left.EulerAngles.z - DampedRightRotation.z) / DampingRate;
                }
                Right.transform.localRotation = Quaternion.Euler(DampedRightRotation * 90);

            }

            //User.transform.localPosition = KinectManager.Instance.GetJointPosition(KinectManager.Instance.GetUserIdByIndex(0), (int)KinectInterop.JointType.SpineMid) * 10;
        }
        else
        {
            if (Left != null)
            {
                Left.transform.localPosition = AirSticks.Left.Position;
                Left.transform.localRotation = Quaternion.Euler(AirSticks.Left.EulerAngles * 90);
            }

            if (Right != null)
            {
                Right.transform.localPosition = AirSticks.Right.Position;
                Right.transform.localRotation = Quaternion.Euler(AirSticks.Right.EulerAngles * 90);
            }

            //User.transform.localPosition = KinectManager.Instance.GetJointPosition(KinectManager.Instance.GetUserIdByIndex(0), (int)KinectInterop.JointType.SpineMid) * 10;
        }
        UpdateDiscs();
    }

    void CheckKeyPresses()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            AdvancePatternOrigin();
        }
        else if (Input.GetKeyDown(KeyCode.W))
        {
            ReverseColors();
        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            AdvanceColors();
        }
    }

    public void ReverseColors()
    {
        if (LeftDiscs != null)
        {
            LeftDiscs.ReverseColors();
        }
        if (RightDiscs != null)
            RightDiscs.ReverseColors();
        if (UserDiscs != null)
            UserDiscs.ReverseColors();
    }

    public void AdvanceColors()
    {
        if (LeftDiscs != null)
        {
            LeftDiscs.AdvanceColors();
        }
        if (RightDiscs != null)
            RightDiscs.AdvanceColors();
        if (UserDiscs != null)
            UserDiscs.AdvanceColors();
    }

    public void AdvancePatternOrigin()
    {
        switch (Origin)
        {
            case PatternOrigin.LeftHand:
                Origin = (PatternOrigin.RightHand);
                break;
            case PatternOrigin.RightHand:
                Origin = (PatternOrigin.LeftHand);
                break;
            case PatternOrigin.User:
                Origin = (PatternOrigin.LeftHand);
                break;
        }
    }

    public void SetPatternOrigin(PatternOrigin origin)
    {
        switch (origin)
        {
            case PatternOrigin.LeftHand:
                if (Left != null)
                    Left.SetActive(true);
                //if (Right != null)
                    //Right.SetActive(false);
                //if (User != null)
                    //User.SetActive(false);
                break;
            case PatternOrigin.RightHand:
                //if (Left != null)
                    //Left.SetActive(false);
                if (Right != null)
                    Right.SetActive(true);
                //if (User != null)
                    //User.SetActive(false);
                break;
            case PatternOrigin.User:
                //if (Left != null)
                    //Left.SetActive(false);
                //if (Right != null)
                    //Right.SetActive(false);
                if (User != null)
                    User.SetActive(true);
                break;
        }
    }

    void UpdateDiscs()
    {
        if (DiscCount != LastDiscCount || ColorSpread != LastColorSpread || BaseSize != LastBaseSize || Spacing != LastSpacing)
        {
            if (Left != null)
            {
                LeftDiscs.Initialise(Left, DiscCount, ColorSpread, BaseSize, Spacing, Vector3.zero, Colors, Material, BaseMaterial);
            }
            if (Right != null)
            {
                RightDiscs.Initialise(Right, DiscCount, ColorSpread, BaseSize, Spacing, Vector3.zero, Colors, Material, BaseMaterial);
            }
            if (User != null)
            {
                UserDiscs.Initialise(User, DiscCount, ColorSpread, BaseSize, Spacing, UserStartPosition, Colors, Material);
            }

            LastDiscCount = DiscCount;
            LastColorSpread = ColorSpread;
            LastBaseSize = BaseSize;
            LastSpacing = Spacing;
        }
    }

}
