using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using DG.Tweening;
using UnityEngine.UI; 
using InteractionScripts;
using Photon.Realtime;



public class CardDealer : MonoBehaviourPun
{
    public BotController bot; // À assigner dans l’Inspector ou via PokerManager
    private int botID = -1; // Un ID spécial pour le bot
    public Transform botCardPos1;
    public Transform botCardPos2;
    public List<Transform> playerPositions = new List<Transform>();
    public PokerGameManager gameManager;
    public GameObject deckParent;
    public Transform leftPile, rightPile, mergePosition;
    public Dictionary<int, List<Card>> playerHands = new Dictionary<int, List<Card>>();
    //public Dictionary<int, List<Transform>> playerHands = new Dictionary<int, List<Transform>>();
    public float shuffleSpeed = 0.2f;
    public Material cardBackMaterial;
    public Transform[] communityCardPositions; // 5 positions à placer dans l’Inspector
    public List<Card> communityCards = new List<Card>();
    //public List<Transform> communityCards = new List<Transform>();
    private List<Card> deck = new List<Card>();
    //private List<Transform> deck = new List<Transform>();
    private PlayerController[] players;  // Liste des contrôleurs de joueurs
    // private int currentPlayerIndex = 0;

    void Start() // récupérer les enfants du deckParent et les mettre dans le deck
    {
        foreach (Transform card in deckParent.transform)
        {
            Card cards = card.GetComponent<Card>();
            deck.Add(cards);
        }
        StartCoroutine(WaitUntilPlayersAreReady());
    }

    IEnumerator WaitUntilPlayersAreReady()
    {
        GameObject[] players = new GameObject[0];

        // On attend jusqu’à ce qu’au moins un joueur soit détecté
        while (players.Length == 0)
        {
            players = GameObject.FindGameObjectsWithTag("Player");
            Debug.Log("⏳ Recherche de joueurs...");
            yield return new WaitForSeconds(0.5f);
        }

        Debug.Log($"✅ {players.Length} joueur(s) trouvé(s) !");
        foreach (GameObject player in players)
        {
            playerPositions.Add(player.transform);
            Debug.Log($"👤 Ajout du joueur : {player.name}");
        }
    }

    public Card DrawCard() // Prendre une carte au hasard du deck
    {
        if (deck.Count == 0)
        {
            return null;
        }
        int cardIndex = Random.Range(0, deck.Count);
        Card card = deck[cardIndex];
        deck.RemoveAt(cardIndex);
        return card;
    }

