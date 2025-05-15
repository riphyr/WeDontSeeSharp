using UnityEngine;
using Photon.Pun;

public class DepositTable : MonoBehaviourPunCallbacks
{
    //public Transform dropPoint; // Point où les cartes vont apparaître
    public float spacing = 0.5f; // Espace entre les cartes
    
    [SerializeField] private Transform[] playerDropPoints; // assignés dans l'inspector

    private Transform GetPlayerDropPoint()
    {
        //int playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        int index = PhotonNetwork.LocalPlayer.ActorNumber - 1;
        if (index >= playerDropPoints.Length)
            index = playerDropPoints.Length - 1; // fallback

        return playerDropPoints[index];
    }


    public void DepositCards()
    {
        int playerID = PhotonNetwork.LocalPlayer.ActorNumber;
        var inventory = CardsManager.instance.GetInventory(playerID);

        if (inventory == null || inventory.Count == 0)
        {
            Debug.Log("Aucune carte à déposer.");
            return;
        }
        Transform dropPoint = GetPlayerDropPoint();
        Vector3 spawnPos = dropPoint.position;
        //GameObject deckprefab = Resources.Load<GameObject>("DeckCard1");
        //Vector3 spawnPos = dropPoint.position;
        foreach (string cardName in inventory)
        {
            Debug.Log($"Card {cardName} deposer.");
            GameObject prefab =  CardsManager.instance.GetCardPrefabByName(cardName);
            if (prefab != null)
            {
                Debug.Log($"Bah c'est bien deposer {prefab.name}");
                PhotonNetwork.Instantiate(prefab.name, spawnPos, Quaternion.identity);
                //CardCollectionManager.Instance.AddCard(cardName);
                spawnPos += new Vector3(spacing, 0, 0); // décaler à droite
            }
            else
            {
                Debug.Log($"Bah c'est nul :(");
            }
        }

        // Vider l'inventaire du joueur après dépôt
        CardsManager.instance.ClearInventory(playerID);
    }
}