using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameController : MonoBehaviour
{
    public TextMeshProUGUI textoJogador1;
    public TextMeshProUGUI textoJogador2;
    public Sprite[] imagensCartas;
    public Transform gridContainer;

    public GameObject telaVitoria;          // Painel da tela de vitória
    public TextMeshProUGUI textoVitoria;    // Texto do painel da vitória

    private List<CardBehavior> cartasReveladas = new List<CardBehavior>();
    private int jogadorAtual = 1;
    private int[] pontos = { 0, 0 };

    void Start()
    {
        CriarCartas();
        AtualizarTexto();

        // Começa com a tela de vitória escondida
        if (telaVitoria != null)
            telaVitoria.SetActive(false);
    }

    void Update()
    {
        // Teste manual para abrir a tela de vitória apertando V
        if (Input.GetKeyDown(KeyCode.V))
        {
            if (telaVitoria != null && textoVitoria != null)
            {
                telaVitoria.SetActive(true);
                textoVitoria.text = "Teste: Jogador 1 Ganhou!";
            }
        }
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
        if (cartasReveladas.Count < 2 && !cartasReveladas.Contains(carta))
        {
            carta.Revelar();
            cartasReveladas.Add(carta);

            if (cartasReveladas.Count == 2)
                StartCoroutine(VerificarCartas());
        }
    }

    IEnumerator VerificarCartas()
    {
        yield return new WaitForSeconds(1f);

        if (cartasReveladas[0].id == cartasReveladas[1].id)
        {
            pontos[jogadorAtual - 1]++;
            AtualizarTexto();

            if (pontos[0] + pontos[1] == 4) // todas as cartas foram reveladas
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
}





