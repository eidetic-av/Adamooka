using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MultiDisplay : MonoBehaviour
{


    void Start()
    {
		Display.displays[0].SetRenderingResolution(Display.displays[0].systemWidth,
													Display.displays[0].systemHeight);
        if (Display.displays.Length > 1)
        {
            Display.displays[1].Activate();
            Display.displays[1].SetRenderingResolution(Display.displays[1].systemWidth,
                                                       Display.displays[1].systemHeight);
        } else {
            // if there is only one display, move information monitor to overlay main image
            var uiCamera = GameObject.Find("InformationMonitor").GetComponentInChildren<Camera>();
            uiCamera.targetDisplay = 0;
        }
    }

}
