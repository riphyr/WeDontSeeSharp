using UnityEngine;

public class PokerHand : MonoBehaviour
{
    public GameObject cardPrefab; // Le prefab d'une carte (un Quad ou un objet 2D avec un SpriteRenderer)
    public Transform cardHandPosition; // Position où les cartes seront affichées (par exemple, les mains du joueur)
    public Sprite[] cardFaces; // Tableau des images de l'avant des cartes
    public Sprite cardBack; // Image de l'arrière des cartes

    private GameObject[] playerCards = new GameObject[2]; // Stocke les deux cartes du joueur

    public void DealCards()
    {
        ClearHand(); // Nettoie les anciennes cartes si existantes

        for (int i = 0; i < 2; i++)
        {
            // Crée une carte à la position définie
            GameObject card = Instantiate(cardPrefab, cardHandPosition);
            card.transform.localPosition = new Vector3(i * 0.5f, 0, 0); // Décalage entre les deux cartes

            // Assigne une carte aléatoire
            SpriteRenderer spriteRenderer = card.GetComponent<SpriteRenderer>();
            spriteRenderer.sprite = GetRandomCardFace();

            // Ajoute un dos (si applicable)
            AddBackToCard(card);

            // Sauvegarde la carte
            playerCards[i] = card;
        }
    }

    private Sprite GetRandomCardFace()
    {
        // Tire une carte aléatoire dans le tableau des faces
        int randomIndex = Random.Range(0, cardFaces.Length);
        return cardFaces[randomIndex];
    }

    private void AddBackToCard(GameObject card)
    {
        // Crée un Quad ou un sprite pour l'arrière de la carte
        GameObject back = new GameObject("CardBack");
        back.transform.parent = card.transform;
        back.transform.localPosition = Vector3.zero;

        SpriteRenderer backRenderer = back.AddComponent<SpriteRenderer>();
        backRenderer.sprite = cardBack;
        backRenderer.sortingOrder = 1; // Place le dos au-dessus de l'avant

        // (Optionnel) Cache l'arrière ou le bascule selon l'état
        back.SetActive(false); // Par défaut, n'affiche pas le dos
    }

    private void ClearHand()
    {
        // Détruit les cartes existantes dans la main
        foreach (GameObject card in playerCards)
        {
            if (card != null)
                Destroy(card);
        }
    }
}




/*using UnityEngine;
using System.Collections.Generic;

public class PokerHandWithQuad : MonoBehaviour
{
    [Header("Card Settings")]
    public Material[] cardMaterials;       // Liste des matériaux pour les cartes (visuels)
    public GameObject cardPrefab;          // Prefab d'une carte (un Quad)
    public Transform leftHandPosition;     // Position de la carte gauche
    public Transform rightHandPosition;    // Position de la carte droite

    private List<string> deck = new List<string>(); // Liste des cartes dans le deck
    private List<string> playerHand = new List<string>(); // Cartes dans la main du joueur

    void Start()
    {
        InitializeDeck(); // Initialise le deck au démarrage
    }

    // Initialise le deck de cartes
    private void InitializeDeck()
    {
        string[] suits = { "Hearts", "Diamonds", "Clubs", "Spades" }; // Couleurs des cartes
        string[] values = { "2", "3", "4", "5", "6", "7", "8", "9", "10", "J", "Q", "K", "A" }; // Valeurs des cartes

        // Remplir le deck avec toutes les cartes possibles
        foreach (string suit in suits)
        {
            foreach (string value in values)
            {
                deck.Add(value + " of " + suit);
            }
        }

        Debug.Log("Deck initialized with " + deck.Count + " cards.");
    }

    // Pioche deux cartes pour le joueur
    public void DealHand()
    {
        if (deck.Count < 2)
        {
            Debug.LogError("Not enough cards in the deck!");
            return;
        }

        // Vider la main actuelle
        playerHand.Clear();

        // Tirer deux cartes au hasard
        for (int i = 0; i < 2; i++)
        {
            int randomIndex = Random.Range(0, deck.Count);
            string card = deck[randomIndex];
            playerHand.Add(card);
            deck.RemoveAt(randomIndex); // Retirer la carte du deck
        }

        Debug.Log("Player's hand: " + playerHand[0] + ", " + playerHand[1]);

        // Afficher les cartes dans les mains du joueur
        ShowCardsInHand();
    }

    // Afficher les cartes dans les positions des mains
    private void ShowCardsInHand()
    {
        // Carte gauche
        CreateCardObject(playerHand[0], leftHandPosition);

        // Carte droite
        CreateCardObject(playerHand[1], rightHandPosition);
    }

    // Crée un objet de carte visuel
    private void CreateCardObject(string cardName, Transform handPosition)
    {
        // Instancier un Quad à partir du prefab
        GameObject cardObject = Instantiate(cardPrefab, handPosition.position, Quaternion.identity);
        cardObject.transform.SetParent(handPosition);

        // Assigner le matériau correspondant à la carte
        MeshRenderer renderer = cardObject.GetComponent<MeshRenderer>();
        renderer.material = GetCardMaterial(cardName);
    }

    // Trouver le matériau correspondant à une carte
    private Material GetCardMaterial(string cardName)
    {
        foreach (Material mat in cardMaterials)
        {
            if (mat.name == cardName)
            {
                return mat;
            }
        }

        Debug.LogWarning("No material found for card: " + cardName);
        return null;
    }
}*/
