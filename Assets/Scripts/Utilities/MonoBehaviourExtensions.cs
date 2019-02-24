using UnityEngine;

namespace Eidetic.Unity.Utility
{
    public static class UnityEngineExtensionMethods
    {
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
            RuntimeEditorInputControl.Inspector.Inspect(gameObject);
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
    }
}
