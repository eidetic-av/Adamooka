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
    ParticleSystem InnerRing;
    ParticleSystem Membrane;
    void Start()
    {
        InnerRing = GameObject.Find("NoiseCircleSystem")
            .GetComponent<ParticleSystem>();
        Membrane = GameObject.Find("MembraneSystem")
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
