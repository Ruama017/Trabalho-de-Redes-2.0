using UnityEngine;
using UnityEngine.UI;

public class CardBehavior : MonoBehaviour
{
    public int id;                 // ID da carta (define o par)
    public GameObject frente;      // Objeto que mostra a frente da carta
    public Image frenteImage;      // Componente de imagem para a frente da carta

    private Button button;
    private GameController gameController;

    private void Awake()
    {
        button = GetComponent<Button>();
        gameController = FindObjectOfType<GameController>();

        if (gameController == null)
        {
            Debug.LogError("GameController não foi encontrado!");
        }

        button.onClick.AddListener(OnClick);

        frente.SetActive(false);   // Esconde a frente inicialmente
    }

    public void ConfigurarCarta(Sprite imagemFrente, int idCarta)
    {
        id = idCarta;
        frenteImage.sprite = imagemFrente; // Define a imagem da frente
    }

    private void OnClick()
    {
        if (gameController != null)
        {
            gameController.RevelarCarta(this);
        }
    }

    public void Revelar()
    {
        frente.SetActive(true);   // Mostra a frente
        button.interactable = false;  // Desativa o botão
    }

    public void Esconder()
    {
        frente.SetActive(false);  // Esconde a frente
        button.interactable = true;   // Reativa o botão
    }
}




