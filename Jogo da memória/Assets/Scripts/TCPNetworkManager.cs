using System;
using System.IO;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using UnityEngine;

public class TCPNetworkManager : MonoBehaviour
{
    public bool isServer = false;
    public int port = 7777;
    public string ipAddress = "127.0.0.1";

    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;

    private Thread receiveThread;

    public static TCPNetworkManager Instance;

    public Action<string> OnMessageReceived;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        DontDestroyOnLoad(gameObject);
    }

    public void StartNetwork()
    {
        if (isServer)
        {
            server = new TcpListener(IPAddress.Any, port);
            server.Start();
            server.BeginAcceptTcpClient(OnClientConnected, null);
            Debug.Log("Servidor iniciado");
        }
        else
        {
            client = new TcpClient();
            client.BeginConnect(ipAddress, port, OnConnectedToServer, null);
            Debug.Log("Conectando ao servidor...");
        }
    }

    void OnClientConnected(IAsyncResult result)
    {
        client = server.EndAcceptTcpClient(result);
        SetupStreams();
        Debug.Log("Cliente conectado!");
    }

    void OnConnectedToServer(IAsyncResult result)
    {
        client.EndConnect(result);
        SetupStreams();
        Debug.Log("Conectado ao servidor!");
    }

    void SetupStreams()
    {
        stream = client.GetStream();
        reader = new StreamReader(stream);
        writer = new StreamWriter(stream);
        writer.AutoFlush = true;

        receiveThread = new Thread(ReceiveLoop);
        receiveThread.Start();
    }

    void ReceiveLoop()
    {
        while (true)
        {
            try
            {
                string msg = reader.ReadLine();
                if (!string.IsNullOrEmpty(msg))
                {
                    Debug.Log("Recebido: " + msg);
                    OnMessageReceived?.Invoke(msg);
                }
            }
            catch { break; }
        }
    }

    public void SendMessageToOther(string message)
    {
        if (writer != null)
        {
            writer.WriteLine(message);
        }
    }

    void OnApplicationQuit()
    {
        stream?.Close();
        client?.Close();
        server?.Stop();
        receiveThread?.Abort();
    }
}
