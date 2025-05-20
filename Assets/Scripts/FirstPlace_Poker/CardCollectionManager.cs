using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardCollectionManager : MonoBehaviourPunCallbacks
{
    public static CardCollectionManager Instance;

    private HashSet<string> collectedCards = new HashSet<string>(); // Les cartes collectées localement

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persister entre les scènes
        }
        else
        {
            Destroy(gameObject); // Si une instance existe déjà, détruire cette nouvelle
        }
    }

    // Ajouter une carte à la collection du joueur local
    public void MarkCardAsCollected(string cardID)
    {
        if (!collectedCards.Contains(cardID))
        {
            collectedCards.Add(cardID);
            photonView.RPC("SyncCardCollection", RpcTarget.Others, cardID); // Synchroniser avec les autres joueurs
        }
    }

    // Vérifier si une carte a été collectée
    public bool IsCardCollected(string cardID)
    {
        return collectedCards.Contains(cardID);
    }

    // Clear les cartes collectées (réinitialisation)
    public void ClearAllCollectedCards()
    {
        collectedCards.Clear();
        photonView.RPC("ClearAllCollectedCardsOnOthers", RpcTarget.Others); // Clear chez les autres joueurs aussi
    }

    // RPC pour synchroniser la collection des cartes avec les autres joueurs
    [PunRPC]
    public void SyncCardCollection(string cardID)
    {
        collectedCards.Add(cardID); // Ajouter la carte à la collection distante
    }

    // RPC pour vider la collection des cartes des autres joueurs
    [PunRPC]
    public void ClearAllCollectedCardsOnOthers()
    {
        collectedCards.Clear(); // Vider la collection à distance
    }

    // Getter pour obtenir les cartes collectées localement
    public List<string> GetCollectedCards()
    {
        return new List<string>(collectedCards);
    }
}