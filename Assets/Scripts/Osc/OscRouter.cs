using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OscRouter : MonoBehaviour {

    public static OscRouter Instance;

    public string TestPropertyAddress = "";
    public Boolean SetTestProperty = false;
    
    /// <summary>
    /// Dictionary containing the methods to invoke when attempting to set Component properties.
    /// They key is the OSC Address as a string; the value is the setter method as an Action.
    /// </summary>
    public Dictionary<string, Action> PropertySetters { get; set; } = new Dictionary<string, Action>();
    
	void Start () {
        if (Instance != null)
            throw (new UnityException("Only use one OscRouter in a scene!"));
        Instance = this;
	}
	
	void Update () {
		if (SetTestProperty)
        {

        }
	}
}
