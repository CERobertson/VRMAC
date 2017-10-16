using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OSCsharp;
using OSCsharp.Data;
using OSCsharp.Net;

public abstract class OSCTransmitter : MonoBehaviour
{
    public UDPTransmitter transmitter;

    public string device;
    public string ip = "127.0.0.1";
    public int port = 7474;

    private string address;

    // Use this for initialization
    public void Init()
    {
        address = string.Format("/{0}", device);
        transmitter = new UDPTransmitter(ip, port);
        transmitter.Connect();
    }

    // Update is called once per frame
    public void SendPosition()
    {
        OscMessage m = new OscMessage(address);
        m.Append(transform.position.x);
        m.Append(transform.position.y);
        m.Append(transform.position.z);
        transmitter.Send(m);
    }
}