using System;
using System.Text.RegularExpressions;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Eidetic.Unity.Utility;

public class TrackerSceneController : MonoBehaviour
{

    public static TrackerSceneController Instance;

    public bool DisableKinectUpdate = false;
    public bool EnableKinectUpdate = false;
    public bool DisableUserRender = false;
    public bool EnableUserRender = false;
    public bool ActivateClones = false;
    public TrackerOutputEffector TrackerOutputEffector;
    public bool CycleCloneColours = false;

    public bool AirSticksNoteOnCyclesColours = true;

    public bool ActivateCloneIntroState = false;
    public bool ActivateRainOutroState = false;
    public bool SillhouetteOff = false;
    public bool RemoveAllClones = false;

    public int CloneCount = 1;
    public bool UpdateCloneAmount = false;

    void Start()
    {
        Instance = this;
        // AirSticks.Left.NoteOn += NoteOnCycleColours;
        AirSticks.Right.NoteOn += NoteOnCycleColours;
    }

    void NoteOnCycleColours() {
        if (AirSticksNoteOnCyclesColours)
            CycleCloneColours = true;
    }

    void CycleColours() {
        TrackerOutputEffector.CycleCloneColours();
        CycleCloneColours = false;
    }

    public void SetCloneState(int state) {
        switch(state) {
            case 0:
                SetCloneAmount(2);
                TrackerOutputEffector.CloneColourPosition = 1;
                TrackerOutputEffector.RefreshCloneColours();
                TrackerOutputEffector.RefreshClones();
                AirSticksNoteOnCyclesColours = false;
                TrackerOutputEffector.CloneDistance = 0.08f;
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = false;
                break;
            case 1:
                SetCloneAmount(4);
                TrackerOutputEffector.CloneDistance = 0.95f;
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = true;
                break;
            case 2:
                SetCloneAmount(6);
                TrackerOutputEffector.CloneDistance = 0.95f;
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = true;
                break;
            case 3:
                SetCloneAmount(8);
                TrackerOutputEffector.CloneDistance = 0.95f;
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = true;
                break;
            case 4:
                SetCloneAmount(10);
                TrackerOutputEffector.CloneDistance = 0.95f;
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = true;
                break;
            case 5:
                SetCloneAmount(12);
                TrackerOutputEffector.CloneDistance = 0.95f;
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = true;
                break;
            case 6:
                SetCloneAmount(14);
                TrackerOutputEffector.CloneDistance = 0.95f;
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = true;
                break;
            case 7:
                SetCloneAmount(16);
                TrackerOutputEffector.CloneDistance = 0.95f;
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = true;
                break;
            case 8:
                SetCloneAmount(18);
                TrackerOutputEffector.CloneDistance = 0.95f;
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = true;
                break;
            case 9:
                SetCloneAmount(20);
                TrackerOutputEffector.CloneDistance = 0.95f;
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = true;
                break;
            case 10:
                SetCloneAmount(22);
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = true;
                TrackerOutputEffector.CloneDistance = 1.5f;
                break;
            case 11:
                SetCloneAmount(24);
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = true;
                TrackerOutputEffector.CloneDistance = 1.5f;
                break;
            case 12:
                SetCloneAmount(24);
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = true;
                TrackerOutputEffector.CloneDistance = 1.5f;
                break;
            case 13:
                SetCloneAmount(24);
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = true;
                TrackerOutputEffector.CloneDistance = 1.5f;
                break;
            case 14:
                SetCloneAmount(24);
                AirSticksNoteOnCyclesColours = true;
                TrackerOutputEffector.ControlCloneDistanceWithAirSticks = true;
                TrackerOutputEffector.CloneDistance = 1.5f;
                break;
        }
    }

    Component[] CloneTransforms = null;

