using System;
using UnityEngine;

namespace Eidetic.Unity.Utility
{
    /// <summary>
    /// Utility methods for Unity relating to Materials.
    /// </summary>
    public static class Material
    {
        /// <summary>
        /// Duplicate the currently applied Material as an instance to allow for editing at runtime which won't affect every other instance of this material.
        /// </summary>
        /// <param name="gameObject">The GameObject whose material will be instanced. This must have a Renderer attached.</param>
        public static void InstanceMaterial(this GameObject gameObject)
        {
            var renderer = gameObject.GetComponent<Renderer>();
            if (renderer)
            {
                var instancedMaterial = new UnityEngine.Material(renderer.material);
                renderer.material = instancedMaterial;
            }
            else
            {
                throw (new ComponentNotFoundException(typeof(Renderer), gameObject));
            }
        }

        /// <summary>
        /// Exception to throw when a component isn't found on a GameObject
        /// </summary>
        public class ComponentNotFoundException : UnityException
        {
            /// <summary>
            /// Construct a new ComponentNotFoundException with an appropriate debug message.
            /// </summary>
            /// <param name="componentType">The type of component not found on the GameObject</param>
            /// <param name="gameObject">The GameObject where the component was missing</param>
            /// <param name="extraMessage">An extra message to log</param>
            public ComponentNotFoundException(Type componentType, GameObject gameObject, string extraMessage = null)
            {
                var logString = "A Component of type \'" + componentType.Name + "\' was not found on the GameObject \'" + gameObject.name + "\'";
                if (extraMessage != null) logString += "\n" + extraMessage;
                Debug.Log(logString);
            }
        }
    }
}