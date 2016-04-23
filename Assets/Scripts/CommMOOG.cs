using UnityEngine;
using System;

public class CommMOOG : MonoBehaviour
{
    public string localIP = "192.168.4.23";
    public string remoteIP = "192.168.4.164";
    public int receivePort = 994;
    public int sendPort = 993;
    private UDPSend udpSend;
    private UDPReceive udpReceive;
    private Movement movement;
    private float scale = 10;

    // Use this for initialization
    void Start()
    {
        udpSend = new UDPSend(remoteIP, sendPort);
        udpReceive = new UDPReceive(localIP, remoteIP, receivePort);
        movement = FindObjectOfType<Movement>();
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
        UDPSend.addFloat(scale * movement.getLinVel().magnitude);
        UDPSend.addVector(scale * movement.getAngVel());
        UDPSend.addVector(scale * movement.getLinAcc());
        UDPSend.addVector(scale * movement.getAngAcc());
        UDPSend.addVector(transform.rotation.eulerAngles);
        UDPSend.sendPacket();
    }
}
