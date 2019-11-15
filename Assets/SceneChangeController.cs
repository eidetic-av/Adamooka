using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Midi;
using System;

public class SceneChangeController : MonoBehaviour
{
    public static SceneChangeController Instance;

    public Scene SelectedScene = Scene.OneFiveNine;
    Scene CurrentScene = Scene.OneFiveNine;
    Scene LastScene = Scene.OneFiveNine;

    GameObject OneFiveNineParent;
    ParticleSystem RainParticleSystem;
    bool KinectPreview = false;

    void Start()
    {
        Instance = this;
        OneFiveNineParent = GameObject.Find("OneFiveNine");
        RainParticleSystem = GameObject.Find("RainParticleSystem").GetComponent<ParticleSystem>();
    }
    void OneFiveNine()
    {
        DisableKinect();

        if (LastScene == Scene.ComputerRainParticles)
        {
            RainController.Instance.DisableEmission = true;
            // reset particles to white (if changed to desmond pallette)
            var rainMain = RainParticleSystem.main;
            rainMain.startColor = Color.white;
            OneFiveNineParent.SetActive(true);
        }
        // set all one five nine parameters to initial states

        // remove the circle system
        var initialStopLength = NoiseCircleController.Instance.StopSystemLength;
        NoiseCircleController.Instance.StopSystemLength = 0.01f;
        NoiseCircleController.Instance.StopSystem = true;
        StartCoroutine(DelayedAction(() =>
        {
            NoiseCircleController.Instance.StopSystemLength = initialStopLength;
        }, 0.5f));

        // first one five nine cue
        MidiManager.Instance.RouteNoteOn(1, Pitch.A1);
    }

    void ComputerRainParticles()
    {
        MidiManager.Instance.RouteNoteOn(2, Pitch.B2);
    }

    void ComputerRainClones()
    {
        MidiManager.Instance.RouteNoteOn(2, Pitch.CSharp3);
    }

    void Desmond()
    {
        MidiManager.Instance.RouteNoteOn(3, Pitch.F2);
    }

    void Hyphen()
    {
        DisableKinect();
        MidiManager.Instance.RouteNoteOn(4, Pitch.C3);
    }

    void Umbeants()
    {
        DisableKinect();
        MidiManager.Instance.RouteNoteOn(5, Pitch.C1);
    }
    void Jordan()
    {
        MidiManager.Instance.RouteNoteOn(6, Pitch.C0);
    }
    void Tunnel()
    {
        DisableKinect();
        MidiManager.Instance.RouteNoteOn(7, Pitch.C1);
    }

    void Update()
    {
        ReadKeys();
        if (CurrentScene != SelectedScene)
        {
            ChangeScene(SelectedScene);
        }
    }

    void DisableKinect()
    {
    }

    void EnableKinect()
    {
    }

    void ReadKeys()
    {
        if (Input.anyKeyDown)
        {
            int sceneNumber;
            if (int.TryParse(Input.inputString, out sceneNumber))
            {
                sceneNumber -= 1;
                if (Enum.IsDefined(typeof(Scene), sceneNumber))
                {
                    KinectPreview = false;
                    DisableKinect();
                    SelectedScene = (Scene)sceneNumber;
                }
            }
            else if (Input.inputString == "k")
            {
                KinectPreview = !KinectPreview;
                if (KinectPreview) EnableKinect();
                else DisableKinect();
            }
            else if (Input.inputString == "f")
            {
                MidiManager.Instance.RouteNoteOn(Channel.Channel3, Pitch.C5);
            }
        }
    }

    public void ChangeScene(Scene newScene)
    {
        var scene = newScene.ToString();

        if (InformationMonitor.Instance.Active)
        {
            var uiText = GameObject.Find("SceneNameText").GetComponent<Text>();
            uiText.text = "Scene: " + scene;
        }

        LastScene = CurrentScene;
        CurrentScene = newScene;

        Invoke(scene, 0);
    }

    IEnumerator DelayedAction(Action action, float delay)
    {
        yield return new WaitForSeconds(delay);
        action.Invoke();
    }
    public enum Scene
    {
        OneFiveNine,
        ComputerRainParticles,
        ComputerRainClones,
        Desmond,
        Hyphen,
        Umbeants,
        Jordan,
        Tunnel
    }
}