    public void StartShuffling()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ShuffleDeck();
        }
    }

    private void ShuffleDeck()
    {
        List<int> shuffledIndices = new List<int>();
        for (int i = 0; i < deck.Count; i++)
            shuffledIndices.Add(i);
        ShuffleList(shuffledIndices);
        List<Card> newDeck = new List<Card>();
        foreach (int index in shuffledIndices)
            newDeck.Add(deck[index]);
        deck = newDeck;
        photonView.RPC("SyncShuffle", RpcTarget.All, shuffledIndices.ToArray());
    }

    void ShuffleList(List<int> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            (list[k], list[n]) = (list[n], list[k]);
        }
    }

    [PunRPC]
    void SyncShuffle(int[] shuffledIndices)
    {
        StartCoroutine(RiffleShuffle(shuffledIndices));
    }

    IEnumerator RiffleShuffle(int[] shuffledIndices)
    {
        List<Card> leftHalf = new List<Card>();
        List<Card> rightHalf = new List<Card>();
        for (int i = 0; i < shuffledIndices.Length; i++)
        {
            if (i < shuffledIndices.Length / 2)
                leftHalf.Add(deck[shuffledIndices[i]]);
            else
                rightHalf.Add(deck[shuffledIndices[i]]);
        }
        // Animation des cartes en deux piles
        for (int i = 0; i < leftHalf.Count; i++)
        {
            leftHalf[i].transform.DOMove(leftPile.position + new Vector3(0, i * 0.01f, 0), shuffleSpeed);
            rightHalf[i].transform.DOMove(rightPile.position + new Vector3(0, i * 0.01f, 0), shuffleSpeed);
        }

        yield return new WaitForSeconds(shuffleSpeed + 0.2f);

        // Fusion des piles
        for (int i = 0; i < Mathf.Max(leftHalf.Count, rightHalf.Count); i++)
        {
            if (i < rightHalf.Count)
            {
                rightHalf[i].transform.DOMove(mergePosition.position + new Vector3(0, i * 0.002f, 0), shuffleSpeed);
                yield return new WaitForSeconds(shuffleSpeed / 2);
            }
            if (i < leftHalf.Count)
            {
                leftHalf[i].transform.DOMove(mergePosition.position + new Vector3(0, i * 0.001f, 0), shuffleSpeed);
                yield return new WaitForSeconds(shuffleSpeed / 2);
            }
        }
        yield return new WaitForSeconds(0.5f);
        StartCoroutine(DealCards());
    }

    public IEnumerator DealCards()
    {
        Debug.Log("Distribution des cartes...");
        players = FindObjectsOfType<PlayerController>();  // Récupère tous les PlayerController dans la scène
        Debug.Log($"players.Count = {players.Length}");

        // Distribution des cartes aux joueurs
        for (int i = 0; i < players.Length; i++)
        {
            int playerID = players[i].GetComponent<PhotonView>().Owner.ActorNumber;
            Debug.Log($"{playerID} le ID du joueur");
            playerHands[playerID] = new List<Card>();
            for (int j = 0; j < 2; j++) // Distribue 2 cartes par joueur
            {
                if (deck.Count == 0)
                    break;
                int cardIndex = 0; // Toujours prendre la première carte après le mélange
                Card card = deck[cardIndex];
                deck.RemoveAt(cardIndex);
                playerHands[playerID].Add(card);
                players[i].SetHandCards(card);
                Debug.Log("Ca vient la");
                photonView.RPC("GiveCardToPlayer", RpcTarget.AllBuffered, card.GetComponent<PhotonView>().ViewID, playerID, j);
            }
        }

        playerHands[botID] = new List<Card>();
        for (int j = 0; j < 2; j++)
        {
            if (deck.Count == 0)
                break;

            int cardIndex = 0;
            Card card = deck[cardIndex];
            deck.RemoveAt(cardIndex);
            playerHands[botID].Add(card);

            // Donne visuellement les cartes au bot si besoin
            // ou garde-les invisibles si le bot n’a pas besoin de les voir
        }

        // Distribution des cartes au bot
        if (gameManager.bot != null)
        {
            List<Card> botCards = new List<Card>();

            for (int i = 0; i < 2; i++)
            {
                if (deck.Count == 0)
                    break;

                Card card = DrawCard();
                botCards.Add(card);

                Transform targetPos = (i == 0) ? botCardPos1 : botCardPos2;
                card.transform.DOMove(targetPos.position, 0.5f);
                card.transform.DORotate(targetPos.rotation.eulerAngles, 0.5f);
            }

            // Enregistre la main du bot dans le manager
            gameManager.bot.SetBotCards(botCards);
        }

        StartGame();
        yield return null;
    }

    [PunRPC]
    IEnumerator GiveCardToPlayer(int cardViewID, int playerID, int cardIndex)
    {
        Debug.Log("Ca give ou pas??????");
        PhotonView cardPV = PhotonView.Find(cardViewID);
        if (cardPV == null)
        {
            Debug.LogError($"❌ ERREUR : Carte avec ViewID {cardViewID} introuvable !");
            yield break;
        }

        GameObject card = cardPV.gameObject;

        if (PhotonNetwork.LocalPlayer.ActorNumber == playerID)
        {
            Debug.Log($"playerPosition.Lenght {playerPositions.Count}");
            Transform playerCardPosition = playerPositions[playerID - 1];
            float distance = -3.3f;
            Vector3 frontPosition = playerCardPosition.position + playerCardPosition.forward * distance;
            float cardSpacing = 0.1f;
            float heightOffset = 0.20f;
            float leftOffset = -11.3f;
            Debug.Log($"📦 Moving card to: {frontPosition + new Vector3(cardIndex * cardSpacing + leftOffset, heightOffset, 0)}");
            card.transform.DOMove(frontPosition + new Vector3(cardIndex * cardSpacing + leftOffset, heightOffset, 0), shuffleSpeed);
            card.transform.DORotate(playerCardPosition.rotation.eulerAngles, shuffleSpeed); 
            

        }

        if (!cardPV.IsMine)
        {
            card.GetComponent<Renderer>().material = cardBackMaterial;
        }
        yield return new WaitForSeconds(0.5f);
        }

    [PunRPC]
    public void DealCommunityCards()
    {
        if (!PhotonNetwork.IsMasterClient)
            return;

        for (int i = 0; i < 5; i++)
        {
            if (deck.Count == 0)
                break;

            Card card = DrawCard();
            communityCards.Add(card);

            // Déplace visuellement la carte vers la position de la table
            card.transform.DOMove(communityCardPositions[i].position, 0.5f);
            card.transform.DORotate(communityCardPositions[i].rotation.eulerAngles, 0.5f);
        }
    }

    void StartGame()
    {
        List<PlayerController> play = new List<PlayerController>();
        foreach(var p in players)
        {
            play.Add(p);
        }
        if (PhotonNetwork.IsMasterClient)
        {
            gameManager.StartRound(play);
        }
    }
}

