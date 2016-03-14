using UnityEngine;
using System.Collections;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPSend : MonoBehaviour
{
    //private static int localport;
    public static Socket unity;

    //prefs
    private string IP;
    public int port;

    //connection stuff
    IPEndPoint remoteEndPoint;
    UdpClient client;

    //response boolean
    public bool response = false;

    //send boolean
    public bool sent = false;

    public string log_filename = "log";

    // Use this for initialization
    void Start()
    {
        print("Initiating start sequence");
        System.IO.StreamWriter clearLog = new System.IO.StreamWriter(log_filename + ".txt", false);
        clearLog.Dispose();
        init();
    }

    public void init()
    {
        print("UDPSend.init()");
        print("Waiting for start button (hint: its space)");

        //IP
        IP = "127.0.0.1"; //193.168.1.2
        port = 993;

        remoteEndPoint = new IPEndPoint(IPAddress.Parse(IP), port);
        client = new UdpClient();

    }

    // Update is called once per frame
    void Update()
    {
        if (sent == false)
        {
            sendString("0");
            if (Input.GetKeyDown(KeyCode.Space))
            {
                sendString("1");
                print("Start message sent");
                print("Sending to " + IP + " : " + port);
                sent = true;
            }
        }
    }

    public void sendString(string message)
    {
        try
        {
            if (sent)
            {
                //byte[] data = Encoding.UTF8.GetBytes(message);
                using (System.IO.StreamWriter sw = new System.IO.StreamWriter(log_filename + ".txt", true))
                {
                    sw.WriteLine(message);
                }
                //client.Send(data, data.Length, remoteEndPoint);
            }
        }
        catch(Exception e)
        {
            print(e.Message);
        }
    }
}