    void SetCloneAmount(int numberOfClones) {
        if (CloneTransforms == null)  CloneTransforms = TrackerOutputEffector.gameObject.GetComponentsInChildren(typeof(Transform));
        for (int i = 1; i < CloneTransforms.Length; i+=2) {
            if (i + 2 >= CloneTransforms.Length) break;
            var cloneObjectLeft = CloneTransforms[i].gameObject;
            var cloneObjectRight = CloneTransforms[i+1].gameObject;
            if ((i + 1) > numberOfClones) {
                cloneObjectLeft.SetActive(false);
                cloneObjectRight.SetActive(false);
            } else {
                cloneObjectLeft.SetActive(true);
                cloneObjectRight.SetActive(true);
            }
        }
    }

    void Update()
    {
        if (UpdateCloneAmount) {
            SetCloneAmount(CloneCount);
            UpdateCloneAmount = false;
        }
        if (DisableKinectUpdate)
        {
            UserMeshVisualizer.Instance.BlockKinectUpdate = true;
            UserMeshVisualizer.Instance.DisableMeshUpdate = true;
            DisableKinectUpdate = false;
        }
        else if (EnableKinectUpdate)
        {
            UserMeshVisualizer.Instance.BlockKinectUpdate = false;
            UserMeshVisualizer.Instance.DisableMeshUpdate = false;
            EnableKinectUpdate = false;
        }
        else if (DisableUserRender)
        {
            GameObject.Find("UserMesh").GetComponent<MeshRenderer>().enabled = false;
            DisableUserRender = false;
        }
        else if (EnableUserRender)
        {
            GameObject.Find("UserMesh").GetComponent<MeshRenderer>().enabled = true;
            EnableUserRender = false;
        } else if (ActivateClones)
        {
            EnableKinectUpdate = true;
            EnableUserRender = true;
            ParticleSceneController.Instance.OneFiveNineOut = true;
            MidiManager.Instance.ShowClones();
            if (GameObject.Find("LightPlane") != null)
                GameObject.Find("LightPlane").SetActive(false);
            var baseOutputQuad = GameObject.Find("OutputQuad");
            var effector = baseOutputQuad.GetComponent<TrackerOutputEffector>();
            effector.UpdateParametersEveryFrame = true;
            ActivateClones = false;
        } else if (CycleCloneColours)
        {
            CycleColours();
        }

        if (ActivateRainOutroState) {
            var baseOutputQuad = GameObject.Find("OutputQuad");
            var effector = baseOutputQuad.GetComponent<TrackerOutputEffector>();
            effector.CloneColourPosition = 3;
            effector.RefreshCloneColours();
            AirSticksNoteOnCyclesColours = false;
        }
        if (SillhouetteOff) {
            var baseOutputQuad = GameObject.Find("OutputQuad");
            var renderer = baseOutputQuad.GetComponent<Renderer>();
            renderer.material.SetColor("_TintColor", new Color(0, 0, 0, 0));
            var effector = baseOutputQuad.GetComponent<TrackerOutputEffector>();
            effector.CloneDistance = 0.005f;
            ActivateRainOutroState = false;
            SillhouetteOff = false;
        }

        if (ActivateCloneIntroState) {
            var baseOutputQuad = GameObject.Find("OutputQuad");
            var effector = baseOutputQuad.GetComponent<TrackerOutputEffector>();
            effector.CloneColourPosition = 1;
            effector.RefreshCloneColours();
            AirSticksNoteOnCyclesColours = false;
            ActivateCloneIntroState = false;
        }

        if (RemoveAllClones) {
            var baseOutputQuad = GameObject.Find("OutputQuad");
            var children = baseOutputQuad.GetComponentsInChildren<Transform>();
            foreach(var child in children) {
                if (child.gameObject != baseOutputQuad)
                    Destroy(child.gameObject);
            }
            var effector = baseOutputQuad.GetComponent<TrackerOutputEffector>();
            effector.LeftHandClones.Clear();
            effector.RightHandClones.Clear();
            RemoveAllClones = false;
            effector.UpdateParametersEveryFrame = false;
        }
    }
}
