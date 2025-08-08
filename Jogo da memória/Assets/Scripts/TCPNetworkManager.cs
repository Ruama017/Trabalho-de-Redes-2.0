using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class TCPNetworkManager : MonoBehaviour
{
    public static TCPNetworkManager Instance;

    public bool isServer = false; // Ative no Inspector em um PC para ser o host
    public string ipAddress = "127.0.0.1"; // IP do servidor (altere no cliente)

    private TcpListener server;
    private TcpClient client;
    private NetworkStream stream;
    private StreamReader reader;
    private StreamWriter writer;
    private Thread networkThread;

    public event Action<string> OnMessageReceived;

    private void Awake()
    {
        // Singleton para acesso global
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void StartNetwork()
    {
        if (isServer)
        {
            StartServer();
        }
        else
        {
            StartClient();
        }
    }

    private void StartServer()
    {
        networkThread = new Thread(() =>
        {
            try
            {
                server = new TcpListener(IPAddress.Any, 5000);
                server.Start();
                Debug.Log("Servidor aguardando conexão...");
                client = server.AcceptTcpClient();
                Debug.Log("Cliente conectado!");
                SetupStreams();
            }
            catch (Exception e)
            {
                Debug.LogError("Erro no servidor: " + e.Message);
            }
        });
        networkThread.Start();
    }

    private void StartClient()
    {
        networkThread = new Thread(() =>
        {
            try
            {
                client = new TcpClient();
                Debug.Log("Conectando ao servidor...");
                client.Connect(ipAddress, 5000);
                Debug.Log("Conectado ao servidor!");
                SetupStreams();
            }
            catch (Exception e)
            {
                Debug.LogError("Erro no cliente: " + e.Message);
            }
        });
        networkThread.Start();
    }

    private void SetupStreams()
    {
        stream = client.GetStream();
        reader = new StreamReader(stream);
        writer = new StreamWriter(stream);

        while (true)
        {
            try
            {
                string message = reader.ReadLine();
                if (!string.IsNullOrEmpty(message))
                {
                    Debug.Log("Recebido: " + message);
                    OnMessageReceived?.Invoke(message);
                }
            }
            catch
            {
                Debug.LogWarning("Conexão encerrada.");
                break;
            }
        }
    }

    public void SendMessageToOther(string message)
    {
        try
        {
            if (writer != null)
            {
                writer.WriteLine(message);
                writer.Flush();
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Erro ao enviar mensagem: " + e.Message);
        }
    }

    private void OnApplicationQuit()
    {
        try
        {
            reader?.Close();
            writer?.Close();
            stream?.Close();
            client?.Close();
            server?.Stop();
            networkThread?.Abort();
        }
        catch { }
    }
}

