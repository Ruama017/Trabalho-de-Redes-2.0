using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class GameController : MonoBehaviour
{
    public TextMeshProUGUI textoJogador1;
    public TextMeshProUGUI textoJogador2;
    public Sprite[] imagensCartas;
    public Transform gridContainer;

    public GameObject telaVitoria;
    public TextMeshProUGUI textoVitoria;

    private List<CardBehavior> cartasReveladas = new List<CardBehavior>();
    private int jogadorAtual = 1;
    private int[] pontos = { 0, 0 };

    private TCPNetworkManager networkManager;
    private bool isMinhaVez = true;

    private int seedCompartilhada;

    void Start()
    {
        networkManager = TCPNetworkManager.Instance;
        networkManager.OnMessageReceived += ProcessarMensagem;

        networkManager.StartNetwork();
        isMinhaVez = networkManager.isServer;

        if (networkManager.isServer)
        {
            // Servidor gera seed aleatória e envia
            seedCompartilhada = Random.Range(0, int.MaxValue);
            networkManager.SendMessageToOther($"SEED|{seedCompartilhada}");

            AplicarSeedECriarCartas(seedCompartilhada);
        }

        AtualizarTexto();

        if (telaVitoria != null)
            telaVitoria.SetActive(false);
        void Start()
        {
            networkManager = TCPNetworkManager.Instance;
            networkManager.OnMessageReceived += ProcessarMensagem;
            networkManager.StartNetwork();
            isMinhaVez = networkManager.isServer;

            // === ADIÇÃO: iniciar chat de voz local ===
            var voz = new GameObject("VoiceChat");
            var sender = voz.AddComponent<VoiceSender>();
            var receiver = voz.AddComponent<VoiceReceiver>();

            if (networkManager.isServer)
                sender.ipDestino = "IP_DO_CLIENTE_AQUI"; // IP do outro jogador
            else
                sender.ipDestino = "IP_DO_SERVIDOR_AQUI";

            receiver.porta = 8050; // mesma porta em ambos
            // ========================================

            if (networkManager.isServer)
            {
                seedCompartilhada = Random.Range(0, int.MaxValue);
                networkManager.SendMessageToOther($"SEED|{seedCompartilhada}");
                AplicarSeedECriarCartas(seedCompartilhada);
            }

            AtualizarTexto();

            if (telaVitoria != null)
                telaVitoria.SetActive(false);
        }

    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (telaVitoria != null && textoVitoria != null)
            {
                telaVitoria.SetActive(true);
                textoVitoria.text = "Teste: Jogador 1 Ganhou!";
            }
        }
    }

    void AplicarSeedECriarCartas(int seed)
    {
        seedCompartilhada = seed;
        Random.InitState(seedCompartilhada);
        CriarCartas();
    }

    void CriarCartas()
    {
        int[] ids = { 1, 1, 2, 2, 3, 3, 4, 4 };
        Shuffle(ids);

        CardBehavior[] cartas = gridContainer.GetComponentsInChildren<CardBehavior>();

        if (cartas.Length != ids.Length)
        {
            Debug.LogError("Número de cartas e IDs não batem!");
            return;
        }

        for (int i = 0; i < ids.Length; i++)
        {
            cartas[i].ConfigurarCarta(imagensCartas[ids[i] - 1], ids[i]);
        }
    }

    void Shuffle(int[] array)
    {
        for (int i = 0; i < array.Length; i++)
        {
            int temp = array[i];
            int randomIndex = Random.Range(0, array.Length);
            array[i] = array[randomIndex];
            array[randomIndex] = temp;
        }
    }

    public void RevelarCarta(CardBehavior carta)
    {
        if (!isMinhaVez || cartasReveladas.Count >= 2 || cartasReveladas.Contains(carta))
            return;

        carta.Revelar();
        cartasReveladas.Add(carta);

        networkManager.SendMessageToOther($"CLICK|{carta.transform.GetSiblingIndex()}");

        if (cartasReveladas.Count == 2)
            StartCoroutine(VerificarCartas());
    }

    void ProcessarMensagem(string msg)
    {
        if (msg.StartsWith("CLICK|"))
        {
            int index = int.Parse(msg.Split('|')[1]);
            CardBehavior carta = gridContainer.GetChild(index).GetComponent<CardBehavior>();

            carta.Revelar();
            cartasReveladas.Add(carta);

            if (cartasReveladas.Count == 2)
                StartCoroutine(VerificarCartas());
        }
        else if (msg.StartsWith("SEED|"))
        {
            int seed = int.Parse(msg.Split('|')[1]);
            AplicarSeedECriarCartas(seed);
        }
    }

    IEnumerator VerificarCartas()
    {
        yield return new WaitForSeconds(1f);

        if (cartasReveladas[0].id == cartasReveladas[1].id)
        {
            pontos[jogadorAtual - 1]++;
            AtualizarTexto();

            if (pontos[0] + pontos[1] == 4)
            {
                FimDeJogo();
            }
        }
        else
        {
            foreach (var carta in cartasReveladas)
                carta.Esconder();

            jogadorAtual = jogadorAtual == 1 ? 2 : 1;
        }

        cartasReveladas.Clear();
        isMinhaVez = !isMinhaVez;
    }

    void AtualizarTexto()
    {
        textoJogador1.text = $"Jogador 1: {pontos[0]}";
        textoJogador2.text = $"Jogador 2: {pontos[1]}";
    }

    void FimDeJogo()
    {
        if (telaVitoria != null && textoVitoria != null)
        {
            telaVitoria.SetActive(true);

            if (pontos[0] > pontos[1])
                textoVitoria.text = "Jogador 1 Ganhou!";
            else if (pontos[1] > pontos[0])
                textoVitoria.text = "Jogador 2 Ganhou!";
            else
                textoVitoria.text = "Empate!";
        }
    }

    public void ReiniciarJogo()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
}





