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

    
    // Use this for initialization
    public void Start()
    {
        init();
    }

    public void init()
    {
        //IP
        IP = "192.168.4.164";
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
}