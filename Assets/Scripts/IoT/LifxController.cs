using LifxNet;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using System;
using Midi;

public class LifxController : MonoBehaviour
{

    static LifxClient Client;
    static List<LightBulb> BulbList = new List<LightBulb>();

    static InputDevice LoopMidi;

    // Use this for initialization
    void Start()
    {
        var task = LifxClient.CreateAsync();
        task.Wait();
        Client = task.Result;
        Client.DeviceDiscovered += Client_DeviceDiscovered;
        Client.DeviceLost += Client_DeviceLost;
        Client.StartDeviceDiscovery();

        foreach (var device in InputDevice.InstalledDevices)
        {
            if (device.Name.ToLower() == "loopMIDI Port".ToLower())
            {
                LoopMidi = device;
                LoopMidi.NoteOn += NoteOn;
                LoopMidi.Open();
                LoopMidi.StartReceiving(null);
                Debug.Log("Connected to LoopMIDI Port");
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

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            foreach (var lightBulb in BulbList)
            {
                ToggleBulb(lightBulb);
            }
        } else if (Input.GetKeyDown(KeyCode.O))
        {
            NoteOn(null);
        }
    }

    private async static void ToggleBulb(LightBulb target)
    {
        var state = await Client.GetLightStateAsync(target);
        var isOn = state.Brightness != 0;
        if (isOn)
        {
            await Client.SetColorAsync(target, state.Hue, state.Saturation, 0, 9000, TimeSpan.FromMilliseconds(0));
        }
        else
        {
            await Client.SetColorAsync(target, state.Hue, state.Saturation, 65535, 9000, TimeSpan.FromMilliseconds(0));
        }
    }

    private static void NoteOn(NoteOnMessage noteOnMessage)
    {
        UnityMainThreadDispatcher.Instance().Enqueue(RandomiseHue);
    }

    private async static void RandomiseHue()
    {
        foreach (var target in BulbList)
        {
            //var state = await Client.GetLightStateAsync(target);
            var newHue = (ushort)Mathf.RoundToInt(UnityEngine.Random.value * 65535);
            await Client.SetColorAsync(target, newHue, 65535, 65535, 9000, TimeSpan.FromMilliseconds(0));
        }
    }

    private static void Client_DeviceDiscovered(object sender, LifxClient.DeviceDiscoveryEventArgs e)
    {
        Debug.Log(Client.GetDeviceLabelAsync(e.Device));
        BulbList.Add((LightBulb)e.Device);
    }

    private static void Client_DeviceLost(object sender, LifxClient.DeviceDiscoveryEventArgs e)
    {
        BulbList.Remove((LightBulb)e.Device);
    }


}
