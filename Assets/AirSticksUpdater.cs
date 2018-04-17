using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AirSticksUpdater : MonoBehaviour
{

    public GameObject Left;
    public GameObject Right;

    // Use this for initialization
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        Left.transform.localPosition = AirSticks.Left.Position;
        Left.transform.localRotation = Quaternion.Euler(AirSticks.Left.EulerAngles * 90);

        Right.transform.localPosition = AirSticks.Right.Position;
        Right.transform.localRotation = Quaternion.Euler(AirSticks.Right.EulerAngles * 90);
    }
}
