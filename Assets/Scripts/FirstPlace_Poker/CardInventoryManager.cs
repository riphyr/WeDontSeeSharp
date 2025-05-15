using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class CardInventoryManager : MonoBehaviourPunCallbacks
{
    public static CardInventoryManager Instance;

    private Dictionary<int, List<string>> playerInventories = new Dictionary<int, List<string>>();

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Persiste entre les scènes
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCard(int playerID, string cardName)
    {
        if (!playerInventories.ContainsKey(playerID))
        {
            playerInventories[playerID] = new List<string>();
        }

        if (!playerInventories[playerID].Contains(cardName))
        {
            playerInventories[playerID].Add(cardName);
            Debug.Log($"Carte '{cardName}' ajoutée à l'inventaire du joueur {playerID}.");
        }
    }

    public List<string> GetInventory(int playerID)
    {
        if (playerInventories.TryGetValue(playerID, out var inventory))
            return new List<string>(inventory);
        return new List<string>();
    }

    public void ClearInventory(int playerID)
    {
        if (playerInventories.ContainsKey(playerID))
            playerInventories[playerID].Clear();
    }
}

