using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive : MonoBehaviour
{
    // Address
    private string IP;
    private int port;

    // Connection
    private static IPEndPoint unityEP;
    private static UdpClient unity_receiver;

    private static byte[] packet;

    // receiving Thread
    private Thread receiveThread;

    public void Start()
    {
        IP = "127.0.0.1";
        port = 994;

        unityEP = new IPEndPoint(IPAddress.Parse(IP), port);
        unity_receiver = new UdpClient();
        unity_receiver.Client.Bind(unityEP);

        packet = new byte[16];

        receiveThread = new Thread(new ThreadStart(ReceiveData));
        receiveThread.IsBackground = true;
        receiveThread.Start();
    }

    // receive thread
    private void ReceiveData()
    {
        while(true)
        {
            try
            {
                packet = unity_receiver.Receive(ref unityEP);
            }
            catch(Exception err)
            {
                print(err.ToString());
            }            
        }
    }

    public static byte[] getMOOGData()
    {
        return packet;
    }

    public void OnApplicationQuit()
    {
        if (receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        unity_receiver.Close();
    }
}