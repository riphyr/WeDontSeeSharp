using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CardPickup : MonoBehaviourPunCallbacks
{
    public string cardName; // doit être unique dans la scène
    private bool isCollected = false;

    private void Start()
    {
        // Vérifie si la carte a déjà été collectée
        if (CardCollectionManager.Instance.IsCardCollected(cardName))
        {
            Destroy(gameObject);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player") || isCollected) return;

        int playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        CardInventoryManager.Instance.AddCard(playerID, cardName);

        isCollected = true;

        // Appel RPC pour supprimer la carte chez tout le monde
        photonView.RPC("RPC_CollectCard", RpcTarget.AllBuffered, cardName);
    }

    [PunRPC]
    void RPC_CollectCard(string cardID)
    {
        CardCollectionManager.Instance.MarkCardAsCollected(cardID);
        Destroy(gameObject); // Supprime l'objet dans toutes les scènes
    }
}

