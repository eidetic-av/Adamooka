using System;
using System.Collections;
using UnityEngine;
using Eidetic.Unity.Runtime;
using RuntimeInspectorNamespace;

namespace Eidetic.Unity.Utility
{
    public static class UnityEngineExtensionMethods
    {
        // GameObject extension methods
        public static GameObject FindChild(this GameObject parent, string name)
        {
            Transform[] trs = parent.GetComponentsInChildren<Transform>(true);
            foreach (Transform t in trs)
            {
                if (t.name == name)
                {
                    return t.gameObject;
                }
            }
            return null;
        }
        public static void FocusInRuntimeInspector(this GameObject gameObject)
        {
            RuntimeInspector.Active = true;
            RuntimeInspector.Instance.Inspect(gameObject);
        }

        // Particle System extension methods
        public static void Restart(this ParticleSystem particleSystem)
        {
            particleSystem.Clear();
            particleSystem.Play();
        }
        public static void SetSimulationSpeed(this ParticleSystem particleSystem, float speed)
        {
            var mainModule = particleSystem.main;
            mainModule.simulationSpeed = speed;
        }

        public static void SetNoiseStrength(this ParticleSystem particleSystem, float strength)
        {
            var noiseModule = particleSystem.noise;
            noiseModule.strength = strength;
        }

        public static UnityEngine.Material GetTrailMaterial(this ParticleSystem particleSystem)
        {
            var rendererModule = particleSystem.GetComponent<ParticleSystemRenderer>();
            return rendererModule.trailMaterial;
        }

        //Vector extensions
        public static Vector3 Multiply(this Vector3 a, Vector3 b) =>
            new Vector3(a.x * b.x, a.y * b.y, a.z * b.z);
        public static Vector3 Add(this Vector3 a, Vector3 b) =>
            new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);

    }
}
