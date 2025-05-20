using UnityEngine;
using System.Collections.Generic;

public class HubManager : MonoBehaviour
{
    private HashSet<string> playersBackInHub = new HashSet<string>();
    public string levelIDJustCompleted; // À renseigner si ce HubManager est spécifique à un niveau
    private Vector3 spawnPos = new Vector3(66, -43, -59);
    private int spacing = 1;

    void Start()
    {
        playersBackInHub.Clear();
        
        foreach (string cardName in CardCollectionManager.Instance.GetCollectedCards())
        {
            GameObject prefab = CardsManager.instance.GetCardPrefabByName(cardName);
            if (prefab != null)
            {
                Instantiate(prefab, spawnPos, Quaternion.identity);
                spawnPos += new Vector3(spacing, 0, 0);
            }
        }
        foreach (string cardName in CardCollectionManager.Instance.GetCollectedCards())
        {
            GameObject prefab = CardsManager.instance.GetCardPrefabByName(cardName);
            if (prefab != null)
            {
                // Place les cartes quelque part sur la table
                Instantiate(prefab, new Vector3(66, -43, -59), Quaternion.identity);
            }
        }
    }

    public void PlayerReturned(string userId)
    {
        if (!playersBackInHub.Contains(userId))
        {
            playersBackInHub.Add(userId);
            Debug.Log($"[HubManager] Joueur revenu au hub : {userId}");

            // Quand tous les joueurs sont revenus
            if (playersBackInHub.Count >= Photon.Pun.PhotonNetwork.CurrentRoom.PlayerCount)
            {
                Debug.Log("[HubManager] Tous les joueurs sont revenus au hub !");
            }
        }
    }
}