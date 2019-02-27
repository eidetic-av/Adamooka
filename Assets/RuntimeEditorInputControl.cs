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
        public static RuntimeInspector Inspector;
        public static bool Active;

        public List<GameObject> FocusedObjects =>
            gameObject.GetComponentsInChildren<RuntimeController>().Select(rc => rc.gameObject).ToList();

        void Awake()
        {
            Inspector = GameObject.Find("RuntimeInspector")
                .GetComponent<RuntimeInspector>();
            // default inspector to not active
            Inspector.gameObject.SetActive(false);
            // and when it is activated, if there is no focused object, focus the first available
            Inspector.OnActivate += () =>
            {
                if (Inspector.InspectedObject == null)
                    FocusedObjects.FirstOrDefault()?.FocusInRuntimeInspector();
            };
        }

        void Update()
        {
            if (Input.GetKey(KeyCode.LeftShift))
            {
                // Toggle inspector
                if (Input.GetKeyDown(KeyCode.I)) {
                    Inspector.gameObject.SetActive(Active = !Active);
                }

                // Set inspector focus to the index(+1) selected with alpha keys
                else if (Active && Input.anyKeyDown)
                    FocusedObjects.FirstOrDefault(o =>
                            Input.GetKeyDown((FocusedObjects.IndexOf(o) + 1).ToString()))?
                        .FocusInRuntimeInspector();
            }
        }
    }
}