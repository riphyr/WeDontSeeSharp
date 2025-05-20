using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardsManager : MonoBehaviourPunCallbacks
{
    public static CardsManager instance;

    public float timer = 6f;
    public int totalCardsToCollect = 42;
    private int cardsCollected = 0;
    private bool isGameOver = false;
    public List<string> cards = new List<string>();
    public Studio.Consigne consigne;

    private Dictionary<int, List<string>> playerInventories = new Dictionary<int, List<string>>();

    //[Header("Affichage des Cartes Déposées")]
    //public static List<CardData> collectedCards = new List<CardData>(); 
    //public Transform cardDisplayArea; // Zone où les cartes seront affichées
    //public GameObject cardPrefab; // Prefab du modèle de carte

    //private Vector3 nextCardPosition; // Position de la prochaine carte déposée

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
        consigne.Text();
        //nextCardPosition = cardDisplayArea.position;
    }

    void Update()
    {
        if (!isGameOver)
        {
            UpdateTimerUI();
        }
        if (cardsCollected >= totalCardsToCollect)
        {
            photonView.RPC("GameOverRPC", RpcTarget.AllBuffered);
            ShowVictoryMessage(); // Ajouté ici
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
        Debug.Log("okii");
        int playerID = PhotonNetwork.LocalPlayer.ActorNumber;

        if (!playerInventories.ContainsKey(playerID))
        {
            playerInventories[playerID] = new List<string>();
        }
        cards.Add(cardName);
        playerInventories[playerID].Add(cardName);
        Debug.Log(playerInventories[playerID].Count);
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
        Debug.Log($"{cardsCollected} CARDS COLLECTED");
    }

    [PunRPC]
    void GameOverRPC()
    {
        isGameOver = true;
    }
    public int GetPlayerCardCount()
    {
        int playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        if (playerInventories.ContainsKey(playerID))
            return playerInventories[playerID].Count;
        return 0;
    }
    void ShowVictoryMessage()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            photonView.RPC("ShowWinMessage", RpcTarget.All);
        }
    }

    [PunRPC]
    void ShowWinMessage()
    {
        // Tu peux créer un objet texte dans ton UI pour afficher ça
        Debug.Log("🎉 Félicitations ! Toutes les cartes ont été récupérées !");
        // Active un TextMeshPro si tu veux l'afficher à l'écran
    }
    [SerializeField] private List<GameObject> allCardPrefabs; // Assigne-les dans l'inspector

    public List<string> GetInventory(int playerID)
    {
        if (playerInventories.TryGetValue(playerID, out List<string> inventory))
        {
            return new List<string>(inventory); // retourne une copie
        }
        return new List<string>();
    }

    public void ClearInventory(int playerID)
    {
        if (playerInventories.ContainsKey(playerID))
        {
            playerInventories[playerID].Clear();
        }
    }

    public GameObject GetCardPrefabByName(string cardName)
    {
        return allCardPrefabs.Find(card => card.name == cardName);
    }



}