using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Midi;
using System;
using Eidetic.Andamooka;

public class PaintController : MonoBehaviour
{
    public static PaintController Instance;

    public ParticleSystem ParticleSystem;

    public Vector3 ShapeScale = Vector3.one;
    
    public Vector3 NoiseIntensity = Vector3.one;
    public float NoiseFrequency = 0.1f;

    public TrackingType AirsticksTracking = TrackingType.Off;
    public Vector3 TrackingScale = Vector3.one;
    public Vector3 TrackingOffset = Vector3.zero;

    void Start()
    {
        Instance = this;
    }

    void Update()
    {
        var shapeModule = ParticleSystem.shape;
        shapeModule.scale = ShapeScale;

        var noiseModule = ParticleSystem.noise;
        noiseModule.frequency = NoiseFrequency;
        noiseModule.strengthXMultiplier = NoiseIntensity.x;
        noiseModule.strengthYMultiplier = NoiseIntensity.y;
        noiseModule.strengthZMultiplier = NoiseIntensity.z;

        UpdateTracking();
    }

    void UpdateTracking() {
        switch (AirsticksTracking) {
            case TrackingType.Left: {
                var stickPosition = AirSticks.Left.Position;
                Vector3 position = Vector3.zero;
                position.x = (stickPosition.x * TrackingScale.x) + TrackingOffset.x;
                position.y = (stickPosition.y * TrackingScale.y) + TrackingOffset.y;
                position.z = (stickPosition.z * TrackingScale.z) + TrackingOffset.z;
                ParticleSystem.transform.SetPositionAndRotation(position, Quaternion.Euler(Vector3.zero));
                break;
            }
        }
    }

    public enum TrackingType {
        Left, Right, Both, Off
    }
}
