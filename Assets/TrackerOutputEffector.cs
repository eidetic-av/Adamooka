using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Eidetic.Unity.Utility;
using Utility;
using Eidetic;

public class TrackerOutputEffector : MonoBehaviour
{

    public bool AnimateAlpha = true;
    public bool EnableAirsticksAlphaControl = false;

    public bool NoteOnCloning = false;

    public bool LogLeftNoteEvents = false;
    public bool LogRightNoteEvents = false;

    public bool CloningActive = false;

    float cloneDistance = 0.5f;
    [Range(0.005f, 0.8f)]
    public float CloneDistance = 0.5f;
    public float CloneThreshold = 0.05f;

    public List<Color> CloneColors;
    int CloneColourPosition = 0;

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

    int CloneCountRight = 0;
    int CloneCountLeft = 0;

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

        AirSticks.Right.NoteOn += AlphaOn;
        AirSticks.Right.NoteOff += AlphaOff;
        gameObject.InstanceMaterial();

        // add 10 clones to the scene on either side and hide them
        for (int i = 0; i < 10; i++)
        {
            InstantiateClone(AirSticks.Hand.Left);
            InstantiateClone(AirSticks.Hand.Right);
        }
        HideClones(AirSticks.Hand.Left);
        HideClones(AirSticks.Hand.Right);
    }

    // Update is called once per frame
    void Update()
    {

        //if (RightOn)
        //{
        //    var distanceFromOrigin = AirSticks.Right.Position - RightNoteOnPosition;
        //    if (distanceFromOrigin.x <= -(CloneThreshold * (RightHandClones.Count + 1)))
        //    {
        //        InstantiateClone(AirSticks.Hand.Right);
        //    }
        //}
        //if (LeftOn)
        //{
        //    var distanceFromOrigin = AirSticks.Left.Position - LeftNoteOnPosition;
        //    if (distanceFromOrigin.x >= (CloneThreshold * (LeftHandClones.Count + 1)))
        //    {
        //        InstantiateClone(AirSticks.Hand.Left);
        //    }
        //}

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
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            // change colour array
            RefreshCloneColours();
            RefreshClones();
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

        if (CloningActive)
        {

            if (RightOn)
            {
                CloneDistance = AirSticks.Right.Position.z.Map(0, 1f, 0.5f, 0.8f);
                CloneCountRight = Mathf.CeilToInt(Mathf.Clamp(AirSticks.Right.Position.z.Map(0, Screen.width, 15f, 1f), 1, 8));
                HideClones(AirSticks.Hand.Right);
                for (int i = 0; i < CloneCountRight; i++)
                {
                    InstantiateClone(AirSticks.Hand.Right);
                }
                //CloneColourPosition = Mathf.FloorToInt(AirSticks.Right.Position.x.Map(-.5f, .5f, 0, 4));
            }

            if (LeftOn)
            {
                CloneDistance = AirSticks.Left.Position.z.Map(0, Screen.width, 0.5f, 0.8f);
                CloneCountLeft = Mathf.CeilToInt(Mathf.Clamp(AirSticks.Left.Position.z.Map(0, Screen.width, 15f, 1f), 1, 8));
                HideClones(AirSticks.Hand.Left);
                for (int i = 0; i < CloneCountLeft; i++)
                {
                    InstantiateClone(AirSticks.Hand.Left);
                }
                //CloneColourPosition = Mathf.FloorToInt(AirSticks.Left.Position.x.Map(-.5f, .5f, 0, 4));
            }

        }

        // refresh on clone change distance
        if (cloneDistance != CloneDistance)
        {
            RefreshClones();
        }

        // AIRSTICKS TINTING

        if (Mathf.Abs(Alpha - NewAlpha) > 0)
        {
            Alpha = Alpha + (NewAlpha - Alpha) / AlphaAnimationDamp;
        }

        if (EnableAirsticksAlphaControl)
        {
            Color color = Color.HSVToRGB(1f, 0f, 1f);
            color.a = Alpha;
            Debug.Log(Alpha);
            gameObject.GetComponent<Renderer>().material.SetColor("_TintColor", color);
        }
    }

    public void CycleCloneColours()
    {
        CloneColourPosition++;
        if (CloneColourPosition > 3) CloneColourPosition = 0;
        RefreshCloneColours();
        RefreshClones();
    }

    void RefreshCloneColours()
    {
        switch (CloneColourPosition)
        {
            case 0:
                CloneColors = new List<Color>()
                {
                    // pinks
                    new Color(1, 0.1882353f, 0.7098039f),
                    new Color(0.854902f, 0.172549f, 0.9098039f),
                    new Color(0.7019608f, 0.1411765f, 1),
                    new Color(0.454902f, 0.172549f, 0.9098039f),
                    new Color(0.2784314f, 0.1882353f, 1)
                };
                break;
            case 1:
                CloneColors = new List<Color>()
                {
                    // blues
                    Rgb(225, 230, 250),
                    Rgb(196, 215, 237),
                    Rgb(171, 200, 226),
                    Rgb(55, 93, 129),
                    Rgb(24, 49, 82)
                };
                break;
            case 2:
                CloneColors = new List<Color>()
                {
                    // reds
                    Rgb(240, 67, 58),
                    Rgb(201, 40, 62),
                    Rgb(130, 3, 51),
                    Rgb(84, 0, 50),
                    Rgb(46, 17, 45)
                };
                break;
            case 3:
                CloneColors = new List<Color>()
                {
                    // greens
                    Rgb(22, 120, 255),
                    Rgb(20, 165, 232),
                    Rgb(34, 244, 255),
                    Rgb(20, 232, 184),
                    Rgb(22, 255, 136)
                };
                break;
        }
    }

    Color Rgb(int r, int g, int b)
    {
        float rVal = ((float)r).Map(0f, 255f, 0f, 1f);
        float gVal = ((float)g).Map(0f, 255f, 0f, 1f);
        float bVal = ((float)b).Map(0f, 255f, 0f, 1f);
        return new Color(rVal, gVal, bVal);
    }

    void RefreshClones()
    {
        RefreshCloneColours();
        List<GameObject> ActiveLeftClones = new List<GameObject>();
        List<GameObject> ActiveRightClones = new List<GameObject>();
        for (int i = 0; i < 100; i++)
        {
            var cloneName = gameObject.name + AirSticks.Hand.Right.ToString() + "Clone (" + i + ")";
            GameObject cloneRight = gameObject.FindChild(cloneName);
            if (cloneRight != null)
            {
                if (cloneRight.activeInHierarchy)
                {
                    ActiveRightClones.Add(cloneRight);
                }
            }
            cloneName = gameObject.name + AirSticks.Hand.Left.ToString() + "Clone (" + i + ")";
            GameObject cloneLeft = gameObject.FindChild(cloneName);
            if (cloneLeft != null)
            {
                if (cloneLeft.activeInHierarchy)
                {
                    ActiveLeftClones.Add(cloneLeft);
                }
            }
            if (cloneLeft == null && cloneRight == null)
            {
                break;
            }
        }
        cloneDistance = CloneDistance;
        HideClones(AirSticks.Hand.Right);
        HideClones(AirSticks.Hand.Left);
        for (int i = 0; i < ActiveRightClones.Count; i++)
        {
            InstantiateClone(AirSticks.Hand.Right);
        }
        for (int i = 0; i < ActiveLeftClones.Count; i++)
        {
            InstantiateClone(AirSticks.Hand.Left);
        }
    }

    void AlphaOn()
    {
        Alpha = 1f;
        NewAlpha = 1f;
    }
    void AlphaOff()
    {
        Alpha = 0f;
        NewAlpha = 0f;
    }


    void LeftNoteOn()
    {
        LeftOn = true;
        //LeftNoteOnPosition = AirSticks.Left.Position;
    }

    void RightNoteOn()
    {
        RightOn = true;
        //RightNoteOnPosition = AirSticks.Right.Position;
    }

    void LeftNoteOff()
    {
        LeftOn = false;
    }

    void RightNoteOff()
    {
        RightOn = false;
    }

    public void InstantiateClone(AirSticks.Hand hand)
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
        var xOffset = cloneDistance * clones.Count;
        if (hand == AirSticks.Hand.Right) xOffset = -xOffset;

        // an a slight z offset to ensure the new clones are rendered behind
        // (just using render queue isn't working)
        var zOffset = 0.01f * clones.Count;

        clone.transform.localPosition = new Vector3(xOffset, 0, zOffset);

        clone.transform.localRotation = Quaternion.Euler(Vector3.zero);

    }

    public void HideClones(AirSticks.Hand hand)
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
