using RuntimeInspectorNamespace;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
		Inspector.gameObject.SetActive(ActiveOnAwake);
    }

    void Update()
    {
        // Toggle inspector with 'I' key.
        if (Input.GetKeyDown(KeyCode.I))
            Inspector.gameObject.SetActive(Active = !Active);
        else if (Active)
        {
            // Set inspector focus to the index(+1) selected with alpha keys
            FocusedObjects.FirstOrDefault(o =>
                Input.GetKeyDown((FocusedObjects.IndexOf(o) + 1).ToString()))?
                .Focus();
        }
    }
}

public static class RuntimeEditorExtensionMethods
{
    public static void Focus(this GameObject gameObject)
    {
        RuntimeEditorInputControl.Inspector.Inspect(gameObject);
    }
}
