using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class VoiceSender : MonoBehaviour
{
    public string ipDestino = "127.0.0.1"; // IP do outro jogador
    public int porta = 8050;               // Porta UDP
    public KeyCode teclaPushToTalk = KeyCode.V;

    private UdpClient udp;
    private AudioClip micClip;
    private int sampleRate = 8000;
    private int lastSample = 0;
    private bool microfoneAtivo = false;

    void Start()
    {
        udp = new UdpClient();
        micClip = Microphone.Start(null, true, 1, sampleRate); // grava 1s em loop
        new Thread(EnviarLoop).Start();
    }

    void Update()
    {
        // Só transmite enquanto segura a tecla
        microfoneAtivo = Input.GetKey(teclaPushToTalk);
    }

    void EnviarLoop()
    {
        while (true)
        {
            if (!microfoneAtivo) { Thread.Sleep(10); continue; }

            int pos = Microphone.GetPosition(null);
            int diff = pos - lastSample;
            if (diff > 0)
            {
                float[] samples = new float[diff];
                micClip.GetData(samples, lastSample);
                byte[] data = FloatArrayParaBytes(samples);
                udp.Send(data, data.Length, ipDestino, porta);
                lastSample = pos;
            }
            Thread.Sleep(10);
        }
    }

    byte[] FloatArrayParaBytes(float[] floatArray)
    {
        byte[] byteArray = new byte[floatArray.Length * 2];
        int index = 0;
        foreach (float f in floatArray)
        {
            short val = (short)(f * short.MaxValue);
            byteArray[index++] = (byte)(val & 0xFF);
            byteArray[index++] = (byte)((val >> 8) & 0xFF);
        }
        return byteArray;
    }

    void OnApplicationQuit()
    {
        udp?.Close();
    }
}