#if WINDOWS_UWP

using System;
using System.Text;
using System.Threading.Tasks;
using Windows.Networking;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using UnityEngine;

public class UWPTCPClient
{
    public delegate void OnReadFinished(string result);
    public event OnReadFinished ReadFinishedEvent;

    public delegate void OnConnectFinished();
    public event OnConnectFinished ConnectFinishedEvent;

    private StreamSocket socket;
    public bool isConnected = false;
    
    public void Connect(string host, int port) {
        var task = Task.Run(async () => { await ConnectAsync(host, port); });
    }

    private async Task ConnectAsync(string host, int port)
    {
        socket = new StreamSocket();
        HostName hostName = new HostName(host);

        // Set NoDelay to false so that the Nagle algorithm is not disabled
        socket.Control.NoDelay = false;

        try
        {
            await socket.ConnectAsync(hostName, port.ToString());
            isConnected = true;
        }
        catch (Exception exception)
        {
            switch (SocketError.GetStatus(exception.HResult))
            {
                case SocketErrorStatus.HostNotFound:
                    Debug.Log("Host not found");
                    throw;
                default:
                    Debug.Log("exception on socket connection: " + exception.Message);
                    throw;
            }
        }

        ConnectFinishedEvent();
    }

    public void Read() {
        Task.Run(async () => { await ReadAsync(); });  
    }

    private async Task ReadAsync()
    {
        if (! isConnected)
        {
            return;
        }

        DataReader reader;
        StringBuilder strBuilder;

        using (reader = new DataReader(socket.InputStream))
        {
            strBuilder = new StringBuilder();
            reader.InputStreamOptions = InputStreamOptions.Partial;
            reader.UnicodeEncoding = Windows.Storage.Streams.UnicodeEncoding.Utf8;
            reader.ByteOrder = ByteOrder.LittleEndian;

            var count = await reader.LoadAsync(1024);
            if (count > 0)
            {
                strBuilder.Append(reader.ReadString(count));
                reader.DetachStream(); 
                ReadFinishedEvent(strBuilder.ToString());
            } else
            {
                ReadFinishedEvent("");
            }
        }
    }

    public void Close()
    {
        socket.Dispose();
    }
}

#endif