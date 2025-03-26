using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    public float timer = 600f;
    public int totalCardsToCollect = 42;
    private int cardsCollected = 0;
    private bool isGameOver = false;

    private Dictionary<int, List<string>> playerInventories = new Dictionary<int, List<string>>();

    [Header("Affichage des Cartes Déposées")]
    public Transform cardDisplayArea; // Zone où les cartes seront affichées
    public GameObject cardPrefab; // Prefab du modèle de carte

    private Vector3 nextCardPosition; // Position de la prochaine carte déposée

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(TimerTick());
        }

        nextCardPosition = cardDisplayArea.position;
    }

    void Update()
    {
        if (!isGameOver)
        {
            UpdateTimerUI();
        }
    }

    IEnumerator TimerTick()
    {
        while (!isGameOver && timer > 0)
        {
            timer -= Time.deltaTime;
            photonView.RPC("UpdateTimer", RpcTarget.AllBuffered, timer);
            yield return null;
        }

        if (timer <= 0)
        {
            photonView.RPC("GameOverRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void UpdateTimer(float newTime)
    {
        timer = newTime;
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        //timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // ========= INVENTAIRE ET COLLECTE DE CARTES =========

    public void CollectCard(string cardName)
    {
        int playerID = PhotonNetwork.LocalPlayer.ActorNumber;

        if (!playerInventories.ContainsKey(playerID))
        {
            playerInventories[playerID] = new List<string>();
        }

        playerInventories[playerID].Add(cardName);
        photonView.RPC("IncrementCardCount", RpcTarget.AllBuffered, playerID, cardName);
    }

    [PunRPC]
    void IncrementCardCount(int playerID, string cardName)
    {
        cardsCollected++;
        Debug.Log($"Carte {cardName} collectée par joueur {playerID}.");

        if (cardsCollected >= totalCardsToCollect)
        {
            photonView.RPC("GameOverRPC", RpcTarget.AllBuffered);
        }
    }

    // ========= DÉPÔT DES CARTES =========

    public void DepositCards()
    {
        int playerID = PhotonNetwork.LocalPlayer.ActorNumber;

        if (playerInventories.ContainsKey(playerID) && playerInventories[playerID].Count > 0)
        {
            List<string> depositedCards = new List<string>(playerInventories[playerID]);
            playerInventories[playerID].Clear();

            photonView.RPC("ShowDepositedCards", RpcTarget.AllBuffered, playerID, depositedCards.ToArray());
        }
        else
        {
            Debug.Log("Aucune carte à déposer.");
        }
    }

    [PunRPC]
    void ShowDepositedCards(int playerID, string[] depositedCards)
    {
        foreach (string cardName in depositedCards)
        {
            Debug.Log($"Joueur {playerID} a déposé la carte : {cardName}");
            SpawnCard(cardName);
        }
    }

    void SpawnCard(string cardName)
    {
        GameObject newCard = Instantiate(cardPrefab, nextCardPosition, Quaternion.identity);
        newCard.name = cardName;

        // Ajouter un texte à la carte si elle a un TextMesh (pour afficher son nom)
        TextMesh textMesh = newCard.GetComponentInChildren<TextMesh>();
        if (textMesh != null)
        {
            textMesh.text = cardName;
        }

        // Décaler la prochaine carte en Z pour ne pas qu'elles se superposent
        nextCardPosition += new Vector3(0, 0, 1f);
    }

    [PunRPC]
    void GameOverRPC()
    {
        isGameOver = true;
        Debug.Log("Jeu terminé !");
    }
}




/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class GameManager : MonoBehaviourPunCallbacks
{
    public static GameManager instance;

    public float timer = 600f; // 10 minutes 
    public int totalCardsToCollect = 42; // Nombre total de cartes à trouver
    private int cardsCollected = 0;
    private bool isGameOver = false;

    private Dictionary<int, List<string>> playerInventories = new Dictionary<int, List<string>>(); // Inventaire par joueur

    void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            StartCoroutine(TimerTick());
        }
    }

    void Update()
    {
        if (!isGameOver)
        {
            UpdateTimerUI();
        }
    }

    IEnumerator TimerTick()
    {
        while (!isGameOver && timer > 0)
        {
            timer -= Time.deltaTime;
            photonView.RPC("UpdateTimer", RpcTarget.AllBuffered, timer);
            yield return null;
        }

        if (timer <= 0)
        {
            photonView.RPC("GameOverRPC", RpcTarget.AllBuffered);
        }
    }

    [PunRPC]
    void UpdateTimer(float newTime)
    {
        timer = newTime;
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        //timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    // ========= INVENTAIRE ET COLLECTE DE CARTES =========

    public void CollectCard(string cardName)
    {
        int playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        Debug.Log("Cest okay");
        if (!playerInventories.ContainsKey(playerID))
        {
            playerInventories[playerID] = new List<string>();
        }

        playerInventories[playerID].Add(cardName);
        photonView.RPC("IncrementCardCount", RpcTarget.AllBuffered, playerID, cardName);
    }

    [PunRPC]
    void IncrementCardCount(int playerID, string cardName)
    {
        cardsCollected++;
        Debug.Log($"Carte {cardName} collectée par joueur {playerID}.");

        if (cardsCollected >= totalCardsToCollect)
        {
            photonView.RPC("GameOverRPC", RpcTarget.AllBuffered);
        }
    }

    // ========= DÉPÔT DES CARTES =========

    public void DepositCards()
    {
        int playerID = PhotonNetwork.LocalPlayer.ActorNumber;

        if (playerInventories.ContainsKey(playerID) && playerInventories[playerID].Count > 0)
        {
            List<string> depositedCards = new List<string>(playerInventories[playerID]);
            playerInventories[playerID].Clear();

            photonView.RPC("ShowDepositedCards", RpcTarget.AllBuffered, playerID, depositedCards.ToArray());
        }
        else
        {
            Debug.Log("Aucune carte à déposer.");
        }
    }

    [PunRPC]
    void ShowDepositedCards(int playerID, string[] depositedCards)
    {
        foreach (string cardName in depositedCards)
        {
            Debug.Log($"Joueur {playerID} a déposé la carte : {cardName}");
            // Ici, on pourrait instancier un objet carte visuellement dans le hub.
        }
    }

    [PunRPC]
    void GameOverRPC()
    {
        isGameOver = true;
        Debug.Log("Jeu terminé !");
    }
}*/



/*using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;  

public class GameManager : MonoBehaviourPunCallbacks
{
    public float timer = 600f; // 10 minutes 
    public int totalCardsToCollect = 42; // Nombre total de cartes à trouver
    private int cardsCollected = 0;
    private bool isGameOver = false;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            // Si c'est le MasterClient, on commence la gestion du timer et des cartes
            StartCoroutine(TimerTick());
        }
    }

    void Update()
    {
        if (!isGameOver)
        {
            // Le timer est synchronisé avec tous les clients via le MasterClient
            UpdateTimerUI();
        }
    }

    IEnumerator TimerTick()
    {
        while (!isGameOver && timer > 0)
        {
            timer -= Time.deltaTime;
            photonView.RPC("UpdateTimer", RpcTarget.AllBuffered, timer); // Synchroniser le timer avec tous les clients
            yield return null;
        }

        if (timer <= 0)
        {
            photonView.RPC("GameOverRPC", RpcTarget.AllBuffered); // Synchroniser la fin du jeu sur tous les clients
        }
    }

    [PunRPC]
    void UpdateTimer(float newTime)
    {
        timer = newTime; // Mettre à jour le timer sur tous les clients
    }

    void UpdateTimerUI()
    {
        int minutes = Mathf.FloorToInt(timer / 60);
        int seconds = Mathf.FloorToInt(timer % 60);
        //timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }

    public void CollectCard()
    {
        if (!isGameOver)
        {
            photonView.RPC("IncrementCardCount", RpcTarget.AllBuffered); // Incrémenter le nombre de cartes collectées pour tous les joueurs
        }
    }

    [PunRPC]
    void IncrementCardCount()
    {
        cardsCollected++;
        if (cardsCollected >= totalCardsToCollect)
        {
            photonView.RPC("GameOverRPC", RpcTarget.AllBuffered); // Synchroniser la fin du jeu sur tous les clients
        }
    }

    [PunRPC]
    void GameOverRPC()
    {
        isGameOver = true;
        Debug.Log("Jeu terminé!");
        // Vous pouvez ici ajouter une fonction pour afficher un message de fin de jeu ou revenir au menu
    }
}*/
