using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPReceive : MonoBehaviour
{
    // receiving Thread
    Thread receiveThread;
    Thread startThread;

    // udpclient object
    UdpClient client;
    UdpClient recievestart;

    // public string IP = "127.0.0.1"; default local
    public int port; // define > init

    //recieve boolean
    public bool syncrecieve;

    // infos
    public Vector3 lastReceivedUDPPacket;

    //constants
    const int INIT_PORT = 8051;
    const int SYNC_PORT = 1738;
    //IPAddress MULTICAST_ADDR = IPAddress.Parse("239.0.0.222");

    // start from unity3d
    public void Start()
    {
        print("UDPSend.init()");

        // status
        print("Listening on 127.0.0.1 : " + INIT_PORT + " User Data");
        print("Listening on 127.0.0.1 : " + SYNC_PORT + "Sync Data");


        receiveThread = new Thread(
            new ThreadStart(ReceiveData));
        startThread = new Thread(
            new ThreadStart(ReceiveStart));
        receiveThread.IsBackground = true;
        startThread.IsBackground = true;
        startThread.Start();
        receiveThread.Start();
    }

    // receive thread
    private void ReceiveData()
    {
        client = new UdpClient();

        IPEndPoint localEp = new IPEndPoint(IPAddress.Any, INIT_PORT);
        client.Client.Bind(localEp);

        //sclient.JoinMulticastGroup(MULTICAST_ADDR);
        while (true)
        {
            try
            {
                byte[] data = client.Receive(ref localEp);
                string text = Encoding.UTF8.GetString(data);
                string[] message = text.Split(',');
                Vector3 result = new Vector3(float.Parse(message[0]), float.Parse(message[1]), float.Parse(message[2]));

                print(">> " + result);

                lastReceivedUDPPacket = result;
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
    }

    //return last udp packet
    public Vector3 getLatestUDPPacket()
    {
        return lastReceivedUDPPacket;
    }

    // receive thread
    private void ReceiveStart()
    {
        recievestart = new UdpClient(SYNC_PORT);
        byte[] data = new byte[3];
        IPEndPoint unityEp = new IPEndPoint(IPAddress.Any, 0);
        recievestart.Client.Bind(unityEp);

        //client.JoinMulticastGroup(MULTICAST_ADDR);
        while (data[0] != 49)
        {
            try
            {
                print("Listening");
                data = client.Receive(ref unityEp);
                string text = Encoding.UTF8.GetString(data);
            }
            catch (Exception err)
            {
                print(err.ToString());
            }
        }
        if (data[0] == 49)
        {
            print("Recieved Confirmation. Starting Simulation");
            syncrecieve = true;
        }
    }

    public bool getRecieve()
    {
        return syncrecieve;
    }

    public void OnApplicationQuit()
    {
        if (receiveThread.IsAlive)
        {
            receiveThread.Abort();
        }
        client.Close();
    }
}