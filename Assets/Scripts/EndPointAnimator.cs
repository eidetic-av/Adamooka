using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EndPointAnimator : MonoBehaviour
{


    public bool RunAutomatically = false;
    [Range(0, 5f)]
    public float Rate = 0;
    public float StartingPosition = -3.5f;
    public float EndingPosition = 0.67f;

    public float CurrentPosition { get; set; }

    // Use this for initialization
    void Start()
    {
        // Start in the middle
        CurrentPosition = (StartingPosition + EndingPosition) / 2;
    }

    // Update is called once per frame
    void Update()
    {
        if (RunAutomatically)
        {
            CurrentPosition += Rate;
            if (CurrentPosition > EndingPosition)
            {
                CurrentPosition = StartingPosition;
            }
            var position = gameObject.transform.localPosition;
            position.x = CurrentPosition;
            gameObject.transform.localPosition = position;
        }
    }
}
