#if !WINDOWS_UWP

using System;
using System.Net.Sockets;
using System.IO;
using UnityEngine;
using System.Text;
using System.Collections.Generic;

public class UnityTCPClient
{
    public delegate void OnReadFinished(string result);
    public event OnReadFinished ReadFinishedEvent;

    public delegate void OnConnectFinished();
    public event OnConnectFinished ConnectFinishedEvent;

    private TcpClient mySocket;
    private NetworkStream theStream;
    private StreamReader reader;

    public void Connect(string host, int port)
    {
        try
        {
            mySocket = new TcpClient(host, port);
            theStream = mySocket.GetStream();
            theStream.ReadTimeout = 1;
            reader = new StreamReader(theStream, Encoding.UTF8);
        }
        catch (Exception e)
        {
            Debug.Log("Socket error: " + e);
        }
        ConnectFinishedEvent();
    }
    
    public IEnumerator<string> Read()
    {
        var readFinished = false;
        while (!readFinished)
        {
            var line = "";
            try
            {
                line = reader.ReadLine();
            }
            catch (Exception e)
            {
                //Debug.Log("exception: " + e.Message);
            }

            if (line != "")
            {
                readFinished = true;
                ReadFinishedEvent(line);
            }
            yield return null;            
        }
    }
}

#endif