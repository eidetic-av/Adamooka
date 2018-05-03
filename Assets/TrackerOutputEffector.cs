using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Eidetic.Unity.Utility;

public class TrackerOutputEffector : MonoBehaviour
{

    public bool AnimateAlpha = true;

    public bool NoteOnCloning = false;

    public bool LogLeftNoteEvents = false;
    public bool LogRightNoteEvents = false;

    [Range(0.005f, 0.8f)]
    public float CloneDistance = 0.5f;
    public float CloneThreshold = 0.05f;

    public List<Color> CloneColors;

    bool leftOn = false;
    bool LeftOn
    {
        get
        {
            return leftOn;
        }
        set
        {
            leftOn = value;
            if (LogLeftNoteEvents)
            {
                if (leftOn)
                {
                    Debug.Log("Left: On");
                }
                else
                {
                    Debug.Log("Left: Off");
                }
            }
        }
    }
    bool rightOn = false;
    bool RightOn
    {
        get
        {
            return rightOn;
        }
        set
        {
            rightOn = value;
            if (LogRightNoteEvents)
            {
                if (rightOn)
                {
                    Debug.Log("Right: On");
                }
                else
                {
                    Debug.Log("Right: Off");
                }
            }
        }
    }

    Vector3 LeftNoteOnPosition;
    Vector3 RightNoteOnPosition;

    List<GameObject> LeftHandClones = new List<GameObject>();
    List<GameObject> RightHandClones = new List<GameObject>();

    bool OccludeBase = true;

    Renderer Renderer;
    float Alpha, NewAlpha = 1f;
    float AlphaAnimationDamp = 5f;

    void Start()
    {
        // Instance the material so it is independently editable
        gameObject.InstanceMaterial();
        Renderer = gameObject.GetComponent<Renderer>();

        // Set AirSticks actions
        AirSticks.Left.NoteOn += LeftNoteOn;
        AirSticks.Left.NoteOff += LeftNoteOff;
        AirSticks.Right.NoteOn += RightNoteOn;
        AirSticks.Right.NoteOff += RightNoteOff;
    }

    // Update is called once per frame
    void Update()
    {
        if (RightOn)
        {
            var distanceFromOrigin = AirSticks.Right.Position - RightNoteOnPosition;
            if (distanceFromOrigin.x <= -(CloneThreshold * (RightHandClones.Count + 1)))
            {
                InstantiateClone(AirSticks.Hand.Right);
            }
        }
        if (LeftOn)
        {
            var distanceFromOrigin = AirSticks.Left.Position - LeftNoteOnPosition;
            if (distanceFromOrigin.x >= (CloneThreshold * (LeftHandClones.Count + 1)))
            {
                InstantiateClone(AirSticks.Hand.Left);
            }
        }

        if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            InstantiateClone(AirSticks.Hand.Right);
        }
        else if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            InstantiateClone(AirSticks.Hand.Left);
        }
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            LeftNoteOff();
            RightNoteOff();
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            OccludeBase = !OccludeBase;
            Color startColor = gameObject.GetComponent<Renderer>().material.GetColor("_TintColor");
            float h, s, v;
            Color.RGBToHSV(startColor, out h, out s, out v);
            if (OccludeBase)
            {
                Color occludeColor = Color.HSVToRGB(h, s, 0);
                gameObject.GetComponent<Renderer>().material.SetColor("_TintColor", occludeColor);
            }
            else
            {
                Color normalColor = Color.HSVToRGB(h, s, 1);
                gameObject.GetComponent<Renderer>().material.SetColor("_TintColor", normalColor);
            }
        }
    }

    void LeftNoteOn()
    {
        if (!NoteOnCloning) return;
        LeftOn = true;
        LeftNoteOnPosition = AirSticks.Left.Position;
    }

    void RightNoteOn()
    {
        if (!NoteOnCloning) return;
        RightOn = true;
        RightNoteOnPosition = AirSticks.Right.Position;
    }

    void LeftNoteOff()
    {
        LeftOn = false;
        HideClones(AirSticks.Hand.Left);
    }

    void RightNoteOff()
    {
        RightOn = false;
        HideClones(AirSticks.Hand.Right);
    }

    void InstantiateClone(AirSticks.Hand hand)
    {
        List<GameObject> clones;
        if (hand == AirSticks.Hand.Left)
            clones = LeftHandClones;
        else
            clones = RightHandClones;

        // if the clone doesn't exist, create it
        // if it does exist, set it active and add it to the visible clones list
        var cloneName = gameObject.name + hand.ToString() + "Clone (" + clones.Count + ")";
        GameObject clone = gameObject.FindChild(cloneName);
        if (!clone)
        {
            clone = Instantiate(gameObject);
        }
        else
        {
            clone.SetActive(true);
        }
        clones.Add(clone);

        // don't clone the children
        for (int i = 0; i < clone.transform.childCount; i++)
        {
            Destroy(clone.transform.GetChild(i).gameObject);
        }

        // set it up
        clone.name = cloneName;
        clone.transform.parent = gameObject.transform;

        // instance the material for editing
        clone.InstanceMaterial();
        var renderer = clone.GetComponent<Renderer>();
        if (CloneColors.Count != 0)
        {
            int colorIndex = clones.Count % CloneColors.Count;
            var cloneColor = CloneColors[colorIndex];
            renderer.material.SetColor("_TintColor", cloneColor);
        }

        // change the renderQueue order so that the newest clones are on the bottom
        var parentRenderer = gameObject.GetComponent<Renderer>();
        var renderQueuePosition = parentRenderer.material.renderQueue - clones.Count;
        renderer.material.renderQueue = renderQueuePosition;

        // make sure to remove this script from the clone so it's not recursive
        Destroy(clone.GetComponent<TrackerOutputEffector>());

        // set the x axis offset based on what number clone it is
        var xOffset = CloneDistance * clones.Count;
        if (hand == AirSticks.Hand.Right) xOffset = -xOffset;

        // an a slight z offset to ensure the new clones are rendered behind
        // (just using render queue isn't working)
        var zOffset = 0.01f * clones.Count;

        clone.transform.localPosition = new Vector3(xOffset, 0, zOffset);

        clone.transform.localRotation = Quaternion.Euler(Vector3.zero);

    }

    void HideClones(AirSticks.Hand hand)
    {
        List<GameObject> clones;
        if (hand == AirSticks.Hand.Left)
            clones = LeftHandClones;
        else
            clones = RightHandClones;

        for (int i = 0; i < clones.Count; i++)
        {
            clones[i].SetActive(false);
        }

        clones.Clear();
    }

    void DestroyClones(AirSticks.Hand hand)
    {
        List<GameObject> clones;
        if (hand == AirSticks.Hand.Left)
            clones = LeftHandClones;
        else
            clones = RightHandClones;

        for (int i = 0; i < clones.Count; i++)
        {
            Destroy(clones[i]);
        }

        clones.Clear();
    }
}
