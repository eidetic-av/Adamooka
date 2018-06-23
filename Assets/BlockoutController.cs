using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using UnityEngine;

public class BlockoutController : MonoBehaviour
{

    [Range(0, 1)]
    public float Alpha = 0f;
    private float alpha = 0f;

    public bool FadingIn { get; private set; } = false;
    public bool FadingOut { get; private set; } = false;

    Stopwatch TransitionTimer = new Stopwatch();
    private int TransitionMilliseconds;

    public bool Full { get; private set; } = false;
    public bool Hidden { get; private set; } = true;

    // Use this for initialization
    void Start()
    {
        var loadedMaterial = gameObject.GetComponent<Renderer>().material;
        gameObject.GetComponent<Renderer>().material = Instantiate(loadedMaterial);
    }

    // Update is called once per frame
    void Update()
    {
        if (FadingIn)
        {
            Alpha = (float)TransitionTimer.ElapsedMilliseconds / TransitionMilliseconds;
            if (Alpha >= 1)
            {
                TransitionTimer.Stop();
                FadingIn = false;
                Full = true;
                Hidden = false;
            }
            gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 1, 1, Alpha));
        }
        else if (FadingOut)
        {
            Alpha = 1 - ((float)TransitionTimer.ElapsedMilliseconds / TransitionMilliseconds);
            if (Alpha <= 0)
            {
                TransitionTimer.Stop();
                FadingOut = false;
                Hidden = true;
                Full = false;
            }
            gameObject.GetComponent<Renderer>().material.SetColor("_Color", new Color(1, 1, 1, Alpha));
        }
    }

    public void FadeIn(int milliseconds)
    {
        TransitionTimer.Restart();
        TransitionMilliseconds = milliseconds;
        FadingIn = true;
        FadingOut = false;
    }

    public void FadeOut(int milliseconds)
    {
        TransitionTimer.Restart();
        TransitionMilliseconds = milliseconds;
        FadingOut = true;
        FadingIn = false;
    }
}
