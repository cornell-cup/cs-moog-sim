using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class UDPSend : MonoBehaviour
{
    //prefs
    private string IP;
    private int port;

    //connection stuff
    private static IPEndPoint computerEP;
    private static UdpClient unity_sender;

    private static byte[] packet;

    private string log_filename = "log_"+DateTime.Now.ToFileTime();

    // Use this for initialization
    public void Start()
    {
        System.IO.StreamWriter clearLog = new System.IO.StreamWriter(log_filename + ".txt", false);
        clearLog.Dispose();
        init();
    }

    public void init()
    {
        //IP
        IP = "127.0.0.1"; // 192.168.4.164
        port = 993;

        computerEP = new IPEndPoint(IPAddress.Parse(IP), port);
        unity_sender = new UdpClient();
        unity_sender.Connect(computerEP);
    }

    public static void newPacket(){
        packet = null;
    }

    public static float addFloat(float f){
        byte[] newFloatByte = BitConverter.GetBytes(f);
        if(packet != null){
            List<byte> oldlist = new List<byte>(packet);
            List<byte> newlist = new List<byte>(newFloatByte);
            oldlist.AddRange(newlist);
            packet = oldlist.ToArray();
        }
        else{
            packet = newFloatByte;
        }
        return f;
    }

    public static void sendPacket(){
        unity_sender.Send(packet, packet.Length);
    }
    
    public void logPacket(string message)
    {
        try
        {
            using (System.IO.StreamWriter sw = new System.IO.StreamWriter(log_filename + ".txt", true))
            {
                sw.WriteLine(message);
            }
        }
        catch(Exception e)
        {
            print(e.Message);
        }
    }
}