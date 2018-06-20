using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscRouter : MonoBehaviour
{

    public static OscRouter Instance;

    public string TestPropertyAddress = "";
    public float TestPropertyValue = 5.0f;
    public Boolean SetTestProperty = false;

    /// <summary>
    /// Dictionary containing the methods to invoke when attempting to set Component properties or fields.
    /// They key is the OSC Address as a string; the value is the setter method as an Action.
    /// </summary>
    public Dictionary<string, Action<object>> MemberSetters = new Dictionary<string, Action<object>>();

    void Start()
    {
        if (Instance != null)
            throw (new UnityException("Only use one OscRouter in a scene!"));
        Instance = this;
    }

    void Update()
    {
        if (SetTestProperty)
        {
            if (MemberSetters.ContainsKey(TestPropertyAddress))
            {
                MemberSetters[TestPropertyAddress].Invoke(TestPropertyValue);
            }
            SetTestProperty = false;
        }
    }
}