/*
public class CardDealer : MonoBehaviourPun
{
    public BotController bot; // À assigner dans l’Inspector ou via PokerManager

    private int botID = -1; // Un ID spécial pour le bot

    public Transform botCardPos1;

    public Transform botCardPos2;

    public Transform[] playerPositions;

    public PokerManager gameManager;

    public GameObject deckParent;

    public Transform leftPile, rightPile, mergePosition;

    public Dictionary<int, List<Transform>> playerHands = new Dictionary<int, List<Transform>>();

    public float shuffleSpeed = 0.2f;

    public Material cardBackMaterial;

    public Transform[] communityCardPositions; 
    public List<Transform> communityCards = new List<Transform>();

    private List<Transform> deck = new List<Transform>();

    private Photon.Realtime.Player[] players;

//private int currentPlayerIndex = 0;

    void Start()
{
    foreach (Transform card in deckParent.transform)
    {
        deck.Add(card);
    }
}

    public Transform DrawCard() // Prendre une carte au hasard du deck
    {

        if( deck.Count == 0 )
        {
            return null;
        }
        int cardIndex = Random.Range(0, deck.Count);
        Transform card = deck[cardIndex];
        deck.RemoveAt(cardIndex);
        return card;

    }

    public int GetDeckCount()
    {
        return deck.Count;
    }

    public void StartShuffling()
    {
        if(PhotonNetwork.IsMasterClient)
        {
            ShuffleDeck();
        }
    }



    private void ShuffleDeck()
    {


    List<int> shuffledIndices = new List<int>();
    for (int i = 0; i < deck.Count; i++)
    {
        shuffledIndices.Add(i);
    }
    
    ShuffleList(shuffledIndices);
    List<Transform> newDeck = new List<Transform>();
    foreach (int index in shuffledIndices)
    {
        newDeck.Add(deck[index]);
    }
    deck = newDeck;
    photonView.RPC("SyncShuffle", RpcTarget.All, shuffledIndices.ToArray());
    }

    void ShuffleList(List<int> list)
    {
        System.Random rng = new System.Random();
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            int temp = list[k];
            list[k] = list[n];
            list[n] = temp;
        }
    }

    [PunRPC]
    void SyncShuffle(int[] shuffledIndices)
    {
        StartCoroutine(RiffleShuffle(shuffledIndices));
    }

    IEnumerator RiffleShuffle(int[] shuffledIndices)
    {
        List<Transform> leftHalf = new List<Transform>();
        List<Transform> rightHalf = new List<Transform>();

        for (int i = 0; i < shuffledIndices.Length; i++)
        {
            if (i < shuffledIndices.Length / 2)
                leftHalf.Add(deck[shuffledIndices[i]]);
            else
                rightHalf.Add(deck[shuffledIndices[i]]);
        }

        // Animation des cartes en deux piles
        for (int i = 0; i < leftHalf.Count; i++)
        {
            leftHalf[i].DOMove(leftPile.position + new Vector3(0, i * 0.01f, 0), shuffleSpeed);
            rightHalf[i].DOMove(rightPile.position + new Vector3(0, i * 0.01f, 0), shuffleSpeed);
        }

        yield return new WaitForSeconds(shuffleSpeed + 0.2f);

        // Fusion des piles
        for (int i = 0; i < Mathf.Max(leftHalf.Count, rightHalf.Count); i++)
        {
            if (i < rightHalf.Count) 
            {
                rightHalf[i].DOMove(mergePosition.position + new Vector3(0, i * 0.001f, 0), shuffleSpeed);
                yield return new WaitForSeconds(shuffleSpeed / 2);
            }

            if (i < leftHalf.Count) 
            {
                leftHalf[i].DOMove(mergePosition.position + new Vector3(0, i * 0.001f, 0), shuffleSpeed);
                yield return new WaitForSeconds(shuffleSpeed / 2);
            }
        }

        yield return new WaitForSeconds(0.5f);
        StartCoroutine(DealCards());
    }
    

public IEnumerator DealCards()
{
    Debug.Log("🃏 Distribution des cartes...");

    players = PhotonNetwork.PlayerList;

    // Distribution des cartes aux joueurs
    for (int i = 0; i < players.Length; i++)
    {
        int playerID = players[i].ActorNumber;
        playerHands[playerID] = new List<Transform>();

        for (int j = 0; j < 2; j++) // Distribue 2 cartes par joueur
        {
            if (deck.Count == 0) break;
            int cardIndex = 0; // Toujours prendre la première carte après le mélange
            Transform card = deck[cardIndex];

            // Enlever la carte du deck après l'avoir choisie
            deck.RemoveAt(cardIndex);

            playerHands[playerID].Add(card);
            photonView.RPC("GiveCardToPlayer", RpcTarget.AllBuffered, card.GetComponent<PhotonView>().ViewID, playerID, j);
        }
    }
    playerHands[botID] = new List<Transform>();
    for (int j = 0; j < 2; j++)
    {
        if (deck.Count == 0)
            break;
        int cardIndex = 0;
        Transform card = deck[cardIndex];
        deck.RemoveAt(cardIndex);
        playerHands[botID].Add(card);
    }

    if (gameManager.bot != null)
    {
        List<Transform> botCards = new List<Transform>();
        for (int i = 0; i < 2; i++)
        {
            if (deck.Count == 0)
                break;
        Transform card = DrawCard();
        botCards.Add(card);
        Transform targetPos = (i == 0) ? botCardPos1 : botCardPos2;
        card.DOMove(targetPos.position, 0.5f);
        card.DORotate(targetPos.rotation.eulerAngles, 0.5f);
    }
    gameManager.SetBotCards(botCards);
    }
    StartGame();
    yield return null;
}
[PunRPC]
    IEnumerator GiveCardToPlayer(int cardViewID, int playerID, int cardIndex)
    {
        PhotonView cardPV = PhotonView.Find(cardViewID);
        if (cardPV == null)
        {
            Debug.LogError($"❌ ERREUR : Carte avec ViewID {cardViewID} introuvable !");
            yield break;
        }

        GameObject card = cardPV.gameObject;

        if (PhotonNetwork.LocalPlayer.ActorNumber == playerID)
        {
            Transform playerCardPosition = playerPositions[playerID - 1];
            float distance = 0.5f;
            Vector3 frontPosition = playerCardPosition.position + playerCardPosition.forward * distance;
            float cardSpacing = 0.1f;
            float heightOffset = 0.35f;
            float leftOffset = -0.05f;

            card.transform.DOMove(frontPosition + new Vector3(cardIndex * cardSpacing + leftOffset, heightOffset, 0), shuffleSpeed);
            card.transform.DORotate(playerCardPosition.rotation.eulerAngles, shuffleSpeed); 
        }

        if (!cardPV.IsMine)
        {
            card.GetComponent<Renderer>().material = cardBackMaterial;
        }
        yield return new WaitForSeconds(0.5f);
        }
        

        [PunRPC]
        public void DealCommunityCards()
        {
            if (!PhotonNetwork.IsMasterClient)
                return;
            for (int i = 0; i < 5; i++)
        {
        if (deck.Count == 0)
            break;
        Transform card = DrawCard();
        communityCards.Add(card);
        card.DOMove(communityCardPositions[i].position, 0.5f);
        card.DORotate(communityCardPositions[i].rotation.eulerAngles, 0.5f);

}

}




void StartCard()

{

if (PhotonNetwork.IsMasterClient)

        {

            StartRound(playerHands, PlayerUsing);

        }

}






/*

void StartGame()
{
    // Initialisation des cartes et de l'ordre de jeu
    currentPlayerIndex = 0; // Le premier joueur commence
    SetNextPlayerTurn();
}

void SetNextPlayerTurn()
{
    // Indiquer le prochain joueur dont c'est le tour
    uiPanel.SetActive(false);
    currentPlayerIndex = (currentPlayerIndex + 1) % players.Length;

    // On notifie tous les joueurs que c'est le tour du joueur suivant
    photonView.RPC("SyncTurn", RpcTarget.All, currentPlayerIndex);
}



    [PunRPC]
    IEnumerator GiveCardToPlayer(int cardViewID, int playerID, int cardIndex)
    {
        PhotonView cardPV = PhotonView.Find(cardViewID);
        if (cardPV == null)
        {
            Debug.LogError($"❌ ERREUR : Carte avec ViewID {cardViewID} introuvable !");
            yield break;
        }

        GameObject card = cardPV.gameObject;

        if (PhotonNetwork.LocalPlayer.ActorNumber == playerID)
        {
            Transform playerCardPosition = playerPositions[playerID - 1];
            float distance = 0.5f;
            Vector3 frontPosition = playerCardPosition.position + playerCardPosition.forward * distance;
            float cardSpacing = 0.1f;
            float heightOffset = 0.35f;
            float leftOffset = -0.05f;

            card.transform.DOMove(frontPosition + new Vector3(cardIndex * cardSpacing + leftOffset, heightOffset, 0), shuffleSpeed);
            card.transform.DORotate(playerCardPosition.rotation.eulerAngles, shuffleSpeed); 
        }

        if (!cardPV.IsMine)
        {
            card.GetComponent<Renderer>().material = cardBackMaterial;
        }
        yield return new WaitForSeconds(0.5f);

    // Vérifier si la somme des deux premières cartes dépasse 21 (busted)
        if (CalculateHandValue(playerID) > 21)
        {
            Debug.Log("Le joueur " + playerID + " a perdu avec ses deux premières cartes !");
            HandleBusted(playerID);
        }
        else
        {
        // Si le joueur n'est pas busted, il peut continuer à jouer
            Debug.Log("Le joueur " + playerID + " a une main de " + CalculateHandValue(playerID));
        }
    }

    [PunRPC]
    void SyncTurn(int playerIndex)
    {
    // Cette méthode est appelée par tous les joueurs
        if (PhotonNetwork.LocalPlayer.ActorNumber == PhotonNetwork.PlayerList[playerIndex].ActorNumber)
        {
            // Si c'est à ton tour, active les actions possibles
            EnablePlayerActions(true);
        
        }
        else
        {
        // Sinon, désactive les actions du joueur
            EnablePlayerActions(false);
        }
    }

    void EnablePlayerActions(bool enable)
    {
        // Active ou désactive les boutons/actions du joueur
        uiPanel.SetActive(enable);
        if(enable)
        {
            Debug.Log("ca rentre");
            if(Input.GetKeyDown(KeyCode.H))
            {
                Debug.Log("la aussi");
                OnHitButtonPressed();
            }
            if(Input.GetKeyDown(KeyCode.S))
            {
                OnStandButtonPressed();
            }
        }
    }
    int CalculateHandValue(int playerID)
    {
        int totalValue = 0;
        bool hasAce = false;

        foreach (Transform cardTransform in playerHands[playerID])
        {
            Card cardScript = cardTransform.GetComponent<Card>();
            totalValue += cardScript.GetCardValue();
            if (cardScript.cardType == Card.CardType.Ace) hasAce = true;
        }

        // Ajuster la valeur de l'As si le joueur dépasse 21
        if (totalValue > 21 && hasAce)
        {
            foreach (Transform cardTransform in playerHands[playerID])
            {
                Card cardScript = cardTransform.GetComponent<Card>();
                cardScript.AdjustAceValue(ref totalValue); // Ajuste la valeur de l'As
            }
        }

    return totalValue;
    }

    void HandleBusted(int playerID)
    {
         // Marquer le joueur comme ayant perdu et terminer son tour
        Debug.Log("Le joueur " + playerID + " a perdu (busted) !");
        // Tu peux ajouter ici une logique pour notifier tous les joueurs, désactiver le bouton "Tirer" du joueur, etc.
    }

    public void OnHitButtonPressed()
{
    Debug.Log("aussi");
    if (PhotonNetwork.LocalPlayer.ActorNumber == PhotonNetwork.PlayerList[currentPlayerIndex].ActorNumber)
    {
        Debug.Log("Il veut une carte");
            int cardIndex = 0; 
            Transform card = deck[cardIndex];

            // Enlever la carte du deck après l'avoir choisie
            deck.RemoveAt(cardIndex);

            playerHands[currentPlayerIndex].Add(card);
            photonView.RPC("GiveCardToPlayer", RpcTarget.AllBuffered, card.GetComponent<PhotonView>().ViewID, currentPlayerIndex, playerHands.Count-1);
        if (CalculateHandValue(PhotonNetwork.LocalPlayer.ActorNumber) > 21)
        {
            HandleBusted(PhotonNetwork.LocalPlayer.ActorNumber);
        }
        else
        {
            // Passer au joueur suivant
            SetNextPlayerTurn();
        }
        
    }
    else
    {
        Debug.Log("Ce n'est pas votre tour !");
    }
}

public void OnStandButtonPressed()
{
    if (PhotonNetwork.LocalPlayer.ActorNumber == PhotonNetwork.PlayerList[currentPlayerIndex].ActorNumber)
    {
    
        Debug.Log("Le joueur " + PhotonNetwork.LocalPlayer.ActorNumber + " choisit de rester.");
        // Passer au joueur suivant
        SetNextPlayerTurn();
    }
    else
    {
        Debug.Log("Ce n'est pas votre tour !");
    }
}

    
    public void StartShuffling()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            ShuffleDeck();
        }
    }
    
}*/

