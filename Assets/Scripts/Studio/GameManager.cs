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
    public static List<CardData> collectedCards = new List<CardData>(); 
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

        //nextCardPosition = cardDisplayArea.position;
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
        Debug.Log("okii");
        int playerID = PhotonNetwork.LocalPlayer.ActorNumber;

        if (!playerInventories.ContainsKey(playerID))
        {
            playerInventories[playerID] = new List<string>();
        }
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
        Debug.Log("Jeu terminé !");
    }
}
