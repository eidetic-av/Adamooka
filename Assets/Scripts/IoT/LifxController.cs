using LifxNet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Midi;
using Eidetic.Utility;

public class LifxController : MonoBehaviour
{

    static LifxClient Client;
    static List<LightBulb> LightBulbs = new List<LightBulb>();

    static InputDevice LoopMidi;

    public bool CycleHue = false;

    static LightStateResponse LastOnLightState;

    // Use this for initialization
    void Start()
    {
        var task = LifxClient.CreateAsync();
        task.Wait();
        Client = task.Result;
        Client.DeviceDiscovered += Client_DeviceDiscovered;
        Client.DeviceLost += Client_DeviceLost;
        Client.StartDeviceDiscovery();

        Debug.Log("Lifx Devices Count on Start: " + Client.Devices.ToList().Count);

        // Print the label of each device
        Client.Devices.ToList().ForEach(device => Debug.Log(Client.GetDeviceLabelAsync(device)));

        foreach (var device in InputDevice.InstalledDevices)
        {
            Debug.Log(device.Name);
            if (device.Name.ToLower() == "LoopBe Internal MIDI".ToLower())
            {
                LoopMidi = device;
                LoopMidi.NoteOn += NoteOn;
                LoopMidi.ControlChange += ControlChange;
                LoopMidi.Open();
                LoopMidi.StartReceiving(null);
                Debug.Log("Connected to LoopBe Port");
            }
        }
    }

    private void OnDestroy()
    {
        if (LoopMidi != null)
        {
            LoopMidi.StopReceiving();
            LoopMidi.Close();
        }
    }

    private static void Client_DeviceDiscovered(object sender, LifxClient.DeviceDiscoveryEventArgs e)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(LogDeviceName(e));
        LightBulbs.Add((LightBulb)e.Device);
    }

    private static IEnumerator LogDeviceName(LifxClient.DeviceDiscoveryEventArgs e)
    {
        Debug.Log(Client.GetDeviceLabelAsync(e.Device));
        yield return null;
    }

    private static void Client_DeviceLost(object sender, LifxClient.DeviceDiscoveryEventArgs e)
    {
        LightBulbs.Remove((LightBulb)e.Device);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            foreach (var lightBulb in LightBulbs)
            {
                ToggleBulb(lightBulb);
            }
        }
        else if (Input.GetKeyDown(KeyCode.O))
        {
            NoteOn(null);
        } else if (Input.GetKeyDown(KeyCode.I))
        {
            CycleHue = !CycleHue;
        }
        if (CycleHue)
        {
            var timeScale = 1;
            var hue = Mathf.RoundToInt(Mathf.Sin(Time.fixedTime * timeScale).Map(-1, 1, 0, 65535));
            foreach (var lightBulb in LightBulbs)
            {
                SetHue(lightBulb, (ushort)hue);
            }
        }
    }

    public static void SetHueFromCC(LightBulb target, float hueCC)
    {
        var convertedHue = (ushort) Mathf.RoundToInt(hueCC.Map(0f, 127f, 0f, 65535f));
        SetHue(target, convertedHue);
    }

    public static async void SetHue(LightBulb target, ushort newHue)
    {
        Debug.Log(target.HostName + ".Hue = " + newHue);
        var last = LastOnLightState;
        await Client.SetColorAsync(target, newHue, last.Saturation, last.Brightness, last.Kelvin, TimeSpan.FromTicks(0));
        LastOnLightState = (await Client.GetLightStateAsync(target));
    }

    private async static void ToggleBulb(LightBulb target)
    {
        var state = await Client.GetLightStateAsync(target);
        var isOn = state.Brightness != 0;
        if (isOn)
        {
            await Client.SetColorAsync(target, state.Hue, state.Saturation, 0, 9000,
                TimeSpan.FromMilliseconds(0));
        }
        else
        {
            ushort brightness = 3000;
            if (LastOnLightState == null)
            {
                LastOnLightState = state;
            }
            if (LastOnLightState.Brightness != 0)
            {
                brightness = LastOnLightState.Brightness;
            }
            await Client.SetColorAsync(target, state.Hue, state.Saturation, brightness,
                LastOnLightState.Kelvin, TimeSpan.FromMilliseconds(0));
        }
    }

    private static void NoteOn(NoteOnMessage noteOnMessage)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(RandomiseHue);
    }

    private static void ControlChange(ControlChangeMessage controlChangeMessage)
    {
        if (controlChangeMessage.Device.Name == "LoopBe Internal MIDI")
        {
                LightBulbs.ForEach(bulb => SetHueFromCC(bulb, controlChangeMessage.Value));
        }
    }

    private async static void RandomiseHue()
    {
        foreach (var target in LightBulbs)
        {
            //var state = await Client.GetLightStateAsync(target);
            var newHue = (ushort)Mathf.RoundToInt(UnityEngine.Random.value * 65535);
            await Client.SetColorAsync(target, newHue, 65535, 65535, 9000, TimeSpan.FromMilliseconds(0));
        }
    }


}

namespace Eideteic
{

    partial class Utility
    {

        static bool RunOnMain(Action action)
        {
            UnityMainThreadDispatcher.Instance().Enqueue(action);
            return true;
        }

    }

}
