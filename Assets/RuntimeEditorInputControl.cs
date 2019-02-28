using RuntimeInspectorNamespace;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Eidetic.Unity.Utility;

namespace Eidetic.Unity.Runtime
{
    public class RuntimeEditorInputControl : MonoBehaviour
    {
        public List<GameObject> RuntimeControlObjects =>
            gameObject.GetComponentsInChildren<RuntimeController>().Select(rc => rc.gameObject).ToList();

        void Awake()
        {
            RuntimeInspector.Active = false;
        }
        void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                if (Input.GetKeyDown(KeyCode.I))
                    RuntimeInspector.Active = !RuntimeInspector.Active;

                // Set inspector focus to the index(+1) selected with alpha keys
                RuntimeControlObjects.SingleOrDefault(o =>
                        Input.GetKeyDown((RuntimeControlObjects.IndexOf(o) + 1).ToString()))?
                    .FocusInRuntimeInspector();
            }
        }
    }
}