using RuntimeInspectorNamespace;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Eidetic.Unity.Utility;

public class RuntimeEditorInputControl : MonoBehaviour
{
    public static RuntimeInspector Inspector;
    public static bool Active;

    public bool ActiveOnAwake;
    public List<GameObject> FocusedObjects;

    void Awake()
    {
        Inspector = GameObject.Find("RuntimeInspector")
            .GetComponent<RuntimeInspector>();
        Inspector.gameObject.SetActive(Active = ActiveOnAwake);
    }

    void Start()
    {
        if (Inspector.InspectedObject == null)
            FocusedObjects.FirstOrDefault()?.FocusInRuntimeInspector();
    }

    void Update()
    {
        // Interaction only through the modifier
        if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl))
        {
            // Toggle inspector
            if (Input.GetKeyDown(KeyCode.I))
                Inspector.gameObject.SetActive(Active = !Active);

            // Set inspector focus to the index(+1) selected with alpha keys
            else if (Active && Input.anyKeyDown)
                FocusedObjects
                    .FirstOrDefault(o =>
                        Input.GetKeyDown((FocusedObjects.IndexOf(o) + 1).ToString()))?
                    .FocusInRuntimeInspector();
        }
    }
}
