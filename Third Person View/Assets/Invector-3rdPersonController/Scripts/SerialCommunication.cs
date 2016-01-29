using UnityEngine;
using System;
using System.IO.Ports;
using System.Collections;
using System.Threading;
using System.Collections.Generic;

public class SerialCommunication
{

    private SerialPort port;
    private string[] ports;
    private int[] frontMotors;
    private int[] backMotors;
    private int i = 1;
    private int interval = 0;
    public SerialCommunication()
    {
        InitConnection();
        frontMotors = new int[]{13,12,11,10,5,3,2,14,15};
        backMotors = new int[] {9,8,7,6};
    }

    private void InitConnection()
    {
        ports = SerialPort.GetPortNames();
        port = new SerialPort("\\\\.\\"+ports[0], 9600);
        Debug.Log(ports[0]);
        port.WriteTimeout = 1000;
        port.Open();
    }

    public void SendMessage(bool inBack)
    {
        if (port.IsOpen)
        {
            interval++;
            if (interval == 5)
            {
                String messageString;
                if (inBack)
                    messageString = backMotors[UnityEngine.Random.Range(0, backMotors.Length)].ToString();
                else
                    messageString = frontMotors[UnityEngine.Random.Range(0, frontMotors.Length)].ToString();
                port.WriteLine(messageString);
                Debug.Log(messageString);
                interval = 0;
            }
        }
    }

    public void SendMultiple(bool FromLeft)
    {
        if (port.IsOpen)
        {
            if (FromLeft)
            {
                port.WriteLine("15");
            }
            else
            {
                port.WriteLine("16");
            }
        }
    }
}
