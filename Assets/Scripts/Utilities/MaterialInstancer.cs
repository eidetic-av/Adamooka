using Eidetic.Unity.Utility;
using UnityEngine;

public class MaterialInstancer : MonoBehaviour
{
    public bool CreateInstance = true;

    // Update is called once per frame
    void Update()
    {
        if (CreateInstance)
        {
            gameObject.InstanceMaterial();
            CreateInstance = false;
        }
    }
}

