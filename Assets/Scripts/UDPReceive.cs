using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive
{
    // Address
    private string localIP;
    private string remoteIP;
    private int port;

    // Connection
    private static IPEndPoint unityEP;
    private static IPEndPoint computer_sender_EP;
    private static UdpClient unity_receiver;

    private static byte[] packet;

    private static bool abortThread;

    // receiving Thread
    private Thread receiveThread;

    public UDPReceive(string local, string remote, int p)
    {
        localIP = local;
        remoteIP = remote;
        port = p;

        unityEP = new IPEndPoint(IPAddress.Parse(localIP), port);
        computer_sender_EP = new IPEndPoint(IPAddress.Parse(remoteIP), port);

        unity_receiver = new UdpClient();
        unity_receiver.Client.Bind(unityEP);
        unity_receiver.Client.ReceiveTimeout = 1000;

        packet = new byte[16];

        abortThread = false;

        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    // receive thread
    private void ReceiveData()
    {
        while(!abortThread)
        {
            try
            {
                packet = unity_receiver.Receive(ref unityEP);
            }
            catch(Exception err)
            {
                Debug.Log(err.ToString());
            }            
        }
    }

    public static byte[] getMOOGData()
    {
        return packet;
    }

    public void OnApplicationQuit()
    {
        abortThread = true;
    }
}