using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using RuntimeInspectorNamespace;
using Eidetic.Andamooka;
using Eidetic.Unity.Runtime;
using Eidetic.Unity.Utility;

[System.Serializable]
public class RingController : RuntimeController
{

    //
    // Runtime control properties
    //
    public bool ControlWithAirSticks {get; set;} = true;

    //
    // Initialisation
    //
    ParticleSystem WhiteSystem;
    ParticleSystem ColourSystem;
    void Start()
    {
        WhiteSystem = GameObject.Find("WhiteSystem")
            .GetComponent<ParticleSystem>();
        ColourSystem = GameObject.Find("ColourSystem")
            .GetComponent<ParticleSystem>();
    }

    //
    // Runtime loop
    //
    void Update() 
    {
        if (ControlWithAirSticks)
        {
                
        }
    }

    //
    // Inner methods
    //
}
