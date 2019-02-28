using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;
using RuntimeInspectorNamespace;
using Eidetic.Unity.Utility;

namespace Eidetic.Unity.Runtime
{

    [System.Serializable]
    public abstract class RuntimeController : MonoBehaviour
    {
        public bool FocusOnAwake;
        public void Awake()
        {
            PresetManager.Instantiate(this);
            
            if (FocusOnAwake)
                gameObject.FocusInRuntimeInspector();
        }

        public List<RuntimeControllerParameter> Pack() => GetType().GetProperties()
                .Where(property => property.CanWrite)
                .Select(property => new RuntimeControllerParameter()
                {
                    Name = property.Name,
                    Value = property.GetValue(this)
                }).ToList();

        public void Unpack(List<RuntimeControllerParameter> preset)
        {
            foreach (var property in GetType().GetProperties().Where(property => property.CanWrite))
            {
                property.SetValue(this, preset.Where(
                    parameter => parameter.Name.Equals(property.Name))
                    .Select(parameter => parameter.Value)
                    .First());
            }
        }
        public virtual void BeforeLoad() {}
        public virtual void AfterLoad() {}
    }

    [System.Serializable]
    public struct RuntimeControllerParameter
    {
        public string Name;
        public object Value;
    }
}