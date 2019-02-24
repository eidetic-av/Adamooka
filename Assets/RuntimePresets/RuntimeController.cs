using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;
using UnityEngine;

namespace Eidetic.Unity.Runtime
{

    [System.Serializable]
    public abstract class RuntimeController : MonoBehaviour
    {
        public void Awake()
        {
            PresetManager.Instantiate(this);
        }

        public List<RuntimeControllerParameter> Pack() => GetType().GetFields()
                .Select(f => new RuntimeControllerParameter()
                {
                    Name = f.Name,
                    Value = f.GetValue(this)
                }).ToList();

        public void Unpack(List<RuntimeControllerParameter> preset)
        {
            foreach (var field in GetType().GetFields())
            {
                field.SetValue(this, preset.Where(
                    parameter => parameter.Name.Equals(field.Name))
                        .Select(parameter => parameter.Value)
                        .First());
            }
        }
    }

    [System.Serializable]
    public struct RuntimeControllerParameter
    {
        public string Name;
        public object Value;
    }
}