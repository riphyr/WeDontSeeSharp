using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;


// SCRIPT PRESENT DANS LE HUB !!!!!
public class CardManager : MonoBehaviourPunCallbacks
{
    public static CardManager instance;
    
    public List<string> collectedCardIDs = new List<string>(); 
    public List<string> depositedCardIDs = new List<string>();
    private int cardsCollected = 0;
    [SerializeField] private List<GameObject> allCardPrefabs; // Assigne-les dans l'inspector
    public List<string> persistentDepositedCards = new List<string>();


    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(this.gameObject); // ← OK !
        }
        else
        {
            Destroy(gameObject); // ← ⚠ Peut se déclencher dans certaines conditions
        }
    }

    // ========= INVENTAIRE ET COLLECTE DE CARTES =========

    public void CollectCard(string cardName)
    {
        Debug.Log("CA RENTRE ??????");
        if (!collectedCardIDs.Contains(cardName))
        {
            collectedCardIDs.Add(cardName);
            Debug.Log($"Carte collectée : {cardName}");
            cardsCollected++;
        }
        Debug.Log($"Carte collectée : {cardName} | Total: {collectedCardIDs.Count}");
    }
    public void DepositCard(string cardName)
    {
        if (collectedCardIDs.Contains(cardName) && !depositedCardIDs.Contains(cardName))
        {
            depositedCardIDs.Add(cardName);
            Debug.Log($"Carte déposée : {cardName}");
            photonView.RPC("SyncCardDeposit", RpcTarget.Others, cardName);
        }
    }
    [PunRPC]
    public void SyncCardDeposit(string cardID)
    {
        if (!depositedCardIDs.Contains(cardID))
        {
            depositedCardIDs.Add(cardID);
        }
    }

    public bool HasCollected(string cardID) => collectedCardIDs.Contains(cardID);
    public bool HasDeposited(string cardID) => depositedCardIDs.Contains(cardID);

    public GameObject GetCardPrefabByName(string cardName)
    {
        return allCardPrefabs.Find(card => card.name == cardName);
    }
    
    public void ClearInventory()
    {
        collectedCardIDs.Clear();
    }



}
