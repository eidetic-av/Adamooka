using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MethodTrigger : MonoBehaviour
{

    public bool Trigger = false;

    public UnityEvent Methods;

    // Update is called once per frame
    void Update()
    {
        if (Trigger)
        {
            if (Methods != null)
                Methods.Invoke();
            Trigger = false;
        }
    }
}
