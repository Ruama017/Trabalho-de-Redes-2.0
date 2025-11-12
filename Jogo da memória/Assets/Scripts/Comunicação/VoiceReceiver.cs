using UnityEngine;
using System.Net;
using System.Net.Sockets;
using System.Threading;

public class VoiceReceiver : MonoBehaviour
{
    public int porta = 8050;
    private UdpClient udp;
    private AudioSource audioSource;
    private const int sampleRate = 8000;

    void Start()
    {
        udp = new UdpClient(porta);
        audioSource = gameObject.AddComponent<AudioSource>();
        new Thread(ReceberLoop).Start();
    }

    void ReceberLoop()
    {
        IPEndPoint remoteEP = new IPEndPoint(IPAddress.Any, 0);
        while (true)
        {
            byte[] data = udp.Receive(ref remoteEP);
            float[] samples = BytesParaFloatArray(data);

            AudioClip clip = AudioClip.Create("Voz", samples.Length, 1, sampleRate, false);
            clip.SetData(samples, 0);
            audioSource.PlayOneShot(clip);
        }
    }

    float[] BytesParaFloatArray(byte[] byteArray)
    {
        int sampleCount = byteArray.Length / 2;
        float[] floatArray = new float[sampleCount];
        for (int i = 0; i < sampleCount; i++)
        {
            short val = (short)(byteArray[i * 2] | (byteArray[i * 2 + 1] << 8));
            floatArray[i] = val / (float)short.MaxValue;
        }
        return floatArray;
    }

    void OnApplicationQuit()
    {
        udp?.Close();
    }
}