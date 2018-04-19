using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaterialInstancer : MonoBehaviour
{
    public bool CreateInstance = true;

    // Update is called once per frame
    void Update()
    {
        if (CreateInstance)
        {
            this.InstanceMaterial();
            CreateInstance = false;
        }
    }
}

public static class MaterialInstancerExtensionMethods
{
    public static void InstanceMaterial(this MonoBehaviour monoBehaviour)
    {
        var renderer = monoBehaviour.gameObject.GetComponent<Renderer>();
        var instancedMaterial = new Material(renderer.material);
        renderer.material = instancedMaterial;
    }
}
