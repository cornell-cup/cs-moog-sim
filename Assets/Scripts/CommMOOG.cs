using UnityEngine;
using System;

public class CommMOOG : MonoBehaviour
{
    public string localIP = "127.0.0.1";
    public string remoteIP = "127.0.0.1";
    public int receivePort = 994;
    public int sendPort = 993;
    private UDPSend udpSend;
    private UDPReceive udpReceive;
    private float scale = 20;

    // Use this for initialization
    void Start()
    {
        udpSend = new UDPSend(remoteIP, sendPort);
        udpReceive = new UDPReceive(localIP, remoteIP, receivePort);
    }

    // Update is called once per frame
    void Update()
    {
        correctOrientation();
        sendData();
    }

    // cancels out MOOG rotations from the camera
    private void correctOrientation()
    {
        byte[] packet = UDPReceive.getMOOGData();
        if (packet != null)
        {
            float time = BitConverter.ToSingle(packet, 0);
            Vector3 orientation = new Vector3(
                BitConverter.ToSingle(packet, 4),
                BitConverter.ToSingle(packet, 8),
                BitConverter.ToSingle(packet, 12));
            transform.Rotate(-orientation);
        }
    }

    // log and send ship motion info to comm
    private void sendData()
    {
        UDPSend.newPacket();
        UDPSend.addFloat(Time.time);
        UDPSend.addFloat(scale * Movement.instance.getLinVel().magnitude);
        UDPSend.addVector(scale * Movement.instance.getAngVel());
        UDPSend.addVector(scale * Movement.instance.getLinAcc());
        UDPSend.addVector(scale * Movement.instance.getAngAcc());
        UDPSend.addVector(transform.rotation.eulerAngles);
        UDPSend.sendPacket();
    }
}
