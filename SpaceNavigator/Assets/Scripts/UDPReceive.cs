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

    public void Start()
    {
        localIP = "192.168.4.23";
        remoteIP = "192.168.4.164";
        port = 994;

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
                float time = BitConverter.ToSingle(packet, 0);
                Vector3 orientation = new Vector3(
                    BitConverter.ToSingle(packet, 4),
                    BitConverter.ToSingle(packet, 8), 
                    BitConverter.ToSingle(packet, 12));
                Debug.Log(time + " " + orientation);
                //cancel out rotations from the camera
                //transform.Rotate(-orientation);
                transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles-orientation);
            }
            catch(Exception err)
            {
                //print(err.ToString());
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