using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    void Update()
    {
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
