using UnityEngine;
using System.Collections.Generic;

using System;
using System.Net;
using System.Net.Sockets;

public class UDPSend
{
    //prefs
    private string IP;
    private int port;

    //connection stuff
    private static IPEndPoint computerEP;
    private static UdpClient unity_sender;

    private static byte[] packet;

    public UDPSend(string ip, int p)
    {
        //IP
        IP = ip;
        port = p;

        computerEP = new IPEndPoint(IPAddress.Parse(IP), port);
        unity_sender = new UdpClient();
        unity_sender.Connect(computerEP);
    }

    public static void newPacket(){
        packet = null;
    }
    
    public static void addVector(Vector3 v)
    {
        addFloat(v.x);
        addFloat(v.y);
        addFloat(v.z);
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