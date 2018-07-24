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

    void Start()
    {
        Instance = this;
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
            ActivateClones = false;
        } else if (CycleCloneColours)
        {
            if (TrackerOutputEffector != null)
            {
                TrackerOutputEffector.CycleCloneColours();
            }
            CycleCloneColours = false;
        }
    }
}
