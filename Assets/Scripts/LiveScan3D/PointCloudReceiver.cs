using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

#if WINDOWS_UWP
using NetworkCommunication;
#else
using System.Net.Sockets;
using System.Threading;
#endif


public class PointCloudReceiver : MonoBehaviour
{
    public static PointCloudReceiver Instance;

#if WINDOWS_UWP
    TransferSocket socket;
#else
    TcpClient socket;
#endif
    public string IP = "127.0.0.1";
    public int port = 48002;
    public bool ReceivePoints = true;
    public bool LogFrameStatus = false;
    bool bReadyForNextFrame = true;
    bool bConnected = false;

    [HideInInspector()]
    public float[] Vertices;

    void Start()
    {
        Instance = this;
        Connect();
    }

    void Update()
    {
        if (!bConnected)
            return;

        if (!ReceivePoints)
            return;

        float[] vertices;
        byte[] colors;

        if (bReadyForNextFrame)
        {
            if (LogFrameStatus) Debug.Log("Requesting frame");

#if WINDOWS_UWP
            socket.RequestFrame();
            socket.ReceiveFrameAsync();
#else
            RequestFrame();
#endif
            bReadyForNextFrame = false;
        }

#if WINDOWS_UWP
        if (socket.GetFrame(out vertices, out colors))
    #else
        if (ReceiveFrame(out vertices, out colors))
    #endif
        {
            if (LogFrameStatus) Debug.Log("Frame received");
            Vertices = vertices;
            bReadyForNextFrame = true;
        }
    }

    public void Connect()
    {
#if WINDOWS_UWP
        socket = new NetworkCommunication.TransferSocket(IP, port);
#else
        socket = new TcpClient(IP, port);
#endif
        bConnected = true;
        Debug.Log("Connnected to LiveScan3D server at " + IP + ":" + port);
    }

    //Frame receiving for the editor
#if WINDOWS_UWP
#else
    void RequestFrame()
    {
        byte[] byteToSend = new byte[1];
        byteToSend[0] = 0;

        socket.GetStream().Write(byteToSend, 0, 1);
    }

    int ReadInt()
    {
        byte[] buffer = new byte[4];
        int nRead = 0;
        while (nRead < 4)
            nRead += socket.GetStream().Read(buffer, nRead, 4 - nRead);

        return BitConverter.ToInt32(buffer, 0);
    }

    bool ReceiveFrame(out float[] lVertices, out byte[] lColors)
    {
        int nPointsToRead = ReadInt();

        lVertices = new float[3 * nPointsToRead];
        short[] lShortVertices = new short[3 * nPointsToRead];
        lColors = new byte[3 * nPointsToRead];

        int nBytesToRead = sizeof(short) * 3 * nPointsToRead;
        int nBytesRead = 0;
        byte[] buffer = new byte[nBytesToRead];

        while (nBytesRead < nBytesToRead)
            nBytesRead += socket.GetStream().Read(buffer, nBytesRead, Math.Min(nBytesToRead - nBytesRead, 64000));

        System.Buffer.BlockCopy(buffer, 0, lShortVertices, 0, nBytesToRead);

        for (int i = 0; i < lShortVertices.Length; i++)
            lVertices[i] = lShortVertices[i] / 1000.0f;

        nBytesToRead = sizeof(byte) * 3 * nPointsToRead;
        nBytesRead = 0;
        buffer = new byte[nBytesToRead];

        while (nBytesRead < nBytesToRead)
            nBytesRead += socket.GetStream().Read(buffer, nBytesRead, Math.Min(nBytesToRead - nBytesRead, 64000));

        System.Buffer.BlockCopy(buffer, 0, lColors, 0, nBytesToRead);

        return true;
    }
#endif
}
