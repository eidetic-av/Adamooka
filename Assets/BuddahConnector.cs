// using UnityEngine;
// using System;
// using System.Threading;
// using System.Net;
// using System.Net.Sockets;
// using System.Collections;
// using System.Collections.Generic;
// using System.Text;

// using VVVV_OSC;
// using Curves;


// public class OSCInput : MonoBehaviour {

//     private Thread thread;
//     private OSCReceiver oscin;

//     // create a thread for OSC and Update every 5 miliseconds
//     void Start()
//     {
//         Debug.Log("Attempting to connect at 8008");
//         oscin = new OSCReceiver(8008);
//         Debug.Log("OSC Hash: " + oscin.GetHashCode());
//         thread = new Thread(new ThreadStart(UpdateOSC));
//         thread.Start();
//     }
//     void OnApplicationQuit()
//     {
//         oscin.Close();
//         thread.Interrupt();
//         if (!thread.Join(8008))
//         {
//             thread.Abort();
//         }
//     }
//     public void destroy()
//     {
//         oscin.Close();
//         thread.Interrupt();
//         if (!thread.Join(8008))
//         {
//             thread.Abort();
//         }
//     }
//     void UpdateOSC()
//     {
//         while (true)
//         {
//             OSCPacket msg = oscin.Receive();
//             if (msg != null)
//             {
//                 if (msg.IsBundle())
//                 {
//                     OSCBundle b = (OSCBundle)msg;
//                     foreach (OSCPacket subm in b.Values)
//                     {
//                         ProcessMessage(subm);
//                     }
//                 }
//                 else
//                 {
//                     ProcessMessage(msg);
//                 }
//             }
//             Thread.Sleep(5);
//         }
//     }

//     public void ProcessMessage(OSCPacket message)
//     {
//         /*  ----------------------------
//          *    ROUTING AND INSTRUMENT METHODS
//          *  ----------------------------*/

//         string[] address = message.Address.Split(new string[] { "/" }, StringSplitOptions.None);

//         /* 
//          * 1. Four Note Melody (fourNoteMelody)
//          *      ->  receiving values from 0-1 on addresses 11, 13, 17, 19
//          *      ->  corresponding to volume of track in ableton
//          */

//         if (address[1] == "fourNoteMelody")
//         {
//             float value = float.Parse(message.Values[0].ToString());
            
//             switch (int.Parse(address[2]))
//             {
//                 case 11: CurveCreator.setTrailLength(11, value);  break;
//                 case 13: CurveCreator.setTrailLength(13, value); break;
//                 case 17: CurveCreator.setTrailLength(17, value); break;
//                 case 19: CurveCreator.setTrailLength(19, value); break;
//             }
//         }
//     }
// }
